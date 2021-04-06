using Modbus.Device;
using NSpeedTest;
using NSpeedTest.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetMod
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort heartbeat = 0;
            ModbusRun mb = new ModbusRun();

            HtmlValues hv = new HtmlValues();

            while (true)
            {
                mb.setHoldingValue(1, heartbeat++);

                WanStats optusWanStats = hv.loadOptusWanStats();
                if(optusWanStats.valid)
                {
                    mb.setHoldingFloat(3, optusWanStats.received);  // Recieved Bytes
                    mb.setHoldingFloat(5, optusWanStats.sent);      // Sent Bytes
                }

                WanSpeed speedTest = PerformSpeedTest();
                if(speedTest.valid)
                {
                    mb.setHoldingFloat(7, speedTest.download);      // Download Speed
                    mb.setHoldingFloat(9, speedTest.upload);        // Upload Speed
                }

                Thread.Sleep(Properties.Settings.Default.FetchDataSeconds*1000);
            }
        }

        private static SpeedTestClient client;
        private static Settings settings;

        static WanSpeed PerformSpeedTest()
        {
            WanSpeed speed = new WanSpeed();
            speed.valid = false;
            try
            {
                client = new SpeedTestClient();
                settings = client.GetSettings();
                var servers = settings.Servers.Where(s => s.Country.Equals("Australia")).Take(10).ToList();
                foreach (var svr in servers)
                {
                    svr.Latency = client.TestServerLatency(svr);
                }
                var server = servers.OrderBy(x => x.Latency).First();

                Console.WriteLine("WAN Speed Test to {0} {1}, distance: {2}km, latency: {3}ms", server.Sponsor, server.Name, (int)server.Distance / 1000, server.Latency);
                var downloadSpeed = client.TestDownloadSpeed(server);
                var uploadSpeed = client.TestUploadSpeed(server);
                Console.WriteLine("WAN Download Speed: {0} Kbps, WAN Upload Speed: {1} Kbps", Math.Round(downloadSpeed, 2), Math.Round(uploadSpeed, 2));

                speed.download = (float)downloadSpeed;
                speed.upload = (float)uploadSpeed;
                speed.valid = true;
            }
            catch (Exception)
            {
                Console.WriteLine("WAN Speed Test Failed");
            }
            return speed;
        }
    }

    struct WanSpeed
    {
        public float download, upload;
        public bool valid;
    }
}