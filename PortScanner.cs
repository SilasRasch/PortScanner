using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SMBv1Buster
{
    public class PortScanner
    {
        public int Port { get; set; }
        private string host;

        public PortScanner(string host, int port)
        {
            Port = port;
            this.host = host;
        }

        public void Scan()
        {
            TcpClient tcpClient = new TcpClient();
            if (Port >= 0) 
            {
                Console.WriteLine($"Checking port {Port}");

                try
                {
                    tcpClient = new TcpClient(host, Port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally 
                { 
                    try
                    {
                        tcpClient.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    Console.WriteLine($"TCP port {Port} is open");
                }
            }
        }
    }
}
