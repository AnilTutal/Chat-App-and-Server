using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Chat_App_and_Server
{
    internal class Server
    {

        const int Size = 50;
        const string SERVER = "127.0.0.1";
        int maxClient;
        List<TcpClient> connections = new List<TcpClient>();
        List<string> nicknames = new List<string>();
        TcpListener server;

        public event EventHandler<ConnectionModel>? NewConnectionEstablished;
        public event EventHandler<string>? ConnectionClosed;

        public void Start(int port, int maxcs)
        {
            maxClient = maxcs;

            server = new TcpListener(IPAddress.Parse(SERVER), port);
            server.Start();

            Thread threadMain = new Thread(MainLoop);
            threadMain.Start();
        }

        void Broadcast(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            foreach (TcpClient conn in connections)
            {
                conn.GetStream().Write(messageBytes, 0, messageBytes.Length);
            }
        }

        void HandleClient(TcpClient conn)
        {
            NetworkStream stream = conn.GetStream();

            while (true)
            {
                try
                {
                    byte[] buffer = new byte[Size];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Broadcast(message);
                }
                catch
                {
                    int index = connections.IndexOf(conn);
                    connections.Remove(conn);
                    conn.Close();

                    string nicknamee = nicknames[index];
                    Broadcast($"{nicknamee} has left the chat...");


                    if (index < nicknames.Count)
                    {
                        nicknames.RemoveAt(index);
                    }


                    string nickname = index < nicknames.Count ? nicknames[index] : "Unknown";
                    string endpoint = conn.Client?.RemoteEndPoint?.ToString() ?? "Unknown";
                    OnConnectionClosed(nickname, endpoint);
                    break;
                }
            }
        }

        public void MainLoop()
        {
            while (true)
            {
                TcpClient conn = server.AcceptTcpClient();

                if (connections.Count >= maxClient)
                {
                    conn.Close();
                }
                else
                {
                    NetworkStream stream = conn.GetStream();

                    byte[] buffer = new byte[Size];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string nickname = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    nicknames.Add(nickname);
                    connections.Add(conn);

                    OnNewConnectionEstablished(nickname, conn.Client?.RemoteEndPoint?.ToString() ?? "Unknown");

                    Broadcast($"{nickname}  joined the chat! ");
                    byte[] connectedMessage = Encoding.UTF8.GetBytes("Connected to the server!");
                    stream.Write(connectedMessage, 0, connectedMessage.Length);

                    Thread threadClient = new Thread(() => HandleClient(conn));
                    threadClient.Start();
                }
            }
        }

        protected virtual void OnNewConnectionEstablished(string username, string connectionInfo)
        {
            NewConnectionEstablished?.Invoke(this, new ConnectionModel()
            {
                Id = Guid.NewGuid(),
                Name = username,
                RemoteIpPoint = connectionInfo,
                Status = true,
                Timestampt = DateTime.Now,
            });
        }

        protected virtual void OnConnectionClosed(string username, string connectionInfo)
        {
            ConnectionClosed?.Invoke(this, $"{username} ({connectionInfo}) has disconnected.");
        }
    }
}
