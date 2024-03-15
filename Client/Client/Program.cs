using System.Net.Sockets;
using System.Text;
using System.Management;
using Newtonsoft.Json;
using Client;


using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    Configuration configuration = new Configuration();
    Console.WriteLine("Введите IP: ");
    string ip = Console.ReadLine();
    Console.WriteLine("Введите порт: ");
    int port = Convert.ToInt32(Console.ReadLine());



    while (true)
    {
        await tcpClient.ConnectAsync(ip, port);

        // Получаем информацию об операционной системе
        configuration.PcName = Environment.MachineName;
        var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        foreach (ManagementObject os in osSearcher.Get())
        {
            configuration.osVersion = os["Caption"].ToString();
            configuration.osBuild = os["BuildNumber"].ToString();
        }

        // Получаем информацию о продукте Kaspersky, если он установлен


        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Product WHERE Name LIKE '%Kaspersky%'");
        ManagementObjectCollection collection = searcher.Get();

        foreach (ManagementObject product in collection)
        {
            configuration.Shield = product["Name"].ToString();
            configuration.ShieldVer = product["Version"].ToString();

            Console.WriteLine($"Антивирусный продукт: {configuration.Shield}");
            Console.WriteLine($"Версия: {configuration.ShieldVer}");
        }

        bool status = IsSystemOnline();
        configuration.Cnn = status;


        // Отправляем данные на сервер
        string json = JsonConvert.SerializeObject(configuration);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await tcpClient.SendAsync(bytes);

        
    }

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

static bool IsSystemOnline()
{
    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
    foreach (ManagementObject os in searcher.Get())
    {
        if (os["LastBootUpTime"] != null)
        {
            DateTime lastBootUpTime = ManagementDateTimeConverter.ToDateTime(os["LastBootUpTime"].ToString());
            DateTime currentTime = DateTime.Now;
            TimeSpan uptime = currentTime - lastBootUpTime;
            if (uptime.TotalSeconds > 0)
                return true; // ПК включен
        }
    }
    return false; // ПК выключен
}
