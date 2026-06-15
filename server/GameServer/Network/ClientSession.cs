using NetMessage;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Common.Network;
using Common.Model;
using GameServer.Manager;

namespace GameServer.Network
{
    public class ClientSession : ISession
    {
        private const int MaxBytes = 1024 * 1024;

        private readonly int m_clientID;
        private readonly Socket m_clientSocket;
        private readonly SocketAsyncEventArgs m_receiveArgs;
        private readonly SocketAsyncEventArgs m_sendArgs;

        private readonly object m_sendLock = new object();
        private Queue<byte[]> m_sendQueue = new Queue<byte[]>();
        private bool isSending = false;

        private readonly MemoryStream m_buffer;
        private int readOffset;

        public bool isOnline = true;
        private float lastCommunicateTime;

        private int m_closed = 0;
        public bool IsClosed
        {
            get { return Interlocked.CompareExchange(ref m_closed, 0, 0) == 1; }
        }

        public Player player;

        public int ClientID
        {
            get { return m_clientID; }
        }

        public Socket ClientSocket
        {
            get { return m_clientSocket; }
        }

        public IPEndPoint? RemoteEndPoint { get; }

        public Action<ClientSession> OnDisconnected;

        private Message heartMessage;

        public ClientSession(int clientID, Socket client)
        {
            m_clientID = clientID;
            m_clientSocket = client;
            RemoteEndPoint = client.RemoteEndPoint as IPEndPoint;

            m_receiveArgs = new SocketAsyncEventArgs();
            m_receiveArgs.Completed += OnReceiveComplete;
            m_receiveArgs.SetBuffer(new byte[MaxBytes], 0, MaxBytes);

            m_sendArgs = new SocketAsyncEventArgs();
            m_sendArgs.Completed += OnSendComplete;
            //m_sendArgs.SetBuffer(new byte[MaxBytes], 0, MaxBytes);

            m_buffer = new MemoryStream(MaxBytes);
            m_buffer.Position = 0;

            lastCommunicateTime = Time.time;

            heartMessage = new Message();
            heartMessage.Response = new ResponseMessage();
            heartMessage.Response.Heart = new HeartCheckResponse();

            MessageDispatcher.Instance.Subscribe<HeartCheckRequest>(OnClientSendHeart);
        }

        public void StartReceive()
        {
            if (!m_clientSocket.Connected)
            {
                return;
            }

            bool pending = m_clientSocket.ReceiveAsync(m_receiveArgs);
            if (!pending)
            {
                OnReceiveComplete(m_clientSocket, m_receiveArgs);
            }
        }

        private void OnReceiveComplete(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)//Ω” ’ ß∞‹
            {
                Close();
                return;
            }

            //Console.WriteLine($"Receive from client {ClientID}({RemoteEndPoint}), bytes: {e.BytesTransferred}");

            //ºÏ≤‚ «∑Ò“Á≥ˆ

            if(e.BytesTransferred + m_buffer.Position  > m_buffer.Capacity)
            {
                Close();
                throw new Exception("overflow");
            }
            //Œ¥“Á≥ˆ–¥»Î
            m_buffer.Write(e.Buffer, 0, e.BytesTransferred);

            //ParsePackage();

            MessageHandler.ParseAndDispatch(this,m_buffer);
            
            lastCommunicateTime= Time.time;
            
            StartReceive();
        }
        public bool IsTimeOut(float timeout)
        {
            return Time.time - lastCommunicateTime >= timeout;
        }
        private void OnClientSendHeart(ISession session, HeartCheckRequest request)
        {
            FriendShipManager.Instance.AppendDirtyFriendData(this, heartMessage);
            this.Send(heartMessage);
            heartMessage.Response.friendList = null;
        }

        public void Send(Message message)
        {
            if(IsClosed) return;

            byte[] data = null;

            data = MessageHandler.Package(message);
           

            if (data == null) return;


            lock (m_sendLock)
            {
                m_sendQueue.Enqueue(data);

                if (isSending) return;

                isSending = true;
            }

            StartSend();
        }
        private void StartSend()
        {
            if (!m_clientSocket.Connected)
            {
                m_sendQueue.Clear();
                isSending = false;
                return;
            }

            byte[] data;

            lock (m_sendLock)
            {
                if (m_sendQueue.Count == 0)
                {
                    isSending = false;
                    return;
                }

                data = m_sendQueue.Dequeue();
            }
            m_sendArgs.SetBuffer(data, 0, data.Length);

            bool pending = m_clientSocket.SendAsync(m_sendArgs);
            if(!pending)
            {
                OnSendComplete(m_clientSocket, m_sendArgs);
            }
        }
        private void OnSendComplete(object? sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)//∑¢ÀÕ ß∞‹
            {
                Close();
            }
            else
            {
                StartSend();
            }
        }
        public void Close()
        {
            if (Interlocked.Exchange(ref m_closed, 1) == 1)
            {
                return;
            }

            try
            {
                if (m_clientSocket.Connected)
                {
                    m_clientSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch
            {
            }
            finally
            {
                MessageDispatcher.Instance.Unsubscribe<HeartCheckRequest>(OnClientSendHeart);

                try
                {
                    m_clientSocket.Close();
                }
                catch
                {
                }

                OnDisconnected?.Invoke(this);
                OnDisconnected = null;
            }
        }
    }
}
