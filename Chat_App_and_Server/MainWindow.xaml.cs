using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Chat_App_and_Server
{
    public partial class MainWindow : Window
    {
        private readonly List<ConnectionModel> _connections = [];

        const int SIZE = 256;
        const string SERVER = "127.0.0.1";
        string nickname;
        int port;
        TcpClient client;
        NetworkStream stream;
        bool server_open = false;
        bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionModel ip = new ConnectionModel();


            if (isConnected == true)
            {
                MessageBox.Show("Zaten bağlı...");
                PortT.Clear();
                NicknameT.Clear();
                return;
            }


            try
            {
                port = int.Parse(PortT.Text);
                nickname = NicknameT.Text;

                client = new TcpClient(SERVER, port);
                stream = client.GetStream();

                byte[] buffer = Encoding.UTF8.GetBytes(nickname);
                stream.Write(buffer, 0, buffer.Length);


                Thread threadReceived = new Thread(new ThreadStart(Receive));
                threadReceived.Start();

                isConnected = true;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Bağlanma hatası... " + ex.Message);
            }

        }

        void ServerB_Click(object sender, RoutedEventArgs e)
        {
            Server server = new Server();
            server.NewConnectionEstablished += OnServerNewConnectionEstablished;

            if (server_open == true)
            {
                MessageBox.Show("Server zaten açık...");
                return;
            }

            try
            {
                int port = int.Parse(SPort.Text);
                int maxc = int.Parse(MaxC.Text);
                Thread serverThread = new Thread(() => server.Start(port, maxc));
                serverThread.Start();

                MessageBox.Show("Server başlatıldı...");
            }

            catch (Exception ex)
            {
                MessageBox.Show("Bağlanma hatası... " + ex.Message);
            }

            server_open = true;

        }

        private void OnServerNewConnectionEstablished(object? sender, ConnectionModel connection)
        {
            if (sender is not Server server)
            {
                return;
            }

            if (connection == null)
            {
                return;
            }

            _connections.Add(connection);
            Dispatcher.Invoke(() => TotatlConnectionCount.Text = _connections.Count.ToString());

            ConnectedUserList.Dispatcher.Invoke(() => ConnectedUserList.Items.Add(connection));
        }

        void Receive()
        {
            try
            {
                byte[] buffer = new byte[SIZE];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        IncomingMessage.Dispatcher.Invoke(() => IncomingMessage.AppendText(message + Environment.NewLine));
                    }
                }
            }

            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Server dolu... " + ex.Message));
            }
        }

        void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (stream == null || !client.Connected)
                {
                    MessageBox.Show("Bağlantı sağlanamadı...");
                    return;
                }

                string sendMessage = $"{nickname}: {MessageT.Text}";
                byte[] sendData = Encoding.UTF8.GetBytes(sendMessage);

                stream.Write(sendData, 0, sendData.Length);
                MessageT.Clear();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Mesaj gönderme hatası... " + ex.Message);
            }
        }
    }
}
