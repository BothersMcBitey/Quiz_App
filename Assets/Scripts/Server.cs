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

public class Server : MonoBehaviour
{

    private Socket listener;
    private Thread listen;

    private void Awake()
    {
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddr = ipHost.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).First();
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 12345);

        Debug.Log(localEndPoint.ToString());

        listener = new Socket(AddressFamily.InterNetwork, SocketType.Rdm, ProtocolType.Tcp);
        try
        {
            listener.Bind(localEndPoint);

            ThreadStart listenStart = new ThreadStart(Listener);
            listen = new Thread(listenStart);
            listen.Start();
        }
        catch (Exception e)
        {
            Debug.Log(e);          
        }
    }

    void Listener()
    {
        listener.Listen(10);

        while (true)
        {
            Debug.Log("Waiting connection ... ");

            Socket clientSocket = listener.Accept();

            // Data buffer 
            List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>();

            int numByte = clientSocket.Receive(buffer);

            byte[] x = new byte[0];
            foreach(ArraySegment<byte> seg in buffer){
                x = x.Concat(seg).ToArray();
            }

            Message msg = ByteArrayToMessage(x);

            Debug.Log(msg.ToString());

            // Close client Socket using the 
            // Close() method. After closing, 
            // we can use the closed Socket  
            // for a new Client Connection 
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
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
            return (Message) obj;
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
