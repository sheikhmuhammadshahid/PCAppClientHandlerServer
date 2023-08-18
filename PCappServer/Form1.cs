using PCappServer.classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace PCappServer
{
    public partial class Form1 : Form
    {
        string pressedBy = "-1";
        string roundType="-1";
        string questionNo = "-1";


        private Socket listenerSocket;
        private List<User> clientSockets = new List<User>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = true;
            getIP();
            new Thread(Start).Start();
        }


        public void Start()
        {
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenerSocket.Bind(new IPEndPoint(IPAddress.Any, 1234));
            listenerSocket.Listen(10);

            Console.WriteLine("Server started...");

            Thread listenThread = new Thread(ListenForClients);
            listenThread.Start();
        }

        public void Stop()
        {
            listenerSocket.Close();

            foreach (User clientSocket in clientSockets)
            {
                clientSocket.Socket.Close();
            }
        }

        private void ListenForClients()
        {
            while (true)
            {
                Socket clientSocket = listenerSocket.Accept();
                Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint);

              

                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = clientSocket.Receive(buffer);
                string name = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (!clientSockets.Any(s => s.Name.ToLower() == name.ToLower()) || name.ToLower() == "admin" || name.ToLower() == "admin1")
                {
                    try
                    {
                        BroadcastMessageToAll("connected:" + name);
                        while (true)
                        {
                            buffer = new byte[1024];
                            bytesRead = clientSocket.Receive(buffer);

                            if (bytesRead == 0)
                            {
                                Console.WriteLine("Client disconnected: " + name);
                                BroadcastMessageToAll("disconnected:" + name);

                                clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                                clientSocket.Close();

                                break;
                            }

                            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine("Received: " + message);

                            BroadcastMessageToAll(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        BroadcastMessageToAll("disconnected:" + name);
                        Console.WriteLine("Error: " + ex.Message);
                        clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                        clientSocket.Close();
                    }
                }
                else {
                    sendMessage("Already exists"+name,clientSocket);
                    clientSocket.Close();
                }

                
            }
            catch (Exception ex)
            {
               
            }
        }
        private void sendMessage(string message, Socket tosend) {
            try
            {
                tosend.Send(Encoding.ASCII.GetBytes(message));
            }
            catch (Exception ex)
            {

            }
        }
        private void BroadcastMessageExcept(string message, Socket excludeClient)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            foreach (User clientSocket in clientSockets)
            {
                if (clientSocket.Socket != excludeClient)
                {
                    try
                    {
                        clientSocket.Socket.Send(buffer);
                    }
                    catch (Exception e) { }
                }
            }
        }
        private void BroadcastMessageToAll(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            foreach (User clientSocket in clientSockets)
            {
                try
                {
                    clientSocket.Socket.Send(buffer);
                }
                catch (Exception ex) { }
                
            }
        }

        private void getIP() {
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

                foreach (IPAddress ipAddress in hostEntry.AddressList)
                {
                    if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"IPv4 Address: {ipAddress}");
                        lblIpAddress.Text = ipAddress.ToString();
                    }
                    else if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        Console.WriteLine($"IPv6 Address: {ipAddress}");
                    }
                }

                Console.ReadLine();

            }
            catch (Exception ex)
            {

                
            }
        }
    }
}
