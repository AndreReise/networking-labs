using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Transport.Server.Tcp
{
    public class TcpServer : IDisposable
    {
        private readonly Socket acceptorSocket;
        private readonly Action<TcpClientSession, Memory<byte>> receiveHandler;

        private SocketAsyncEventArgs acceptorEventArgs;

        private readonly ConcurrentDictionary<Guid, TcpClientSession> sessions = new();

        public TcpServer(AddressFamily addressFamily, EndPoint endpoint, Action<TcpClientSession, Memory<byte>> receiveHandler)
        {
            acceptorSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.receiveHandler = receiveHandler;

            acceptorSocket.Bind(endpoint);
            acceptorSocket.Listen();
        }

        public void Start()
        {
            acceptorEventArgs = new SocketAsyncEventArgs();
            acceptorEventArgs.Completed += HandleAcceptComplete;

            acceptorSocket.AcceptAsync(acceptorEventArgs);
        }

        public void Dispose()
        {
            CloseSessions();

            acceptorEventArgs.Completed -= HandleAcceptComplete;
            acceptorSocket.Dispose();

            acceptorSocket.Close();
        }

        private void CloseSessions()
        {
            foreach (var session in sessions.Values)
            {
                session.Disconnect();
            }
        }

        private void StartAccept(SocketAsyncEventArgs e)
        {
            // Socket must be cleared since the context object is being reused
            e.AcceptSocket = null;

            // Async accept a new client connection
            if (!acceptorSocket.AcceptAsync(e))
            {
                // Operation completed synchronously - begin processing
                ProcessAccept(e);
            }

        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Create a new session to register
                var session = new TcpClientSession(e.AcceptSocket!, receiveHandler);

                // Register the session
                RegisterSession(session);

                // Connect new session
                session.Connect();
            }
            else
            {
                Console.WriteLine("Error occurred while accepting");
            }

            // Accept the next client connection
            StartAccept(acceptorEventArgs);
        }

        internal void RegisterSession(TcpClientSession session)
        {
            // Register a new session
            sessions.TryAdd(session.Id, session);
        }

        private void HandleAcceptComplete(object? sender, SocketAsyncEventArgs? args)
        {
            ProcessAccept(args);
        }
    }
}
