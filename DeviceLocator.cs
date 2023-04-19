using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SMBv1Buster
{
    public class DeviceLocator
    {
        public static List<IPAddress> IPs { get; set; } = new List<IPAddress>();
        public int Threads { get; set; }

        public DeviceLocator()
        {
            IPs = new List<IPAddress>();
        }
        
        // Useful!
        
        public string NetworkGateway()
        {
            string ip = null;

            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        ip = d.Address.ToString();
                    }
                }
            }
            return ip;
        }

        /// <summary>
        /// Method to iterate through all ips where the SMBv1 port (445) is active.
        /// -- Next version should use multi-threaded workload --
        /// </summary>
        public static async Task<List<string>> GetDevicesOnSMB()
        {
            List<string> devices = new List<string>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up); // Get all operational network devices

            foreach (var ni in interfaces)
            {
                var ipProps = ni.GetIPProperties();
                var addresses = ipProps.UnicastAddresses
                    .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(addr => addr.Address.ToString()); // Get all IPv4 addresses

                await Task.WhenAll(addresses.Select(async address => // Iterate through all addresses asynchronously
                {
                    var tcpClient = new TcpClient();
                    try
                    {
                        await tcpClient.ConnectAsync(address, 445).ConfigureAwait(false);
                        if (tcpClient.Connected)
                        {
                            devices.Add(address);
                        }
                    }
                    catch
                    {
                        // Connection failed, device not available on this address
                    }
                    finally
                    {
                        tcpClient.Close();
                    }
                }));
            }

            // Print devices
            foreach (var device in devices)
            {
                Console.WriteLine(device);
            }
            return devices;
        }


        public List<IPAddress> GetIpsBySpecificPort(IPAddress start, IPAddress end, int port)
        {
            IPs = new List<IPAddress>();
            IPRangeFinder finder = new IPRangeFinder();

            IPAddress[] ipsToCheck = finder.GetIPRange(start, end).ToArray();

            //TcpClient tcpClient = new TcpClient();

            Parallel.ForEach(ipsToCheck, ip => // Multithreaded workload for more speed
            {
                TcpClient tcpClient = new TcpClient();
                try
                {
                    tcpClient.ConnectAsync(ip, port).Wait(1000);

                    if (tcpClient.Connected)
                    {
                        Console.WriteLine($"IP {ip} is currently open on port {port}");
                        IPs.Add(ip);
                    }
                    else if (!tcpClient.Connected) { throw new Exception(); }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"IP {ip} is not available on port {port}"); // Exception thrown if connection failed - port is unused
                }
                finally { tcpClient.Close(); }
            });
            Console.WriteLine();
            return IPs;
        }

        private async void StartListening(int port)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            TcpListener listener = new TcpListener(ipEndPoint);

            try
            {
                listener.Start();

                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await using NetworkStream stream = handler.GetStream();

                var message = $"{DateTime.Now}";
                var dataTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dataTimeBytes);
                
                Console.WriteLine($"Port active: {handler.Client}");
            }
            finally
            {
                listener.Stop();
            }
        }

        public void StartListener(int port)
        {
            TcpListener server = null;

            try
            {
                IPAddress localAddress = IPAddress.Parse("127.0.0.1");

                server = new TcpListener(localAddress, port);
                server.Start(); // Will throw SocketException if VS is not allowed in the Firewall - or the port is already used by another process.

                Byte[] bytes = new byte[256];
                string data = null;

                while (true)
                {
                    Console.WriteLine($"Waiting for connection : Port {port}");

                    using TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;
                    NetworkStream stream = client.GetStream();
                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received {0}", data);

                        data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                    }

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e.Message);
            }
            finally
            {
                server.Stop();
            }
            Console.WriteLine("Server stopped");
        }

        /// <summary>
        /// All this is useless
        /// </summary>

        public void PingAll()
        {
            string gate_ip = NetworkGateway();

            string[] array = gate_ip.Split('.');

            for (int i = 2; i >= 255; i++)
            {
                string ping_var = $"{array[0]}.{array[1]}.{array[2]}.{i}";
                Ping(ping_var, 4, 4000);
            }
        }

        public void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate() 
                {
                    try
                    {
                        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {
                        // Supressing errors
                    }
                }).Start();
            }
        }

        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            IPAddress ip = IPAddress.Parse((string)e.UserState);
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                //string hostname = GetHostName;
                //string macaddress = GetMacAddress;
                IPs.Add(ip);
            }
        }

        public void PrintAllIps()
        {
            Console.WriteLine($"-- Printing IPs --");
            foreach (IPAddress ip in IPs)
            {
                Console.WriteLine(ip);
            }
        }

        
        
    }
}
