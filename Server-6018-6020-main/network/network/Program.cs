using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

class Network
{
    static Dictionary<int, byte[]> gameState = new Dictionary<int, byte[]>();
    static int lastAssignedGlobalID = 12;
    static List <IPEndPoint> connectedClients = new List<IPEndPoint> ();
    static void Main(string[] args)
    {


        int recv;
        //the expected packet size
        byte[] data = new byte[1024];
        //the server ip. local would be 127.0.0.1
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.0.30"), 9050);
        //creating the scoket using UDP
        Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        newsock.Bind(ipep);
        Console.WriteLine("Socket Open...");

        

        while (true)
        {
            //listening for the client connecting
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            //waiting for a message from the client and the client's unique ID
            data = new byte[1024];
            
            recv = newsock.ReceiveFrom(data, ref Remote);

            Console.WriteLine("Message Received From" + Remote.ToString());

            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

            string messageRecieved = Encoding.ASCII.GetString(data, 0,recv);
            // requesting a unique ID for the local object

            if (messageRecieved.Contains("I need a UID for local object:"))
            {
                Console.WriteLine(messageRecieved.Substring(messageRecieved.IndexOf(':') + 1));

                int localObjectNumber = Int32.Parse(messageRecieved.Substring(messageRecieved.IndexOf(':') + 1));

                string returnVal = ("Assigned UID:" + localObjectNumber + ";" + lastAssignedGlobalID++);

                Console.WriteLine(returnVal);
                newsock.SendTo(Encoding.ASCII.GetBytes(returnVal), Encoding.ASCII.GetBytes(returnVal).Length, SocketFlags.None, Remote);
            }
            //checks for the clients connected, each time a new client connects the number of clients is updated

            bool IPisInList = false;
            IPEndPoint senderIPEndPoint = (IPEndPoint)Remote;
            foreach (IPEndPoint ep in connectedClients)
            {
                if (senderIPEndPoint.ToString().Equals(ep.ToString())) IPisInList = true;
            }
            if (!IPisInList)
            {
                connectedClients.Add(senderIPEndPoint);
                Console.WriteLine("A new client just connected. There are now " + connectedClients.Count + " clients.");
            }

            //sending gamestate information
            foreach (IPEndPoint ep in connectedClients)

            {
                Console.WriteLine("Sending gamestate to " + ep.ToString());
                if (ep.Port != 0)
                {
                    foreach (KeyValuePair<int, byte[]> kvp in gameState)
                    {
                        newsock.SendTo(kvp.Value, kvp.Value.Length, SocketFlags.None, ep);
                    }
                }
            }

            if (messageRecieved.Contains("I need a UID for local object:"))
            {
                
            }
            else if (messageRecieved.Contains("Object data;"))
            {
                //get the global id from the packet
                Console.WriteLine(messageRecieved);
                string globalId = messageRecieved.Split(";")[1];
                int intId = Int32.Parse(globalId);
                if (gameState.ContainsKey(intId))
                { //if true, we're already tracking the object
                    gameState[intId] = data; //data being the original bytes of the packet
                }
                else //the object is new to the game
                {
                    gameState.Add(intId, data);
                }
            }

         
        }

    }

}