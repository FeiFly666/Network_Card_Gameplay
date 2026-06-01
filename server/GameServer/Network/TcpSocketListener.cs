using GameServer.Manager;
using NetMessage;
using System.Net;
using System.Net.Sockets;

namespace GameServer.Network
{
    public class TcpSocketListener
    {
        private int nextID = 0;
        public readonly Dictionary<int, ClientSession> Clients = new Dictionary<int, ClientSession>();
        private readonly Dictionary<Socket, int> m_connectedClients = new Dictionary<Socket, int>();
        private readonly Socket m_listenSocket;
        private readonly SocketAsyncEventArgs m_acceptArgs;

        private List<ClientSession> m_timeoutClients = new List<ClientSession>();
        public TcpSocketListener()
        {
            m_listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_acceptArgs = new SocketAsyncEventArgs();
            m_acceptArgs.Completed += OnAcceptComplete;
        }

        public void Start(string ip, int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            m_listenSocket.Bind(localEndPoint);
            m_listenSocket.Listen(100);

            Console.WriteLine($"TCP server started: {localEndPoint}");

            StartAccept();
        }

        private void StartAccept()
        {
            m_acceptArgs.AcceptSocket = null;

            bool pending = m_listenSocket.AcceptAsync(m_acceptArgs);
            if (!pending)
            {
                OnAcceptComplete(m_listenSocket, m_acceptArgs);
            }
        }

        private void OnAcceptComplete(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.AcceptSocket == null)
            {
                StartAccept();
                return;
            }

            Socket clientSocket = e.AcceptSocket;
            int clientID = ++nextID;
            ClientSession session = new ClientSession(clientID, clientSocket);

            Clients[clientID] = session;
            m_connectedClients[clientSocket] = clientID;

            session.OnDisconnected += RemoveClientSession;

            Console.WriteLine($"Client connected: id={clientID}, remote={session.RemoteEndPoint}");

            session.StartReceive();
            StartAccept();
        }

        public void CheckSession()
        {
            m_timeoutClients.Clear();
            foreach(var client in Clients.Values.ToList())
            {
                if(client.IsTimeOut(10))
                {
                    client.isOnline = false;
                    Console.WriteLine($"tcp listener: client {client.ClientID} time out and will be closed");
                    m_timeoutClients.Add(client);
                }
            }
            foreach(var client in m_timeoutClients)
            {
                client.Close();
            }
        }

        public bool TryGetClient(int clientID, out ClientSession? session)
        {
            return Clients.TryGetValue(clientID, out session);
        }

        private void RemoveClientSession(ClientSession session)
        {
            if(m_connectedClients.ContainsKey(session.ClientSocket))
            {
                Console.WriteLine($"client disconnected: id={m_connectedClients[session.ClientSocket]}");

                if (session.ClientSocket == null) return;
                if (!m_connectedClients.ContainsKey(session.ClientSocket)) return;



                if (Clients.ContainsKey(m_connectedClients[session.ClientSocket]))
                {
                    Clients.Remove(m_connectedClients[session.ClientSocket]);
                }
                if(session.player != null)//已经登录
                {
                    if (OnlinePlayerManager.Instance.IsPlayerOnline(session.player.id))
                    {
                        if(OnlinePlayerManager.Instance.TryGetClientSession(session.player.id,out var onlineSesion))
                        {
                            if(onlineSesion == session)
                            {
                                OnlinePlayerManager.Instance.RemoveOnlinePlayer(session.player.id);
                            }
                        }
                        //OnlinePlayerManager.Instance.RemoveOnlinePlayer(session.player.id);
                    }
                    if (RoomManager.Instance.IsPlayerInRoom(session.player.id, session.ClientID))
                    {
                        //TODO:退出房间
                        UserExitRequest request = new UserExitRequest();
                        RoomManager.Instance.OnUserExit(session, request);

                        //m_socketToID.Remove(session.ClientID);
                    }
                }
                m_connectedClients.Remove(session.ClientSocket);
            }
        }
    }
}
