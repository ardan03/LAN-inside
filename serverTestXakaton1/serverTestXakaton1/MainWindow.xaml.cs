using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace serverTestXakaton1
{
    public partial class MainWindow : Window
    {
        // Определение класса конфигурации с новым полем Status
        public class Configuratin
        {
            public string Cnn { get; set; }
            public string PcName { get; set; }
            public string osVersion { get; set; }
            public string osBuild { get; set; }
            public string Shield { get; set; }
            public string ShieldVer { get; set; }
            public string FireWallActive { get; set; }
            public string ipAdress { get; set; }
        }

        ObservableCollection<Configuratin> users = new ObservableCollection<Configuratin>();

        public MainWindow()
        {
            InitializeComponent();
            UsersContainer.ItemsSource = users;
        }

        async void BtnStat_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                tcpListener.Bind(ipPoint);
                tcpListener.Listen();
                MessageBox.Show("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    Socket handler = await tcpListener.AcceptAsync();
                    NetworkStream stream = new NetworkStream(handler);

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    StringBuilder sb = new StringBuilder();

                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    } while (bytesRead > 0);

                    string receivedJson = sb.ToString();

                    // Десериализация JSON и добавление пользователя в коллекцию
                    Configuratin user = JsonConvert.DeserializeObject<Configuratin>(receivedJson);
                    users.Add(user);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
