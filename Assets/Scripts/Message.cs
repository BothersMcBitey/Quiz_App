using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Message : Object
{
    public enum MsgType
    {
        BUZZ,
        REGISTER,
        ANSWER,
        QSTART,
        STATE,
        ERROR,
        SUCCESS
    }

    public DateTime hitTime;
    public MsgType type;
    public string name;
    public State state;
    public bool isBuzzNotAB;

    public Message(MsgType type, string name=null, int port=0, State state=null, bool isBuzzNotAB = true)
    {
        this.type = type;
        this.name = name;
        this.state = state;
        this.isBuzzNotAB = isBuzzNotAB;

        hitTime = DateTime.Now.ToUniversalTime();       
    }

    public DateTime GetTime()
    {
        return hitTime;
    }

    public override string ToString()
    {
        string s = hitTime.ToLongTimeString() + "." + hitTime.Millisecond + ", " + type.ToString() + ", ";
        switch (type)
        {
            case MsgType.REGISTER:
                s += name;
                break;
        }
        return s;
    }

    public static byte[] ObjectToByteArray(Object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static Message ByteArrayToMessage(byte[] arrBytes)
    {
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            return (Message)obj;
        }
    }
}
