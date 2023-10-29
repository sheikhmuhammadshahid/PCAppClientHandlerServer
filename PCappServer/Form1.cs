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
using System.Runtime.Remoting.Messaging;

namespace PCappServer
{
    public partial class Form1 : Form
    {
        string pressedBy = "-1";
        string roundType = "-1";
        string questionNo = "-1";
        List<string> teamsName = new List<string>();
        string onGoingEvent = "-1";


        /* MyMessage myMessage = new MyMessage();*/
        OnGoingEvent onGoingEvents = new OnGoingEvent();
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
            listenerSocket.Bind(new IPEndPoint(getIP(), 1234));
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
                /*    Console.WriteLine("Client connected: " + clientSocket.RemoteEndPoint);*/

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
                /*myMessage*//* = v;*/
                return 1;
            }
            var vs = JsonConvert.DeserializeObject<OnGoingEvent>(name);
            if (vs.round != null && vs.eventId != 0)
            {
                onGoingEvents = vs;
                return 2;
            }

            return 0;
        }


        private void HandleClient(Socket clientSocket)
        {
            MyMessage myMessage = new MyMessage();
            string name = "";
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
                    sendMessage(clientSocket, 1, myMessage);
                    clientSocket.Close();
                }
                else if (!teamsName.Any(s => s.ToLower() == name.ToLower()) && name.ToLower() != "admin" && name.ToLower() != "admin1")
                {
                    myMessage.todo = "issue";
                    myMessage.value = name + " is not enrolled in this event.";
                    sendMessage(clientSocket, 1, myMessage);
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

                            new Thread(() => { BroadcastMessageToAll(1, myMessage);

                                if (name != "admin")
                                {

                                    myMessage.todo = "eventId";
                                    myMessage.value = onGoingEvents.eventId.ToString();
                                    txtbxMessage.Text += "\n" + myMessage.value;
                                    sendMessage(clientSocket, 1, myMessage);

                                    //  sendMessage(clientSocket,2,myMessage);

                                }


                                myMessage.todo = "Teamsconnected";
                                myMessage.value = string.Join(",", clientSockets.Select(s => s.Name).ToArray());

                                txtbxMessage.Text += "\n" + myMessage.value;
                                sendMessage(clientSocket, 1, myMessage);


                            }).Start();
                            /*if (name == "admin1")
                            {
                                myMessage.todo = "eventId";
                                myMessage.value = onGoingEvent;
                                sendMessage(clientSocket, 1, myMessage);
                            }*/

                            while (true)
                            {
                                buffer = new byte[3024];
                                bytesRead = clientSocket.Receive(buffer);

                                if (bytesRead == 0)
                                {
                                    txtbxconnectedClients.Text += "\ndisconnected:" + name;
                                    myMessage.value = name;
                                    myMessage.todo = "disconnected";
                                    new Thread(() => { BroadcastMessageToAll(1, myMessage); }).Start();

                                    clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                                    clientSocket.Close();

                                    break;
                                }

                                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                int checkStatus = isMyMessage(message);

                                if (checkStatus == 2)
                                {
                                    pressedBy = "-1";
                                }
                                else if (checkStatus == 1)
                                {
                                    myMessage = JsonConvert.DeserializeObject<MyMessage>(message);
                                    if (myMessage.todo == "buzzer")
                                    {
                                        lock (pressedBy)
                                        {
                                            if (pressedBy == "-1")
                                            {
                                                Console.WriteLine("Received: " + message);
                                                txtbxMessage.Text += "\n" + message;
                                                pressedBy = myMessage.value;
                                                new Thread(() => BroadcastMessageToAll(checkStatus, myMessage)).Start();
                                            }

                                        }
                                    }
                                    else if (myMessage.todo == "eventId")
                                    {
                                        onGoingEvents.eventId = int.Parse(myMessage.value);
                                    }
                                    else if (myMessage.todo == "clear")
                                    {
                                        pressedBy = "-1";
                                        roundType = "-1";
                                        questionNo = "-1";
                                        teamsName = new List<string>();
                                        onGoingEvent = "-1";


                                        /* MyMessage myMessage = new MyMessage();*/
                                         onGoingEvents = new OnGoingEvent();

                                        foreach (var item in clientSockets)
                                        {
                                            try
                                            {
                                                item.Socket.Close();
                                            }
                                            catch (Exception e) { }
                                        }
                                        clientSockets = new List<User>();
                                    } 
                                    else if (myMessage.todo == "teams")
                                    {
                                        teamsName = myMessage.value.Split(',').ToList();
                                    }
                                    else if (myMessage.todo == "round")
                                    {
                                        roundType = myMessage.value;
                                    }
                                }
                                if (myMessage.todo != "buzzer")
                                {
                                    Console.WriteLine("Received: " + message);
                                    txtbxMessage.Text += "\n" + message;
                                    new Thread(() => BroadcastMessageToAll(checkStatus, myMessage)).Start();
                                }
                              
                            }
                        } 
                        catch (Exception ex)
                        {
                            txtbxconnectedClients.Text += ("\ndisconnected:" + name);
                            myMessage.value = name;
                            myMessage.todo = "disconnected";
                            new Thread(() => BroadcastMessageToAll(1, myMessage)).Start();
                            Console.WriteLine("Error: " + ex.Message);
                            clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                            clientSocket.Close();
                        }



                    }
                    else
                    {
                        myMessage.todo = "issue";
                        myMessage.value = "Already exists ";
                        sendMessage(clientSocket, 1,myMessage);
                        clientSocket.Close();
                    }
                }

                
            }
            catch (Exception ex)
            {
                txtbxconnectedClients.Text += ("\ndisconnected:" + name);
                myMessage.todo = "disconnected";
                myMessage.value = name;
                new Thread(() => BroadcastMessageToAll(1,myMessage)).Start();
                Console.WriteLine("Error: " + ex.Message);
                clientSockets.Remove(clientSockets.Where(s => s.Socket == clientSocket).SingleOrDefault());
                clientSocket.Close();
            }
        }
     
        private void sendMessage( Socket tosend,int isMessage,MyMessage myMessage) {
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
        private void BroadcastMessageToAll(int whatClass,MyMessage myMessag)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(whatClass==1? JsonConvert.SerializeObject(myMessag): JsonConvert.SerializeObject(onGoingEvents));

            try
            {
                foreach (User clientSocket in clientSockets)
                {
                    try
                    {
                        
                            try
                            {
                                clientSocket.Socket.Send(buffer);
                            } catch (Exception ex) { }
                    }
                    catch (Exception ex) { }

                }
               
            }
            catch (Exception ex) { }
        }

        private IPAddress getIP() {
            IPAddress iPAddress = null;
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
                        iPAddress= ipAddress;
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
            return iPAddress;
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
