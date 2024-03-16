using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Linq;
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
        Configuratin configuratin = new Configuratin();
        public MainWindow()
        {
            InitializeComponent();
            Reestr.ItemsSource = restr;
        }
        int _countClient = 0;
        ObservableCollection<Configuratin> restr = new ObservableCollection<Configuratin>();
        async void BtnStat_Click(object sender, RoutedEventArgs e)
        {
            
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                tcpListener.Bind(ipPoint);
                tcpListener.Listen();    // запускаем сервер
                MessageBox.Show("Сервер запущен. Ожидание подключений... ");

                while (true)
                {
                    // получаем подключение в виде сокета
                    Socket handler = await tcpListener.AcceptAsync();
                    NetworkStream stream = new NetworkStream(handler);

                    // Создаем буфер для чтения данных по частям
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    StringBuilder sb = new StringBuilder();

                    // Считываем данные по частям
                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    } while (bytesRead > 0);

                    // Преобразуем полученные данные в строку JSON
                    string receivedJson = sb.ToString();

                    // Десериализация JSON и обновление списка с информацией о конфигурации
                    configuratin = JsonConvert.DeserializeObject<Configuratin>(receivedJson);
                    // Закрываем соединени
                    bool replaced = false;
                    for (int i = 0; i < restr.Count; i++)
                    {
                        if (restr[i].ipAdress == configuratin.ipAdress)
                        {
                            restr[i] = configuratin;
                            replaced = true;
                            break;
                        }
                    }

                    if (!replaced)
                    {
                        restr.Add(configuratin); // Если объект с таким IP-адресом не найден, добавляем новый объект в коллекцию
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