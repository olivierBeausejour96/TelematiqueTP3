using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TP3
{
    class Program
    {
        static IPEndPoint[] googleDNSEndpoints = {
            new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53),
            new IPEndPoint(IPAddress.Parse("8.8.4.4"), 53)
        };
        static IPEndPoint udesDNSEndpoint = new IPEndPoint(IPAddress.Parse("10.44.82.8"), 53);
        delegate void task();
        static void Main(string[] args)
        {
            task ServerTask = () => 
            {
                UdpClient udpServer = new UdpClient(23232);

                while (true)
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 23232);
                    var data = udpServer.Receive(ref remoteEP); // listen on port 11000
                    Console.WriteLine("Server Receive data from " + remoteEP.ToString() + ": " + new string(data.Select(b => (char)b).ToArray()));
                    udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
                }
            };



            task ClientTask = () =>
            {
                var client = new UdpClient();
                IPEndPoint ep = googleDNSEndpoints[0];
                client.Connect(ep);
                string data = "6google3com0";
                Console.WriteLine("Type data to send. Enter 'q' or 'Q' to exit");

                List<DNSQuestion> queries = new List<DNSQuestion>();
                queries.Add(new DNSQuestion("www.dotnetperls.com"));
                DNSPacket packet = new DNSPacket(queries);

                var ba = packet.toByteArray();

                // send data
                client.Send(ba, ba.Length);

                // then receive data
                var receivedData = client.Receive(ref ep);

                packet = new DNSPacket(receivedData);

                Console.WriteLine("Client Receive data from " + ep.ToString() + ": \n");
                for (int i = 0; i < 4; i++)
                {
                    Console.Write(String.Format("{0,1:D}", receivedData[i]));
                    if (i != 4-1)
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
                while (data.ToLower() != "q")
                {
                    Console.WriteLine("Type data to send. Enter 'q' or 'Q' to exit");
                    data = Console.ReadLine();

                    // send data
                    client.Send(data.ToCharArray().Select(c => (byte)c).ToArray(), data.Length);

                    // then receive data
                    receivedData = client.Receive(ref ep);

                    Console.WriteLine("Client Receive data from " + ep.ToString() + ": " + new string(receivedData.Select(b => (char)b).ToArray()));
                }



            };

            Task.Run(new Action(ServerTask));
            var qwe = Task.Run(new Action(ClientTask));
            qwe.Wait();
        }
    }
}
