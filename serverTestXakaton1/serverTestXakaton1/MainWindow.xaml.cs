using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace serverTestXakaton1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Reestr.ItemsSource = restr;
        }

        Configuratin configuratin = new Configuratin();
        ObservableCollection<Configuratin> restr = new ObservableCollection<Configuratin>();
        
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

                    // Десериализация JSON и обновление списка с информацией о конфигурации
                    configuratin = JsonConvert.DeserializeObject<Configuratin>(receivedJson);
                    restr.Add(configuratin);

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