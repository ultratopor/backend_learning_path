using System.Net.Sockets;
using System.Text;

namespace Naked_TCP
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new TcpClient();
            try
            {
                await client.ConnectAsync("127.0.0.1", 8888);
                Console.WriteLine("Подключено к серверу.");
                var stream = client.GetStream();
                _ = ReadLoopAsync(stream);

                while(true)
                {
                    Console.WriteLine("Введите строки для отправки.");
                    string line = Console.ReadLine();
                    byte[] data = Encoding.UTF8.GetBytes(line + "\n");
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static async Task ReadLoopAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Соединение закрыто");
                        break;
                    }

                    if (bytesRead > 0)
                    {
                        string receiveData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Получены данные: {receiveData}");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка чтения: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Цикл чтения завершён");
            }
        }
    }
}
