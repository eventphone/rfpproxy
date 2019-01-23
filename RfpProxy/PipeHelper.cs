using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public static class PipeHelper
    {
        public static async Task FillPipeAsync(Socket socket, PipeWriter writer, CancellationToken cancellationToken)
        {
            try
            {
                while (socket.Connected)
                {
                    var memory = writer.GetMemory(512);
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                        break;

                    writer.Advance(bytesRead);

                    var result = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

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
    }
}