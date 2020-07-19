using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using Object = System.Object;
using UnityEngine.UI;

[System.Serializable]
public class Client : MonoBehaviour
{

    public InputField nameInput;

    private Socket conn;
    
    public string host;
    public int port;
    private int id = 0;

    public string userName;

    public bool registered = false;

    public void Start()
    {
        nameInput = FindObjectOfType<InputField>();
    }

    public void Connect()
    {
        userName = GetName();

        conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        conn.Connect(host, port);
        conn.Send(Message.ObjectToByteArray(new Message(id, Message.MsgType.REGISTER, userName)));

        Message response = Server.RecieveMessage(conn);

        if (response.type == Message.MsgType.CONNUPDATE)
        {
            port = response.port;
            id = response.userID;
            registered = true;

            conn.Close();
            conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            conn.Connect(host, port);
        }
    }

    private string GetName()
    {
        return nameInput.text;
    }    


    private void OnApplicationQuit()
    {
        if (registered)
        {
            conn.Send(Message.ObjectToByteArray(new Message(id, Message.MsgType.DISCONNECT)));
            conn.Close();
        }
    }
}
