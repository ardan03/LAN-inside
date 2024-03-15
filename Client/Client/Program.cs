using System.Net.Sockets;
using System.Text;
using System.Management;
using Newtonsoft.Json;
using Client;
using System.Net;

//Код для нахождения ip
string hostName = Dns.GetHostName();
IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{


    Configuration configuration = new Configuration();
    Console.WriteLine("Введите ip: ");
    string ip = Console.ReadLine();
    Console.WriteLine("Введите port: ");
    int port = Convert.ToInt32(Console.ReadLine());

    await tcpClient.ConnectAsync(ip, port);
    // Определяем имя пк
    configuration.PcName = Environment.MachineName;
    // отправляем данные
    //Код для определения ОС
    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
    foreach (ManagementObject os in searcher.Get())
    {
        //Версия windows
        configuration.osVersion = os["Caption"].ToString();
        //Номер сборки
        configuration.osBuild = os["BuildNumber"].ToString();
    }

    foreach (IPAddress ipad in ipAddresses)
    {
        if (ipad.AddressFamily == AddressFamily.InterNetwork) // Выбираем IPv4 адреса
        {
            configuration.IpAdress = ip.ToString();
            
            break;
        }
    }

    string json = JsonConvert.SerializeObject(configuration);
    byte[] bytes = Encoding.UTF8.GetBytes(json);
    await tcpClient.SendAsync(bytes);

    

    


}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}