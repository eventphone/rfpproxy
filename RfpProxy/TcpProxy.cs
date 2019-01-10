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
        private readonly string _server;
        private readonly int _port;
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts;


        protected TcpProxy(int listenPort, string server, int serverPort)
        {
            _server = server;
            _port = serverPort;
            _listener = TcpListener.Create(listenPort);
            _cts = new CancellationTokenSource();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                _listener.Start();
                var accept = _listener.AcceptTcpClientAsync();
                using (var combined = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
                {
                    var tasks = new HashSet<Task>
                    {
                        accept,
                        Task.Delay(Timeout.Infinite, combined.Token)
                    };
                    while (!combined.IsCancellationRequested)
                    {
                        await Task.WhenAny(tasks).ConfigureAwait(false);
                        if (cancellationToken.IsCancellationRequested)
                            return;
                        if (accept.IsCompleted)
                        {
                            var client = await accept.ConfigureAwait(false);
                            tasks.Add(HandleClientAsync(client, combined.Token));

                            tasks.Remove(accept);
                            accept = _listener.AcceptTcpClientAsync();
                            tasks.Add(accept);
                        }
                        else
                        {
                            //client finished
                            foreach (var task in tasks.Where(x => x.IsCompleted).ToList())
                            {
                                await task.ConfigureAwait(false);
                                tasks.Remove(task);
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            using (var server = new TcpClient(AddressFamily.InterNetworkV6) {Client = {DualMode = true}})
            {
                await server.ConnectAsync(_server, _port).ConfigureAwait(false);
                var clientData = OnClientConnected(client, server);

                var clientPipe = new Pipe();
                var fillClientPipe = FillPipeAsync(client.Client, clientPipe.Writer, cts.Token);
                var readClientPipe = ReadFromClientAsync(clientData, clientPipe.Reader, cancellationToken);

                var serverPipe = new Pipe();
                var fillServerPipe = FillPipeAsync(server.Client, serverPipe.Writer, cts.Token);
                var readServerPipe = ReadFromServerAsync(clientData, serverPipe.Reader, cancellationToken);

                await Task.WhenAny(fillClientPipe, readClientPipe, fillServerPipe, readServerPipe).ConfigureAwait(false);
                cts.Cancel();
            }
        }

        private async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken cancellationToken)
        {
            try
            {
                while (socket.Connected)
                {
                    var memory = writer.GetMemory(512);
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                    if (bytesRead == 0)
                        break;

                    writer.Advance(bytesRead);

                    var result = await writer.FlushAsync(cancellationToken);

                    if (result.IsCompleted)
                        break;
                }
                writer.Complete();
            }
            catch (OperationCanceledException ex)
            {
                socket.Close();
                writer.Complete(ex);
            }
            catch (SocketException ex)
            {
                socket.Close();
                writer.Complete(ex);
            }
        }

        protected abstract Task ReadFromClientAsync(T clientData, PipeReader client, CancellationToken cancellationToken);
        protected abstract Task ReadFromServerAsync(T clientData, PipeReader server, CancellationToken cancellationToken);

        protected abstract T OnClientConnected(TcpClient client, TcpClient server);

        public void Stop()
        {
            _cts.Cancel();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}