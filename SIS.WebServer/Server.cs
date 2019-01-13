using SIS.WebServer.Api;
using SIS.WebServer.Routing;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SIS.WebServer
{
    public class Server
    {
        private const string LocalhostIpAddress = "127.0.0.1";

        private readonly int port;

        private readonly TcpListener listener;

        private readonly ServerRoutingTable serverRoutingTable;

        //private readonly IHttpHandler handler; //Добавено при Гълов

        private bool isRunning;

        public Server(int port, ServerRoutingTable serverRoutingTable)
        {
            this.port = port;
            this.listener = new TcpListener(IPAddress.Parse(LocalhostIpAddress), port);
            this.serverRoutingTable = serverRoutingTable;
            //this.handler = new HttpHandler(serverRoutingTable); //И двете ли ще ги има?
        }

        //public Server(int port, IHttpHandler handler) //Добавено при Гълов
        //{
        //    this.port = port;
        //    this.listener = new TcpListener(IPAddress.Parse(LocalhostIpAddress), port);
        //    this.handler = handler;
        //}

        //public void Run()
        //{
        //    this.listener.Start();
        //    this.isRunning = true;
        //    Console.WriteLine($"Server started at http://{LocalhostIpAddress}:{port}");

        //    Task task = Task.Run(this.ListenLoop);
        //    task.Wait();
        //}

        //public async Task ListenLoop()
        //{
        //    while (this.isRunning)
        //    {
        //        Socket client = await this.listener.AcceptSocketAsync();
        //        ConnectionHandler connectionHandler = new ConnectionHandler(client, this.serverRoutingTable);
        //        Task responseTask = connectionHandler.ProcessRequestAsync();
        //        responseTask.Wait();
        //    }
        //}

        public void Run()
        {
            this.listener.Start();
            this.isRunning = true;

            Console.WriteLine($"Server started at http://{LocalhostIpAddress}:{this.port}");
            while (isRunning)
            {
                //Console.WriteLine("Waiting for client...");
                Socket client = listener.AcceptSocketAsync().GetAwaiter().GetResult();
                Task.Run(() => Listen(client));
            }
        }

        public async void Listen(Socket client)
        {
            var connectionHandler = new ConnectionHandler(client, this.serverRoutingTable);
            await connectionHandler.ProcessRequestAsync();
        }
    }
}