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
                    

                    bool replaced = false;
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (users[i].ipAdress == user.ipAdress)
                        {
                            users[i] = user;
                            replaced = true;
                            break;
                        }
                    }

                    if (!replaced)
                    {
                        users.Add(user); // Если объект с таким IP-адресом не найден, добавляем новый объект в коллекцию
                    }

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
