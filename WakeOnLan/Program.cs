using System;
using System.Globalization;
using System.Net.Sockets;

namespace WakeOnLan
{
    class Program
    {
        static void Main(string[] args)
        {
            string macAddress = null;
            string remoteAddress = null;
            int remotePort = -1;

            if (args.Length >= 6)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i].Equals("/mac") && !string.IsNullOrEmpty(args[i + 1]))
                    {
                        macAddress = args[i + 1];
                    }
                    else if (args[i].Equals("/ip") && !string.IsNullOrEmpty(args[i + 1]))
                    {
                        remoteAddress = args[i + 1];
                    }
                    else if (args[i].Equals("/p") && !string.IsNullOrEmpty(args[i + 1]))
                    {
                        if (!int.TryParse(args[i + 1], out remotePort))
                            remotePort = -1;
                    }
                    else if (args[i].Equals("/port") && !string.IsNullOrEmpty(args[i + 1]))
                    {
                        if (!int.TryParse(args[i + 1], out remotePort))
                            remotePort = -1;
                    }
                }
            }

            if (string.IsNullOrEmpty(macAddress)) return;
            if (string.IsNullOrEmpty(remoteAddress)) return;
            if (remotePort < 0) return;

            byte[] macAddressBytes = GetMacAddressBytes(macAddress);

            Console.WriteLine("MAC Address:    {0}", MacAddressToString(macAddressBytes));
            Console.WriteLine("Remote Address: {0}", remoteAddress);
            Console.WriteLine("Remote Port:    {0}", remotePort);
			Console.WriteLine();

            // 6-bytes trailer + 16 * 6-byte MAC address
            byte[] magicPacket = new byte[17 * 6];

            // trailer: 6 times 0xFF.
            for (int i = 0; i < 6; i++)
                magicPacket[i] = 0xFF;

            // body: 16 times MAC address
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    magicPacket[i * 6 + j] = macAddressBytes[j];

            try
            {
                using (UdpClient udp = new UdpClient(remoteAddress, remotePort))
                {
                    udp.Send(magicPacket, magicPacket.Length);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);                
                Console.WriteLine();
                Console.ResetColor();
            }            
        }

        private static byte[] GetMacAddressBytes(string macAddress)
        {
            macAddress = macAddress.Replace(":", "").Replace("-", "").Replace(" ", "");

            byte[] macAddressBytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                macAddressBytes[i] = byte.Parse(macAddress.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            return macAddressBytes;
        }

        private static string MacAddressToString(byte[] macAddressBytes)
        {
            string macAddress = string.Empty;
            for (int i = 0; i < macAddressBytes.Length; i++)
            {
                macAddress += (i > 0 ? ":" : "") + macAddressBytes[i].ToString("x2");
            }
            return macAddress;
        }

    }
}
