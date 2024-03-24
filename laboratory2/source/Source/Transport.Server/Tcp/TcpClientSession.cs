using System.Net.Sockets;

namespace Transport.Server.Tcp
{
    public class TcpClientSession : IDisposable
    {
        private readonly Socket clientSocket;
        private readonly Action<TcpClientSession, Memory<byte>> receiveHandler;

        private SocketAsyncEventArgs receiveArgs;
        private byte[] receiveBuffer;

        private SocketAsyncEventArgs sendArgs;
        private byte[] sendBuffer;

        private object sendLock = new object();

        public TcpClientSession(Socket clientSocket, Action<TcpClientSession, Memory<byte>> receiveHandler)
        {
            this.clientSocket = clientSocket;
            this.receiveHandler = receiveHandler;

            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public void Connect()
        {
            receiveArgs = new SocketAsyncEventArgs();
            sendArgs = new SocketAsyncEventArgs();

            receiveBuffer = new byte[1024];
            sendBuffer = new byte[1024];

            sendArgs.Completed += HandleCompletedAsyncOperation;
            receiveArgs.Completed += HandleCompletedAsyncOperation;

            Console.WriteLine("Client {0} has been connected", Id);

            TryReceive();
        }

        public void Disconnect()
        {
            receiveArgs.Completed -= HandleCompletedAsyncOperation;
            sendArgs.Completed -= HandleCompletedAsyncOperation;

            receiveArgs.Dispose();
            sendArgs.Dispose();

            clientSocket.Dispose();

            Console.WriteLine("Client {0} has been disconnected", Id);
        }

        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer)
        {
            if (buffer.IsEmpty)
            {
                return ValueTask.FromResult(0);
            }

            return clientSocket.SendAsync(buffer);
        }

        private void HandleCompletedAsyncOperation(object? sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    if (HandleReceive())
                    {
                        TryReceive();
                    }

                    break;
                case SocketAsyncOperation.Send:

                    break;
                default:
                    throw new InvalidOperationException("Hit unreachable path");
            }
        }

        private void TryReceive()
        {
            // If receive is sync OR receive operation IS NOT last
            var shouldContinueSync = true;

            while (shouldContinueSync)
            {
                shouldContinueSync = false;

                try
                {
                    receiveArgs.SetBuffer(receiveBuffer);

                    if (!clientSocket.ReceiveAsync(receiveArgs))
                    {
                        shouldContinueSync = HandleReceive();
                    }
                }
                catch (ObjectDisposedException) { }
            }
        }



        private bool HandleReceive()
        {
            var args = receiveArgs;
            var size = args.BytesTransferred;

            if (size > 0)
            {
                receiveHandler(this, receiveBuffer.AsMemory(0, size));
            }


            if (args.SocketError == SocketError.Success)
            {
                if (size > 0)
                {
                    // Everything is ok - schedule next reading
                    return true;
                }
                else
                {
                    // End of stream
                    Console.WriteLine("Client {0} disconnected", Id);

                    Disconnect();

                    return false;
                }
            }
            else
            {
                Console.WriteLine("Error {0} occurred for client {1}", args.SocketError, Id);

                Disconnect();

                return false;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
