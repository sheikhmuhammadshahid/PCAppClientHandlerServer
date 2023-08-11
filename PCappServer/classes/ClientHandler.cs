using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PCappServer.classes
{
    class ClientHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private Form1 server;

        public ClientHandler(TcpClient client, Form1 server)
        {
            this.client = client;
            this.server = server;
            stream = client.GetStream();
        }

        public void HandleClient()
        {
            try
            {
                byte[] data = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.ASCII.GetString(data, 0, bytesRead);
                    message = message.TrimEnd('\0'); // Remove null terminator
                    Console.WriteLine("Received: " + message);

                    server.BroadcastMessage(message, this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client disconnected: " + client.Client.RemoteEndPoint);
                server.BroadcastMessage("Client disconnected: " + client.Client.RemoteEndPoint, this);
            }
            finally
            {
                server.RemoveClient(this);
                client.Close();
                stream.Close();
            }
        }
        public void SendMessage(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public void Stop()
        {
            client.Close();
            stream.Close();
        }
    }
}
