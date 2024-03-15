using System.Net.Sockets;
using System.Text;
using System.Management;
using Newtonsoft.Json;
using Client;

using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    Configuration configuration = new Configuration();
    Console.WriteLine("Введите ip: ");
    string ip = Console.ReadLine();
    Console.WriteLine("Введите port: ");
    int port = Convert.ToInt32(Console.ReadLine());

    await tcpClient.ConnectAsync(ip, port);
    // сообщение для отправки

    configuration.PcName = Environment.MachineName;
    // отправляем данные
    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
    foreach (ManagementObject os in searcher.Get())
    {
        configuration.osVersion = os["Caption"].ToString();
        configuration.osBuild = os["BuildNumber"].ToString();
    }
    string json = JsonConvert.SerializeObject(configuration);
    byte[] bytes = Encoding.UTF8.GetBytes(json);
    await tcpClient.SendAsync(bytes);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}