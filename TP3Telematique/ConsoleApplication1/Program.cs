using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        static string httpRequestFormat = "GET {0} HTTP/1.1 \r\nHost: {1}\r\n\r\n";

        delegate void task();
        static public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        static public DNSPacket DNSCall(string domainName, IPEndPoint DNSEndpoint)
        {
            var client = new UdpClient();
            client.Connect(DNSEndpoint);
            List<DNSQuestion> queries = new List<DNSQuestion>();
            queries.Add(new DNSQuestion(domainName));
            DNSPacket packet = new DNSPacket(queries);
            Console.WriteLine("DNSQuery packet: ");
            packet.print();
            var ba = packet.toByteArray();
            client.Send(ba, ba.Length);
            IPEndPoint remoteEP = null;
            var receivedData = client.Receive(ref remoteEP);
            client.Close();
            packet = new DNSPacket(receivedData);
            return packet;
        }

        static public List<IPAddress> GetAddressesFromDNSResponse(DNSPacket packet)
        {
            List<IPAddress> l = new List<IPAddress>();

            for (int i = 0; i < packet.Answers.Count; i++)
            {
                if (packet.Answers[i].type != 1)
                    continue;
                string addr = "";
                for (int k = 0; k < packet.Answers[i].data.Length; k++)
                {
                    addr += ((int)packet.Answers[i].data[k]).ToString();
                    if (k != packet.Answers[i].data.Length - 1)
                    {
                        addr += ".";
                    }
                }
                l.Add(IPAddress.Parse(addr));
            }
            return l;
        }

        static public byte[] ExecuteHttpRequest(string domainName, string path, IPAddress address)
        {
            string request = string.Format(httpRequestFormat, path, domainName);

            Console.WriteLine("");
            Console.WriteLine("HTTP request: ");
            Console.WriteLine(request);

            var ba = Encoding.ASCII.GetBytes(request);
            var tcpClient = new TcpClient();
            tcpClient.Connect(new IPEndPoint(address, 80));
            var tcpStream = tcpClient.GetStream();

            tcpStream.Write(ba, 0, ba.Length);
            List<byte> l = new List<byte>();
            int step = 128;
            byte[] baR = new byte[step];
            int readBytes = step;
            while (readBytes == step)
            {
                readBytes = tcpStream.Read(baR, 0, baR.Length);
                l.AddRange(baR);
            }
            tcpStream.Close();
            tcpClient.Close();

            return l.ToArray();
        }


        static void Main(string[] args)
        {


            task ClientTask = () =>
            {
                string domainName = "www.google.ca";
                string rootPath = "/";
                const string imageSearchRegexPattern = "([a-z\\-_0-9\\/\\:\\.]*\\.(jpg|jpeg|png|gif|tiff))";
                const string domainNameRegexPattern = "/[a-z]*\\.[a-z]*\\.[a-z]*/";


                IPEndPoint DNSEndpoint = googleDNSEndpoints[0];

                DNSPacket receivedPacket = DNSCall(domainName, DNSEndpoint);

                Console.WriteLine("");
                Console.WriteLine("Received packet: ");
                receivedPacket.print();

                var vA = GetAddressesFromDNSResponse(receivedPacket);

                IPAddress theAddress = vA.First();

                var baR = ExecuteHttpRequest(domainName, rootPath, theAddress);

                string test = new string(baR.Select(c => (char)c).ToArray());

                Console.WriteLine("");
                Console.WriteLine("HttpRequestAnswer: ");
                Console.WriteLine(test);

                Regex regex = new Regex(imageSearchRegexPattern);
                MatchCollection match = regex.Matches(test);
                string domain = "";
                string path = "";
                if (match.Count > 0)
                {
                    foreach (Match m in match)
                    {
                        if (m == null)
                            continue;

                        if (m.Value.ToLower().Contains("https"))
                        {
                            domain = new string(m.Value.Skip(8).TakeWhile(c => c != '/').ToArray());
                            path = new string(m.Value.Skip(8 + domain.Length).TakeWhile(c => c != '\r').ToArray());
                            break;
                        }

                    }
                }


                baR = ExecuteHttpRequest(domain, path, theAddress);

                test = new string(baR.Select(c => (char)c).ToArray());
                Console.WriteLine("");
                Console.WriteLine("HttpRequestAnswer: ");
                Console.WriteLine(test); // this line makes a window sound. What the actual fuck


                string[] lines = test.Split('\n');
                int j;
                int nbBytesToSkip = 0;
                for (j = 0; j < lines.Length; j++)
                {
                    if (string.IsNullOrWhiteSpace(lines[j]))
                    {
                        nbBytesToSkip += lines[j].Length+1;
                        break;
                    }
                    else
                    {
                        nbBytesToSkip += lines[j].Length+1;
                    }
                }


                var asdda = test.Skip(nbBytesToSkip).Take(test.Length - nbBytesToSkip).Select(c => c).ToArray();

                ByteArrayToFile(@".\test2.jpg", test.Skip(nbBytesToSkip).Take(test.Length - nbBytesToSkip).Select(c => (byte)c).ToArray());

                int sad = 2;
                
    
            };

            var qwe = Task.Run(new Action(ClientTask));
            qwe.Wait();
            Console.ReadLine();
        }
        
    }

}
