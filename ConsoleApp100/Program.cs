using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace ConsoleApp100
{
    internal class Program
    {
        static TcpListener tcpListener;
        static Thread listenerThread;
        static void Main(string[] args)
        {
            Console.WriteLine("=== Server ===");

            Console.Write("Введіть IP-адресу: ");
            string ipAddress = Console.ReadLine();
            Console.Write("Введіть порт: ");
            int port = int.Parse(Console.ReadLine());

            tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.Start();

        }
        private static void ListenForClients()
        {
            tcpListener.Start();

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private static void HandleClientComm(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream clientStream = tcpClient.GetStream();
            Console.WriteLine($"Клієнт {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address} підключився.");

            bool exit = false;

            while (!exit)
            {
                byte[] message = new byte[4096];
                int bytesRead;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                    break;

                string receivedMessage = Encoding.UTF8.GetString(message, 0, bytesRead);
                Console.WriteLine($"Клієнт {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address}: {receivedMessage}");


                if (receivedMessage.Equals("Bye", StringComparison.OrdinalIgnoreCase))
                {
                    exit = true;
                    Console.WriteLine($"Клієнт {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address} відключився.");
                }
                else
                {
                    string response = GetRandomResponse();
                    byte[] sendBytes = Encoding.UTF8.GetBytes(response);
                    clientStream.Write(sendBytes, 0, sendBytes.Length);
                }
            }

            tcpClient.Close();
        }

        private static string GetRandomResponse()
        {
            string[] computerResponses = { "Розумію.", "Цікаво.", "Продовжуйте.", "Якщо це важливо для вас.", "Мені цікаво ваше мислення." };

            Random random = new Random();
            int index = random.Next(computerResponses.Length);
            return computerResponses[index];
        }

    }
}