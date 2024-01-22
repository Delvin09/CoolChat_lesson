using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CoolChat.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);

            try
            {
                UdpClient scanClient = new UdpClient(AddressFamily.InterNetwork);
                await scanClient.SendAsync(Encoding.UTF8.GetBytes("SCAN BY COOL CHAT SERVER"), new IPEndPoint(IPAddress.Broadcast, 7701));
                var result = await scanClient.ReceiveAsync();
                var message = Encoding.UTF8.GetString(result.Buffer);
                if (!message.StartsWith("YES"))
                {
                    Console.WriteLine("Servers not found!");
                    return;
                }

                var port = int.Parse(message.Split(':')[1]);
                var ip = result.RemoteEndPoint.Address;

                tcpClient.Connect(ip, port/*"192.168.1.253", 7700*/);

                using var stream = tcpClient.GetStream();

                var readTask = Read(stream);
                var writeTask = Write(stream);

                Task.WaitAll(readTask, writeTask);
            }
            catch
            {
                Console.WriteLine("Client was disconnected");
            }
        }

        static Task Read(NetworkStream stream)
        {
            return Task.Run(async () =>
            {
                var endPoint = stream.Socket.LocalEndPoint;
                var reader = new StreamReader(stream);
                while (true)
                {
                    var msg = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        var splittedMsg = msg.Split("@");
                        if (splittedMsg[0] == endPoint?.ToString())
                        {

                        }
                        else
                        {
                            Console.WriteLine(msg);
                        }
                    }
                }
            });
        }

        static Task Write(Stream stream)
        {
            return Task.Run(async () =>
            {
                var writer = new StreamWriter(stream);
                while (true)
                {
                    //Console.Write("Enter the text: ");
                    var text = Console.ReadLine();
                    await writer.WriteLineAsync(text);
                    await writer.FlushAsync();
                }
            });
        }
    }
}
