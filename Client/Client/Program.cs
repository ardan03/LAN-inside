using System.Net.Sockets;
using System.Text;
using System.Management;

using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    Console.WriteLine("Введите ip: ");
    string ip = Console.ReadLine();
    Console.WriteLine("Введите port: ");
    int port = Convert.ToInt32(Console.ReadLine());

    await tcpClient.ConnectAsync(ip, port);
    // сообщение для отправки

    var computerName = Environment.MachineName;

    // считыванием строку в массив байт
    byte[] requestData = Encoding.UTF8.GetBytes(computerName + " ");
    // отправляем данные
    await tcpClient.SendAsync(requestData);
    Console.WriteLine("Сообщение отправлено");
    var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
    foreach (ManagementObject os in searcher.Get())
    {
        string version = os["Caption"].ToString();
        string buildNumber = os["BuildNumber"].ToString();
        byte[] versionOS = Encoding.UTF8.GetBytes(version);
        byte[] buildNum = Encoding.UTF8.GetBytes(buildNumber);
        // отправляем данные
        await tcpClient.SendAsync(versionOS);
        await tcpClient.SendAsync(buildNum);

    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}