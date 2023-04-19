using SMBv1Buster;
using System.Net;
using System.Net.NetworkInformation;

internal class Program
{
    private static void Main(string[] args)
    {
        DeviceLocator locator = new DeviceLocator(); // Parameter er thread count
        IPRangeFinder finder = new IPRangeFinder();
        //string defaultGateway = locator.NetworkGateway(); // 192.168.0.1 på mit hjemmenetværk

        IPAddress startingIP;
        IPAddress endingIP;
        int port;

        Console.WriteLine("Welcome to SMBv1 Buster v1.0");

        void GetPort()
        {
            try
            {
                Console.WriteLine("Please input the port you'd like to scan");
                port = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Please input a valid integer");
                GetPort();
            }
        }

        void GetStartingIP()
        {
            try
            {
                Console.WriteLine("Input the starting IP");
                startingIP = IPAddress.Parse(Console.ReadLine());
            }
            catch 
            {
                Console.WriteLine($"Please input a valid IP-Address - (ex. {locator.NetworkGateway()})");
                GetStartingIP();
            }
        }

        void GetEndingIP()
        {
            try
            {
                Console.WriteLine("Input the ending IP");
                endingIP = IPAddress.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine($"Please input a valid IP-Address - (ex. {locator.NetworkGateway()})");
                GetEndingIP();
            }
        }

        void PrintIPsOnSpecifiedPort()
        {
            IEnumerable<IPAddress> ipsOnPort = locator.GetIpsBySpecificPort(startingIP, endingIP, port);

            locator.PrintAllIps();
        }

        void PrintAllIPs() // Old function to display the use of GetIPRange
        {
            List<IPAddress> myIps = finder.GetIPRange(startingIP, endingIP).ToList();

            foreach (var ip in myIps)
            {
                Console.WriteLine(ip);
            }
        }

        //GetPort();
        //GetStartingIP();
        //GetEndingIP();
        //PrintIPsOnSpecifiedPort();
        DeviceLocator.GetDevicesOnSMB();



        Console.ReadLine();
        
    }
}