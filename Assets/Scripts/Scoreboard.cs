using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Scoreboard : MonoBehaviour
{


    public string host;
    public int port;
    public int hostID;
    public int channelID;
    public int connectionId;

    string userName = "scoreboard";

    public void Connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(hostID, host, port, 0, out error);
    }

    public void Join()
    {
        byte[] msg = Message.ObjectToByteArray(new Message(Message.MsgType.REGISTER, userName));
        byte error;
        NetworkTransport.Send(hostID, connectionId, channelID, msg, msg.Length, out error);
    }


    public void Update()
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
                break;

            case NetworkEventType.DataEvent:
                Message msg = Message.ByteArrayToMessage(recBuffer);
                Debug.Log(msg);
                HandleMessage(msg);
                break;
        }
    }

    private void HandleMessage(Message msg)
    {
        switch (msg.type)
        {
            case Message.MsgType.QSTART:

                break;

            case Message.MsgType.ERROR:

                break;

            case Message.MsgType.SUCCESS:

                break;
        }
    }
}
