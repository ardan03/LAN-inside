using System;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Windows;
using Microsoft.Win32;
using NetFwTypeLib;
using Newtonsoft.Json;


namespace ClientWPF
{
    public partial class MainWindow : Window
    {
        private readonly Configuration configuration = new Configuration();
        TcpClient client = new TcpClient();
        private System.Timers.Timer timer;
        
        public MainWindow()
        {
            InitializeComponent();
            
            getConfig();
        }
        private void InitializeTimer()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 30000; // 60000 миллисекунд = 1 минута
            getConfig();
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true; // Установим автоматический повтор
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Метод, вызываемый при каждом истечении интервала времени (1 минута)
            SendDataToServer();
        }

        private void SendDataToServer()
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    // Если клиент не инициализирован или не подключен, то подключаемся к серверу
                    string serverIP = textBoxIp.Text;
                    int port = int.Parse(textBoxPort.Text);
                    client = new TcpClient(serverIP, port);
                }

                GetLocalIPAddress();
                NetworkStream stream = client.GetStream();
                SendConfiguration(stream);
                ReceiveData(stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке данных на сервер: " + ex.Message);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string serverIP = textBoxIp.Text;
            int port = int.Parse(textBoxPort.Text);
             client = new TcpClient(serverIP, port);

            try
            {
                GetLocalIPAddress();
 
                MessageBox.Show("Подключено к серверу.");

                NetworkStream stream = client.GetStream();
                ReceiveData(stream );
                SendConfiguration(stream);

                //MessageBox.Show("Подключение закрыто.");
                InitializeTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к серверу: " + ex.Message);
            }
        }

        private void GetLocalIPAddress()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ipA in ipAddresses)
            {
                if (ipA.AddressFamily == AddressFamily.InterNetwork)
                {
                    configuration.ipAdress = ipA.ToString();
                    break;
                }
            }
        }

        private void SendConfiguration(NetworkStream stream)
        {
            string info = JsonConvert.SerializeObject(configuration);
            byte[] responseDataBytes = Encoding.UTF8.GetBytes(info);
            stream.Write(responseDataBytes, 0, responseDataBytes.Length);
        }

        private void ReceiveData(NetworkStream stream)
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            MessageBox.Show("Полученные данные от сервера: " + receivedData);
            SecurityRule securityRule = JsonConvert.DeserializeObject<SecurityRule>(receivedData);
            if (!CheckSecurityRules(securityRule))
            {
                Console.WriteLine("Перрывание ссесии");
                client.Close();
                this.Close();
            }
        }

        private bool CheckSecurityRules(SecurityRule securityRule)
        {
            bool CorrectlyOS = checkAccuracyOS(securityRule);
            bool CorrectlyFW = checkAccuracyFireWall(securityRule);
            bool CorrectlyAntivirus = checkAccuracyAntivirus(securityRule);
            bool Correct=false;
            if (!CorrectlyOS)
            {
                MessageBox.Show("Не корректная Операционная система");
                return Correct;
            }
   
            if (!CorrectlyFW)
            {
                MessageBox.Show("Не корректные настройки для МЭС");
                return Correct;
            }
            if (!CorrectlyAntivirus)
            {
                MessageBox.Show("Не корректные настройки для Антивируса");
                return Correct;
            }
            return !Correct;
        }

        void getConfig()
        {
            configuration.PcName = Environment.MachineName;
            var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject os in osSearcher.Get())
            {
                configuration.osVersion = os["Caption"].ToString();
                configuration.osBuild = os["BuildNumber"].ToString();
            }
            INetFwMgr manager = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
            if (manager != null)
            {
                // Получаем текущий статус Firewall
                configuration.FireWallActive = manager.LocalPolicy.CurrentProfile.FirewallEnabled;
            }
            configuration.AntivirusActive = IsWindowsDefenderActive();
        }
        static bool IsWindowsDefenderActive()
        {
            try
            {
                // Путь к ключу реестра, который хранит информацию о состоянии Windows Defender
                const string keyPath = @"SOFTWARE\Microsoft\Windows Defender";
                const string valueName = "DisableAntiSpyware";

                // Открываем соответствующий ключ реестра для чтения
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        // Проверяем значение DisableAntiSpyware
                        object value = key.GetValue(valueName);
                        if (value != null && value is int disableAntiSpywareValue)
                        {
                            // Если значение равно 0, то Windows Defender активен
                            return disableAntiSpywareValue == 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений, если произошла ошибка при чтении реестра
                MessageBox.Show("An error occurred while checking Windows Defender status: " + ex.Message);
            }
                return false;

        }
        bool checkAccuracyOS(SecurityRule inputRule)
        {
            foreach (string ver in inputRule.VersionOS)
            {
                if (configuration.osVersion.Contains(ver))
                {
                    return true;
                }
            }
            return false;
        }
        bool checkAccuracyFireWall(SecurityRule inputRule)
        {
            if (inputRule.FireWall == configuration.FireWallActive)
            {
                return false;
            }
            return true;
        }
        bool checkAccuracyAntivirus(SecurityRule inputRule)
        {
            if (inputRule.activeAntivirus != configuration.AntivirusActive)
            {
                return false;
            }
            return true;
        }
    }
}

