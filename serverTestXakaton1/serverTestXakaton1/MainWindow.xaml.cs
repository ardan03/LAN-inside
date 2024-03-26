using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Configuration;

namespace serverTestXakaton1
{
    public partial class MainWindow : Window
    {
       

        ObservableCollection<Configuratin> users = new ObservableCollection<Configuratin>();
        static string hostName = Dns.GetHostName();
        IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
        public MainWindow()
        {
            InitializeComponent();
            UsersContainer.ItemsSource = users;
            foreach (IPAddress ipA in ipAddresses)
            {
                if (ipA.AddressFamily == AddressFamily.InterNetwork) // Выбираем IPv4 адреса
                {
                    ipSrev.Text = ipA.ToString();
                    break;
                }
            }
        }
        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    SecurityRule securityRule = new SecurityRule();

                    string jsonData = JsonConvert.SerializeObject(securityRule);
                    byte[] jsonDataBytes = Encoding.UTF8.GetBytes(jsonData);
                    await stream.WriteAsync(jsonDataBytes, 0, jsonDataBytes.Length);

                    while (true)
                    {
                        int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (readBytes <= 0)
                            break;

                        string receivedData = Encoding.UTF8.GetString(buffer, 0, readBytes);
                        Configuratin security = JsonConvert.DeserializeObject<Configuratin>(receivedData);
                        users.Add(security);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке клиента: " + ex.Message);
            }
            finally
            {
                client.Close();
                MessageBox.Show("Соединение с клиентом закрыто.");
            }
        }
        async void BtnStat_Click(object sender, RoutedEventArgs e)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8888);
            server.Start();
            MessageBox.Show("Сервер запущен. Ожидание подключений...");

            try
            {
                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();

                    _ = HandleClientAsync(client);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении клиента: " + ex.Message);
            }
            finally
            {
                server.Stop();
                MessageBox.Show("Сервер остановлен.");
            }
        }
    }
}
