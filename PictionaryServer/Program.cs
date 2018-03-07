using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PictionaryServer
{
    class Program
    {
        //Format
        //Command   Id          Reszta
        //data[0]   data[1]     data[2 3 4 ...]
        private static int COMMAND = 0;
        private static int ID = 1;


        static void Main(string[] args)
        {
            int serverConnectingPort = 11000;        
            int serverRecivingPort = 11001;
            int serverSendingPort = 11002;
            UdpClient udpServerConnecting = new UdpClient(serverConnectingPort);                     
            byte id = 0;
            ConcurrentQueue<byte[]> queueToSend = new ConcurrentQueue<byte[]>();
            ConcurrentDictionary<int, Client> clients = new ConcurrentDictionary<int, Client>();



            Task sending = Task.Factory.StartNew(() =>
            {
                UdpClient udpServerSending = new UdpClient();
                byte[] dataToSend;
                while (true)
                {
                    if (queueToSend.TryDequeue(out dataToSend))
                    {
                        foreach (KeyValuePair<int, Client> client in clients)
                        {
                            if (client.Key != dataToSend[ID])
                            udpServerSending.Send(dataToSend, dataToSend.Length, client.Value.ipEndPoint);
                            //Console.WriteLine("Wyslalem " + dataToSend.ToString() + " do " +client.Value.ipEndPoint.ToString());
                        }
                    }

                }
            });

            Task reciving = Task.Factory.StartNew(() =>
            {
                UdpClient udpServerReciving = new UdpClient(serverRecivingPort);
                while (true)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, serverRecivingPort);
                    byte[] data = udpServerReciving.Receive(ref remoteEP);
                    Client client;
                    clients.TryGetValue(data[ID], out client);
                    if (data[COMMAND] == (byte)Commands.Color)
                    {
                        client.colorRed = data[2];
                        client.colorGreen = data[3];
                        client.colorBlue = data[4];
                        Console.WriteLine("[INFO] Client change color. Id " + client.id);
                    }
                    else if (data[COMMAND] == (byte)Commands.FirstCoords)
                    {
                        client.x = BitConverter.ToDouble(data, 2);
                        client.y = BitConverter.ToDouble(data, 10);
                    }
                    else if (data[COMMAND] == (byte)Commands.NextCoords)
                    {
                        double x = BitConverter.ToDouble(data, 2);
                        double y = BitConverter.ToDouble(data, 10);
                        byte[] datagram = new byte[37];
                        datagram[0] = (byte)Commands.Line;
                        datagram[1] = client.id;
                        datagram[2] = client.colorRed;
                        datagram[3] = client.colorGreen;
                        datagram[4] = client.colorBlue;
                        byte[] x1 = BitConverter.GetBytes(client.x);
                        byte[] y1 = BitConverter.GetBytes(client.y);
                        byte[] x2 = BitConverter.GetBytes(x);
                        byte[] y2 = BitConverter.GetBytes(y);
                        Buffer.BlockCopy(x1, 0, datagram, 5, x1.Length);
                        Buffer.BlockCopy(y1, 0, datagram, 5 + x1.Length, y1.Length);
                        Buffer.BlockCopy(x2, 0, datagram, 5 + x1.Length + y1.Length, x2.Length);
                        Buffer.BlockCopy(y2, 0, datagram, 5 + x1.Length + y1.Length + x2.Length, y2.Length);
                        client.x = x;
                        client.y = y;
                        queueToSend.Enqueue(datagram);
                    }
                }
            });


            while (true)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, serverConnectingPort);
                byte[] data = udpServerConnecting.Receive(ref ep);
                if (data[COMMAND] == (byte)Commands.Connect)
                {
                    byte addId;
                    if (data[ID] == 0)
                    {
                        addId = ++id;                       
                    } else
                    {
                        addId = data[ID];
                    }
                    serverSendingPort++;
                    if ((clients.TryAdd(addId, new Client(addId, new IPEndPoint(ep.Address, serverSendingPort), data[2], data[3], data[4]))) ) //Zawsze zwróci prawdę, bo client który już był wraca z tym samym id, ale tamto zostalo usuniete, wiec to id jest jakby nowe, mozna trzymac wszystkich klientow, ale to jest chyba lepsze
                    {
                        byte[] datagram = new byte[10];
                        datagram[0] = (byte)Commands.Connect;
                        datagram[1] = addId;
                        byte[] recivingPort = BitConverter.GetBytes(serverRecivingPort);
                        byte[] sendingPort = BitConverter.GetBytes(serverSendingPort);
                        Buffer.BlockCopy(recivingPort, 0, datagram, 2, recivingPort.Length);
                        Buffer.BlockCopy(sendingPort, 0, datagram, 2 + recivingPort.Length, sendingPort.Length);
                        udpServerConnecting.Send(datagram, datagram.Length, ep);
                        Console.WriteLine("[CONNECTED] Client from " + ep.ToString() + " connected. Id " + addId);
                    }
                }
                else if (data[COMMAND] == (byte)Commands.Disconnect)
                {
                    Client client;
                    if (clients.TryRemove(data[1], out client))
                    {
                        Console.WriteLine("[DISCONNECTED] Client from " + client.ipEndPoint.ToString() + " disconnected. Id " + client.id);
                    }
                }
            }





        }

    }
}
