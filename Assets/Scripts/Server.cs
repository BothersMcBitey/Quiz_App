using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Server : MonoBehaviour
{

    private class ClientConnection
    {
        public Socket listener;
        public int clientId;
        public int port;
        public Thread handler;
        public string clientName;

        public ClientConnection(Socket s, int id, Thread t, string n, int p)
        {
            listener = s;
            clientId = id;
            handler = t;
            clientName = n;
            port = p;
        }
    }

    private Socket listener;
    private List<ClientConnection> clients;
    private Thread listen;

    private IPAddress hostip;
    public int nextPort;
    private int nextClientID = 1;

    private void Awake()
    {
        clients = new List<ClientConnection>();

        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        hostip = ipHost.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).First();
        IPEndPoint localEndPoint = new IPEndPoint(hostip, nextPort++);

        Debug.Log(localEndPoint.ToString());

        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            listener.Bind(localEndPoint);

            ThreadStart listenStart = new ThreadStart(RegisterListen);
            listen = new Thread(listenStart);
            listen.Start();
        }
        catch (Exception e)
        {
            Debug.Log(e);          
        }
    }

    public static Message RecieveMessage(Socket socket)
    {
        byte[] packet = new byte[0];
        byte[] buffer = new byte[1024];
        int numByte = 1024;
        while (numByte == 1024)
        {
            numByte = socket.Receive(buffer);

            packet = packet.Concat(buffer).ToArray();
        }

        return Message.ByteArrayToMessage(packet);
    }

    void RegisterListen()
    {
        listener.Listen(10);

        while (true)
        {
            Debug.Log("Waiting for REGISTER connection ... ");

            Socket clientSocket = listener.Accept();

            // Data buffer 
            Message msg = RecieveMessage(clientSocket);

            Debug.Log(msg.ToString());

            if(msg.type == Message.MsgType.REGISTER)
            {
                RegisterClient(msg, clientSocket);
            }
            else
            {
                clientSocket.Send(Message.ObjectToByteArray(new Message(0, Message.MsgType.BADPORT)));
            }

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }

    void ClientListen(Socket listener, int clientID)
    {
        listener.Listen(1);

        while (true)
        {
            Debug.Log("Waiting for Client"+clientID+" connection ... ");

            Socket clientSocket = listener.Accept();

            Debug.Log("Client " + clientID + " successfully connected");

            bool connected = true;
            while (connected)
            {

                // Data buffer 
                Message msg = RecieveMessage(clientSocket);

                Debug.Log(msg.ToString());

                if (msg.userID != clientID)
                {
                    connected = false;
                    clientSocket.Send(Message.ObjectToByteArray(new Message(0, Message.MsgType.BADPORT)));
                }

                if (msg.type == Message.MsgType.DISCONNECT)
                {
                    connected = false;
                }
                else
                {
                    clientSocket.Send(Message.ObjectToByteArray(new Message(0, Message.MsgType.ERROR)));
                }
            }

            Debug.Log("Client disconnected from handler " + clientID);

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }

    void RegisterClient(Message msg, Socket client)
    {
        if (0 < clients.Count(x => x.clientName.Equals(msg.name)))
        {
            ClientConnection cc = clients.First(x => x.clientName.Equals(msg.name));

            client.Send(Message.ObjectToByteArray(new Message(cc.clientId, Message.MsgType.CONNUPDATE, port: cc.port)));
        }
        else
        {
            int clientPort = nextPort++;
            int clientID = nextClientID++;
            IPEndPoint localEndPoint = new IPEndPoint(hostip, clientPort);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);

                ThreadStart clientStart = new ThreadStart(() => ClientListen(listener, clientID));
                Thread clientHandle = new Thread(clientStart);
                clientHandle.Start();

                client.Send(Message.ObjectToByteArray(new Message(clientID, Message.MsgType.CONNUPDATE, port: clientPort)));

                clients.Add(new ClientConnection(listener, clientID, clientHandle, msg.name, clientPort));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
