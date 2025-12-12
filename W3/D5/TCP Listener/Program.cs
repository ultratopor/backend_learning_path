using System.Net.Sockets;
using System.Text;

namespace TCP_Listener
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var listener = new TcpListener(System.Net.IPAddress.Any, 8888);
            listener.Start();
            Console.WriteLine("TCP Listener started on port 8888.");
            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка прослушивания: {ex.Message}");
            }
            
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            var stream = client.GetStream();

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
                        await stream.WriteAsync(buffer, 0, bytesRead);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Цикл чтения завершён");
                client.Close();
            }
        }
    }
}
