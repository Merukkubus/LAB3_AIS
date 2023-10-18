using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientApp
{
    class Client
    {
        private static int client_port = 8001;
        private static int server_port = 8000;
        private static string ip = "127.0.0.1";
        static UdpClient udpClient;

        public static void PrintMenu()
        {
            Console.WriteLine("Что вы хотите сделать?" + '\n' +
            "1. Вывести все строки файла. " + '\n' +
            "2. Вывести запись по номеру." + '\n' +
            "3. Записать новые данные в файл." + '\n' +
            "4. Удалить запись из файла." + '\n' +
            "esc. Завершить работу." + '\n');
        }
        private static void StartClient()
        {
            PrintMenu();
            string n = "";
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    Console.Clear();
                    SendRequest("get_all");
                    RecieveMessage();
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    Console.Clear();
                    Console.WriteLine("Какую запись вывести?");
                    n = Console.ReadLine();
                    IntCheck(n);
                    SendRequest("get_by_id|" + n.ToString());
                    RecieveMessage();
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    Console.Clear();
                    Console.WriteLine("Введите строку через запятую:");
                    string line = Console.ReadLine();
                    string[] req = line.Split(',');
                    if (req.Length != 5)
                    {
                        Console.WriteLine("Неверный формат");
                        StartClient();
                    }
                    try
                    {
                        int.Parse(req[0]);
                        int.Parse(req[3]);
                        byte.Parse(req[4]);
                    }
                    catch
                    {
                        Console.WriteLine("Неверный формат");
                        StartClient();
                    }
                    SendRequest("add|" + line);
                    RecieveMessage();
                    Console.ReadLine();
                    Console.Clear();
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    Console.Clear();
                    Console.WriteLine("Какую запись удалить?");
                    n = Console.ReadLine();
                    IntCheck(n);
                    SendRequest("delete|" + n.ToString());
                    RecieveMessage();
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Некорректный выбор. Попробуйте еще раз.");
                    StartClient();
                    break;
            }
            Console.WriteLine();
        }
        private static void RecieveMessage()
        {
            var remoteIP = (IPEndPoint)udpClient.Client.LocalEndPoint;
            try
            {
                byte[] data = udpClient.Receive(ref remoteIP);
                string message = Encoding.Unicode.GetString(data);
                Console.WriteLine(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static void SendRequest(string msg)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(msg);
                udpClient.Send(data, data.Length, ip, server_port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void IntCheck(string value)
        {
            try
            {
                int.Parse(value);
            }
            catch
            {
                Console.WriteLine("Неверный формат");
                StartClient();
            }
            if (int.Parse(value) < 0)
            {
                Console.WriteLine("Неверный формат");
                StartClient();
            }
        }
        static void Main(string[] args)
        {
            udpClient = new UdpClient(client_port);
            Console.WriteLine($"Клиент запущен. Порт: {client_port}");
            do
            {
                StartClient();
            } while (true);
        }
    }
}