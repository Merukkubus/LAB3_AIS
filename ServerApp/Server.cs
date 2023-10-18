using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ServerApp
{
    class Server
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static int client_port = 8001;
        private static int server_port = 8000;
        private static string ip = "127.0.0.1";
        static UdpClient udpClient;
        private static async Task SendMessageAsync(string msg)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(msg);
                await udpClient.SendAsync(data, data.Length, ip, client_port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static async Task<string> ReceiveMessageAsync()
        {
            var remoteIP = (IPEndPoint)udpClient.Client.LocalEndPoint;
            string message = "";
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                message = Encoding.Unicode.GetString(result.Buffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return message;
        }
        static async Task Main(string[] args)
        {
            try
            {
                udpClient = new UdpClient(server_port);
                Console.WriteLine($"Сервер запущен. Порт: {server_port}");
                await Task.Run(() => ProcessingRequest());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static async Task ProcessingRequest()
        {
            Controller cr = new Controller();
            while (true)
            {
                string option = await ReceiveMessageAsync();
                string[] req = option.Split('|');
                switch (req[0])
                {
                    case "get_all":
                        {
                            Console.WriteLine($"Запрос всех записей. {client_port}");
                            try
                            {
                                await SendMessageAsync(cr.GetAll());
                            }
                            catch (Exception e)
                            {
                                await SendMessageAsync(e.Message);
                                logger.Error(e.ToString());
                            }
                            break;
                        }
                    case "get_by_id":
                        {
                            Console.WriteLine($"Запрос записи по номеру. {client_port}");
                            try
                            {
                                int id = int.Parse(req[1]);
                                await SendMessageAsync(cr.GetSeparate(id));
                            }
                            catch (Exception e)
                            {
                                await SendMessageAsync(e.Message);
                                logger.Error(e.ToString());
                            }
                            break;
                        }
                    case "add":
                        {
                            Console.WriteLine($"Запрос на добавление записи. {client_port}");
                            try
                            {
                                cr.AddToFile(req[1]);
                                await SendMessageAsync("Запись добавлена.\n");
                            }
                            catch (Exception e)
                            {
                                await SendMessageAsync(e.Message);
                                logger.Error(e.ToString());
                            }
                            break;
                        }
                    case "delete":
                        {
                            Console.WriteLine($"Запрос на удаление записи. {client_port}");
                            try
                            {
                                int id = int.Parse(req[1]);
                                if (cr.Delete(id))
                                {
                                    await SendMessageAsync("Запись удалена.\n");
                                }
                                else
                                {
                                    await SendMessageAsync("Неверное значение.\n");
                                }
                            }
                            catch (Exception e)
                            {
                                await SendMessageAsync(e.Message);
                                logger.Error(e.ToString());
                            }
                            break;
                        }
                    default:
                        {
                            await SendMessageAsync($"Неверный ввод {option}\n");
                            break;
                        }
                }
            }
        }
    }
}