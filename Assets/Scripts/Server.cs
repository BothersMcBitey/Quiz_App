using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete

[Serializable]
public class Server : MonoBehaviour
{
    [Serializable]
    public class Client
    {
        public int connectionID;
        public string userName;
        public int score;

        public Client(int connID, string uName)
        {
            connectionID = connID;
            userName = uName;
            score = 0;
        }
    }

    public List<Client> clients;
    public List<Message> buzzers;
    public bool isBuzzEnabled = false;
    public List<Message> answers;
    public bool isAnswerEnabled = false;

    public int channelID;
    public int hostID;

    private void Awake()
    {
        NetworkTransport.Init();

        ConnectionConfig config = new ConnectionConfig();
        channelID = config.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(config, 20);

        hostID = NetworkTransport.AddHost(topology, 12345);       
    }    

    // Update is called once per frame
    void Update()
    {
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                Debug.Log("Client Connected on " + connectionId);
                break;

            case NetworkEventType.DataEvent:
                Message msg = Message.ByteArrayToMessage(recBuffer);
                Debug.Log(msg);
                HandleMessage(msg, connectionId);
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log("Client Disconnected");
                break;
        }
    }

    private void HandleMessage(Message msg, int connectionID)
    {
        switch (msg.type)
        {
            case Message.MsgType.REGISTER:
                if(0 < clients.Where(x => x.userName.Equals(msg.name)).Count())
                {
                    Send(new Message(Message.MsgType.ERROR), connectionID);
                }
                else
                {
                    clients.Add(new Client(connectionID, msg.name));
                    Send(new Message(Message.MsgType.SUCCESS), connectionID);
                    Debug.Log("Client " + msg.name + " registered on connection " + connectionID);
                }
                break;

            case Message.MsgType.BUZZ:
                if (isBuzzEnabled)
                {
                    if(buzzers.Where(x => x.name.Equals(msg.name)).Count() < 1)
                    {
                        buzzers.Add(msg);
                    }
                    else
                    {
                        Send(new Message(Message.MsgType.ERROR), connectionID);
                    }
                }
                break;

            case Message.MsgType.ANSWER:
                if (isAnswerEnabled)
                {
                    if (answers.Where(x => x.name.Equals(msg.name)).Count() < 1)
                    {
                        answers.Add(msg);
                    }
                    else
                    {
                        Send(new Message(Message.MsgType.ERROR), connectionID);
                    }
                }
                break;
        }
    }

    public void Send(Message msg, int connectionID)
    {
        byte[] buffer = Message.ObjectToByteArray(msg);
        byte error;
        NetworkTransport.Send(hostID, connectionID, channelID, buffer, buffer.Length, out error);
    }
}
