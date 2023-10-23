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
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Drawing.Text;

namespace PCappServer
{
    public partial class Form1 : Form
    {
        string pressedBy = "-1";
        string roundType="-1";
        string questionNo = "-1";
        List<string> teamsName=new List<string>();
        string onGoingEvent = "-1";


        MyMessage myMessage = new MyMessage();
        OnGoingEvent onGoingEvents=new OnGoingEvent();
        private Socket listenerSocket;
        private List<User> clientSockets = new List<User>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
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
            try
            {
                while (true)
                {
                    Socket clientSocket = listenerSocket.Accept();
                    Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint);



                    Thread clientThread = new Thread(() => HandleClient(clientSocket));
                    clientThread.Start();
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

        }
        private int isMyMessage(string name) {
            var v = JsonConvert.DeserializeObject<MyMessage>(name);
            if (v.todo != null && v.value != null)
            {
                myMessage = v;
                return 1;
            }
            var vs=JsonConvert.DeserializeObject<OnGoingEvent>(name);
            if (vs.round != null && vs.eventId != 0)
            {
                onGoingEvents = vs;
                return 2;
            }

            return 0;
        }
        

        private void HandleClient(Socket clientSocket)
        {
            string name="";
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = clientSocket.Receive(buffer);
                name = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                myMessage = JsonConvert.DeserializeObject<MyMessage>(name);
                name = myMessage.value;
                if (onGoingEvents.eventId == 0 && name.ToLower() != "admin")
                {
                    myMessage.todo = "issue";
                    myMessage.value = name + " event is not started yet. ";
                    sendMessage(clientSocket, 1);
                    clientSocket.Close();
                }
                else if (!teamsName.Any(s => s.ToLower() == name.ToLower()) && name.ToLower() != "admin" &&name.ToLower() != "admin1")
                {
                    myMessage.todo = "issue";
                    myMessage.value = name+ " is not enrolled in this event.";
                    sendMessage(clientSocket, 1);
                    clientSocket.Close();
                }
                else
                {
                    if (!clientSockets.Any(s => s.Name.ToLower() == name.ToLower()) || name.ToLower() == "admin" || name.ToLower() == "admin1")
                    {
                        try
                        {
                            txtbxconnectedClients.Text += "\nconnected:" + name;

                            if (clientSockets.Any(s => s.Name.ToLower() == name.ToLower()) && (name.ToLower() == "admin" || name.ToLower() == "admin1"))
                            {
                                User user = clientSockets.Where(s => s.Name.ToLower() == name.ToLower()).FirstOrDefault();
                                user.Socket.Close();
                                clientSockets.Remove(user);
                            }
                            clientSockets.Add(new User(name, clientSocket));

                            new Thread(() => { BroadcastMessageToAll(1); }).Start();
                            while (true)
                            {
                                buffer = new byte[1024];
                                bytesRead = clientSocket.Receive(buffer);

                                if (bytesRead == 0)
                                {
                                    txtbxconnectedClients.Text += "\ndisconnected:" + name;
                                    myMessage.value = name;
                                    myMessage.todo = "disconnected";
                                    new Thread(() => { BroadcastMessageToAll(1); }).Start();

                                    clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                                    clientSocket.Close();

                                    break;
                                }

                                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                int checkStatus = isMyMessage(message);

                                if (checkStatus == 1)
                                {
                                    if (myMessage.todo == "eventId")
                                    {
                                        onGoingEvents.eventId = int.Parse(myMessage.value);
                                    }
                                    if (myMessage.todo == "teams")
                                    {
                                        teamsName = myMessage.value.Split(',').ToList();
                                    }
                                    else if (myMessage.todo == "round")
                                    {
                                        roundType = myMessage.value;
                                    }
                                }
                                Console.WriteLine("Received: " + message);
                                txtbxMessage.Text += "\n" + message;

                                new Thread(() => BroadcastMessageToAll(checkStatus)).Start();
                            }
                        }
                        catch (Exception ex)
                        {
                            txtbxconnectedClients.Text += ("disconnected:" + name);
                            myMessage.value = name;
                            myMessage.todo = "disconnected";
                            new Thread(() => BroadcastMessageToAll(1)).Start();
                            Console.WriteLine("Error: " + ex.Message);
                            clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                            clientSocket.Close();
                        }



                    }
                    else
                    {
                        myMessage.todo = "issue";
                        myMessage.value = "Already exists ";
                        sendMessage(clientSocket, 1);
                        clientSocket.Close();
                    }
                }

                
            }
            catch (Exception ex)
            {
                txtbxconnectedClients.Text += ("disconnected:" + name);
                myMessage.todo = "disconnected";
                myMessage.value = name;
                new Thread(() => BroadcastMessageToAll(1)).Start();
                Console.WriteLine("Error: " + ex.Message);
                clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                clientSocket.Close();
            }
        }
        private void handleMessage() {
            try {
                
            } catch (Exception ex) {
                return ;
            } }
        private void sendMessage( Socket tosend,int isMessage) {
            try
            {
                tosend.Send(Encoding.ASCII.GetBytes(isMessage == 1 ? JsonConvert.SerializeObject(myMessage):JsonConvert.SerializeObject(onGoingEvents)));
            }
            catch (Exception ex)
            {

            }
        }
        private void BroadcastMessageExcept(string message, Socket excludeClient,int whatClass)
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
        private void BroadcastMessageToAll(int whatClass)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(whatClass==1? JsonConvert.SerializeObject(myMessage): JsonConvert.SerializeObject(onGoingEvents));

            try
            {
                foreach (User clientSocket in clientSockets)
                {
                    try
                    {
                        new Thread(()=> { clientSocket.Socket.Send(buffer); }).Start();
                    }
                    catch (Exception ex) { }

                }
            }
            catch (Exception ex) { }
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

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
