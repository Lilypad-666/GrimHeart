using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldOfTitansLauncher {
    class Program {
        static void Main(string[] args) {
            Directory.SetCurrentDirectory(@"C:\Users\Star\Desktop\WoT 2.0 Server");
            DarkRiftServer server = new DarkRiftServer(new ServerSpawnData(new System.Net.IPAddress(0), 4296, IPVersion.IPv4));
            server.Start();
            
            while (true) {
                server.ExecuteCommand(Console.ReadLine());
            }
        }
    }
}
