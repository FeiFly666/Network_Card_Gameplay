using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMessage;
using Common.Network;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Common;
using System;
using Assets.Scripts.Util;
using Assets.Scripts.UI.Common;
using Assets.Scripts.Manager;
public class NetClient : Singleton<NetClient>, ISession
{
    public const int MaxBytes = 1024 * 1024;

    //private readonly int m_clientID;
    private Socket m_TcpSocket;
    private SocketAsyncEventArgs m_receiveArgs;
    private SocketAsyncEventArgs m_sendArgs;

    private MemoryStream m_buffer;

    public bool isConnected;
    public float lastCommunicateTime;

    private bool manualClosed = false;

    public Action OnDisconnected;
    public Action OnConnected;

    public bool isInit = false;

    private readonly object m_sendLock = new object();
    private Queue<byte[]> m_sendQueue = new Queue<byte[]>();
    private bool isSending = false;

    public void Init()
    {
        //InitSocket();

        if(!isInit)
        {
            m_buffer = new MemoryStream(MaxBytes);

            MessageDispatcher.Instance.Onlog += DebugLog;

            isInit = true;
        }

    }
    private void InitSocket()
    {
        m_TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        if(m_receiveArgs != null)
        {
            m_receiveArgs.Completed -= OnRecieveComplete;
        }

        m_receiveArgs = new SocketAsyncEventArgs();
        m_receiveArgs.SetBuffer(new byte[MaxBytes], 0, MaxBytes);
        m_receiveArgs.Completed += OnRecieveComplete;

        if(m_sendArgs != null)
        {
            m_sendArgs.Completed -= OnSendComplete;
        }

        m_sendArgs = new SocketAsyncEventArgs();
        m_sendArgs.SetBuffer(new byte[MaxBytes], 0, MaxBytes);
        m_sendArgs.Completed += OnSendComplete;
    }
    void DebugLog(string msg)
    {
        Debug.Log(msg);
    }
    public void Start()
    {
        OnDisconnected += ShowReconnect;
        //OnConnected += StartReceive;

        //ConnectToServer();
        
    }
    public bool ConnectToServer()
    {
        try
        {
            m_TcpSocket.Connect("127.0.0.1", 8080);

            if (m_TcpSocket.Connected)
            {
                Debug.Log("连接成功");

                isConnected = true;

                manualClosed = false;

                OnConnected?.Invoke();

                StartReceive();

                lastCommunicateTime = Time.time;

                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"连接失败: {ex.Message}");

            isConnected = false;

            OnDisconnected?.Invoke();
        }
        return false;
    }
    private void ShowReconnect()
    {
        // TODO: 弹窗重连
        if(!manualClosed)
            MessageBox.Show(MessageType.Error, "网络连接失败，请点击确定重试",()=> { 
                if(Reconnect())
                {
                    if(PlayerInfoManager.Instance.userID != string.Empty)
                    {
                        PlayerInfoManager.Instance.SendLogin(PlayerInfoManager.Instance.userID);
                    }
                }
            }
            
            , () => { Application.Quit(); });
    }
    public bool Reconnect()//给弹窗用
    {
        ResetConnectState();
        InitSocket();
        return ConnectToServer();
    }
    private void ResetConnectState()
    {
        if (m_buffer == null)
            m_buffer = new MemoryStream(MaxBytes);
        else
            m_buffer.SetLength(0);

        m_buffer.Position = 0;

        lock (m_sendLock)
        {
            m_sendQueue.Clear();
            isSending = false;
        }
    }
    public void StartReceive()
    {
        if (!isConnected)
        {
            //Debug.Log(" recieve message failed");
            return;
        }

       // Debug.Log("start recieve message");

        bool pending = m_TcpSocket.ReceiveAsync(m_receiveArgs);
        if (!pending)
        {
            OnRecieveComplete(m_TcpSocket, m_receiveArgs);
        }
    }
    private void OnRecieveComplete(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)//接收失败
        {
            MessageDispatcher.Instance.Enqueue(() => Close());
            //Close();
            return;
        }
        //Debug.Log("接收到新消息");

        byte[] receiveBytes = new byte[e.BytesTransferred];
        Buffer.BlockCopy(e.Buffer, 0, receiveBytes, 0, e.BytesTransferred);
       

        //ParsePackage();
        MessageDispatcher.Instance.Enqueue(
            () =>
            {
                if (receiveBytes.Length + m_buffer.Position > m_buffer.Capacity)
                {
                    Close();
                    throw new Exception("overflow");
                }

                //未溢出写入
                m_buffer.Write(receiveBytes, 0, receiveBytes.Length);

                lastCommunicateTime = Time.time;
                MessageHandler.ParseAndDispatch(this, m_buffer);
            }
        );
        StartReceive();
    }
    public void Send(Message message)
    {
        byte[] data = MessageHandler.Package(message);

        if (data == null) return;

        lock(m_sendLock)
        {
            m_sendQueue.Enqueue(data);
            if(isSending)
            {
                return;
            }
            isSending = true;
        }
        StartSend();
    }
    private void StartSend()
    {
        if(!m_TcpSocket.Connected)
        {
            isSending = false;
            m_sendQueue.Clear();
            return;
        }

        byte[] data = null;
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

        bool pending = m_TcpSocket.SendAsync(m_sendArgs);

        if (!pending)
        {
            OnSendComplete(m_TcpSocket, m_sendArgs);
        }
    }

    private void OnSendComplete(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)//发送失败
        {
            MessageDispatcher.Instance.Enqueue(() => Close());
            //Close();
        }
        else
        {
            StartSend();
        }
    }

    public void Close(bool isManual = false)
    {
        if (!isConnected) return;

        isConnected = false;

        try
        {
            m_TcpSocket?.Shutdown(SocketShutdown.Both);
        }
        catch
        {

        }

        m_TcpSocket?.Close();

        manualClosed = isManual;

        if(!isManual)
        {
            MessageDispatcher.Instance.Enqueue(
               () =>
               {
                   OnDisconnected?.Invoke();
               }
            );
        }


        Debug.Log("连接关闭");
    }
    
}
