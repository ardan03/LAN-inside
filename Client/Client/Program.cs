﻿using System.Net.Sockets;
using System.Text;
using System.Management;
using NetFwTypeLib;
using Newtonsoft.Json;
using Client;
using System.Net;


string hostName = Dns.GetHostName();
IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);

try
{
    Configuration configuration = new Configuration();
    Console.WriteLine("Введите IP: ");
    string ip = Console.ReadLine();
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
}

