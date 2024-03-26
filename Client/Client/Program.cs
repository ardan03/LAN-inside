using System.Net.Sockets;
using System.Text;
using System.Management;
using NetFwTypeLib;
using Newtonsoft.Json;
using Client;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Win32;


Configuration configuration = new Configuration();
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

string hostName = Dns.GetHostName();
IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);

/*try
{
    Configuration configuration = new Configuration();
    Console.WriteLine("Введите IP: ");
    string ip = Console.ReadLine();
    Console.WriteLine("Введите порт:");
    int port = int.Parse(Console.ReadLine());

    while (true)
    {
        using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        await tcpClient.ConnectAsync(ip, port);

        // Получаем информацию об операционной системе
        configuration.PcName = Environment.MachineName;
        var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        foreach (ManagementObject os in osSearcher.Get())
        {
            configuration.osVersion = os["Caption"].ToString();
            configuration.osBuild = os["BuildNumber"].ToString();
        }
        if(!configuration.osVersion.Contains("11") && !configuration.osVersion.Contains("10"))
        {
            Console.WriteLine("An operating system update is required");
            configuration.osVersion += "(Error)";
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
        if (configuration.ShieldVer == null || configuration.ShieldVer.Contains("21.16.6.467"))
        {
            Console.WriteLine("An antivirus system update is required");
            configuration.ShieldVer = "Error";
            configuration.Shield = "Error";
        }

        INetFwMgr manager = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr")) as INetFwMgr;
        if (manager != null)
        {
            // Получаем текущий статус Firewall
            configuration.FireWallActive = manager.LocalPolicy.CurrentProfile.FirewallEnabled;
        }
        else
        {
            Console.WriteLine("Failed to access Windows Firewall settings.");
        }
        foreach (IPAddress ipA in ipAddresses)
        {
            if (ipA.AddressFamily == AddressFamily.InterNetwork) // Выбираем IPv4 адреса
            {
                configuration.ipAdress = ipA.ToString();
                break;
            }
        }
        bool isPCRunning = System.Diagnostics.Process.GetProcesses().Any(p => p.ProcessName.ToLower().Contains("explorer"));
        configuration.Cnn = isPCRunning;
        string json = JsonConvert.SerializeObject(configuration);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await tcpClient.SendAsync(bytes);
        await System.Threading.Tasks.Task.Delay(5000);
    }

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}*/
// IP адрес и порт сервера
string serverIP;
int port;
Console.WriteLine("Введите IP: ");
serverIP = Console.ReadLine();
Console.WriteLine("Введите порт:");
port = int.Parse(Console.ReadLine());
getConfig();
foreach (IPAddress ipA in ipAddresses)
{
    if (ipA.AddressFamily == AddressFamily.InterNetwork) // Выбираем IPv4 адреса
    {
        configuration.ipAdress = ipA.ToString();
        break;
    }
}
try
{
    // Создаем TCP клиент и подключаемся к серверу
    TcpClient client = new TcpClient(serverIP, port);
    Console.WriteLine("Подключено к серверу.");

    // Получаем сетевой поток для чтения данных
    NetworkStream stream = client.GetStream();

    // Получаем ответ от сервера
    byte[] buffer = new byte[client.ReceiveBufferSize];
    int bytesRead = stream.Read(buffer, 0, buffer.Length);
    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    Console.WriteLine("Полученные данные от сервера: " + receivedData);
    SecurityRule securityRule = JsonConvert.DeserializeObject<SecurityRule>(receivedData);
    bool CorrectlyOS = checkAccuracyOS(securityRule);
    bool CorrectlyFW = checkAccuracyFireWall(securityRule);
    bool CorrectlyAnivirus = checkAccuracyAntivirus(securityRule);
    if (!CorrectlyOS) Console.WriteLine("Не корректная Операционая система");
    if (!CorrectlyFW)
    {
        Console.WriteLine("Не коректные настройки для МЭС");
        return;
    }
    if (!CorrectlyAnivirus)
    {
        Console.WriteLine("Не коректные настройки для Антивируса");
        return;
    }

   
    while (true)
    {
        
            string info = JsonConvert.SerializeObject(configuration);
            byte[] responseDataBytes = Encoding.UTF8.GetBytes(info);
            stream.Write(responseDataBytes, 0, responseDataBytes.Length);
            
        
      
        
    }
    // Закрываем соединение
    client.Close();
    Console.WriteLine("Подключение закрыто.");
}
catch (Exception ex)
{
    Console.WriteLine("Ошибка при подключении к серверу: " + ex.Message);
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
        Console.WriteLine("An error occurred while checking Windows Defender status: " + ex.Message);
    }

    // По умолчанию считаем, что Windows Defender не активен
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
    if (inputRule.FireWall != configuration.FireWallActive)
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

