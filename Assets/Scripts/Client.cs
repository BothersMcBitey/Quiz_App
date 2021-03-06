﻿using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class Client : MonoBehaviour
{

    public InputField nameInput;
    public GameObject inputField;
    public GameObject registerBtn;
    public GameObject connectBtn;

    public string host;
    public int port;
    public int hostID;
    public int channelID;
    public int connectionId;

    public string userName;

    public void Connect()
    {
        byte error;
        connectionId = NetworkTransport.Connect(hostID, host, port, 0, out error);

        registerBtn.SetActive(true);
        inputField.SetActive(true);
        connectBtn.SetActive(false);
    }

    public void Join()
    {
        byte[] msg = Message.ObjectToByteArray(new Message(Message.MsgType.REGISTER, GetName()));
        byte error;
        NetworkTransport.Send(hostID, connectionId, channelID, msg, msg.Length, out error);
    }

    private string GetName()
    {
        userName = nameInput.text;
        return nameInput.text;
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
