using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public abstract class TcpProxy<T>:IDisposable
    {
        private readonly int _listenPort;
        private readonly string _server;
        private readonly int _port;
        private TcpListener _listener;


        protected TcpProxy(int listenPort, string server, int serverPort)
        {
            _listenPort = listenPort;
            _server = server;
            _port = serverPort;
        }

        public virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            _listener = CreateListener(_listenPort);
            try
            {
                _listener.Start();
                var accept = _listener.AcceptTcpClientAsync(cancellationToken).AsTask();
                var tasks = new HashSet<Task>
                {
                    accept
                };
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.WhenAny(tasks).ConfigureAwait(false);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (accept.IsCompleted)
                    {
                        var client = await accept.ConfigureAwait(false);
                        tasks.Add(HandleClientAsync(client, cancellationToken));

                        tasks.Remove(accept);
                        accept = _listener.AcceptTcpClientAsync(cancellationToken).AsTask();
                        tasks.Add(accept);
                    }
                    else
                    {
                        //client finished
                        foreach (var task in tasks.Where(x => x.IsCompleted).ToList())
                        {
                            try
                            {
                                await task.ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("RFP Connection failed");
                                Console.WriteLine(ex);
                            }
                            tasks.Remove(task);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("cancelled in TcpProxy.RunAsync");
            }
            finally
            {
                _listener.Stop();
            }
        }

        protected virtual TcpListener CreateListener(int port)
        {
            return TcpListener.Create(port);
        }

        protected virtual TcpClient ConnectToServer(TcpClient client)
        {
            return new TcpClient(AddressFamily.InterNetworkV6){Client = {DualMode = true}};
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                using (client)
                using (var server = ConnectToServer(client))
                {
                    await server.ConnectAsync(_server, _port, cts.Token).ConfigureAwait(false);
                    var clientData = OnClientConnected(client, server);
                    var tasks = new List<Task>();
                    try
                    {
                        var clientPipe = new Pipe();
                        var fillClientPipe = PipeHelper.FillPipeAsync(client.Client, clientPipe.Writer, cts.Token);
                        tasks.Add(fillClientPipe);
                        var readClientPipe = ReadFromClientAsync(clientData, clientPipe.Reader, cts.Token);
                        tasks.Add(readClientPipe);

                        var serverPipe = new Pipe();
                        var fillServerPipe = PipeHelper.FillPipeAsync(server.Client, serverPipe.Writer, cts.Token);
                        tasks.Add(fillServerPipe);
                        var readServerPipe = ReadFromServerAsync(clientData, serverPipe.Reader, cts.Token);
                        tasks.Add(readServerPipe);
                        var t = await Task.WhenAny(tasks).ConfigureAwait(false);
                        await t;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{clientData}] HandleClient failed\n{ex}");
                    }
                    finally
                    {
                        //delay cancellation to allow processing of pending packets
                        cts.CancelAfter(100);
                        try
                        {
                            await Task.WhenAll(tasks);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"[{clientData}] Exception during teardown\n{ex}");
                        }
                        finally
                        {
                            OnClientDisconnected(clientData);
                        }                        
                    }
                }
        }

        protected abstract Task ReadFromClientAsync(T clientData, PipeReader client, CancellationToken cancellationToken);

        protected abstract Task ReadFromServerAsync(T clientData, PipeReader server, CancellationToken cancellationToken);

        protected abstract T OnClientConnected(TcpClient client, TcpClient server);

        protected abstract void OnClientDisconnected(T client);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _listener?.Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}