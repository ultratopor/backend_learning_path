using System.Diagnostics;
using System.Buffers;

namespace Async_Copy
{
    internal class Program
    {
        private const int BufferSize = 81920;

        static async Task Main(string[] args)
        {
            // Для теста можно создать фиктивный файл через fsutil (Windows):
            // fsutil file createnew test_source.bin 1073741824

            Console.WriteLine("=== Async File Copier (Day 1 Task) ===");

            // Простая валидация аргументов для примера
            if (args.Length < 2)
            {
                Console.WriteLine("Использование: AsyncFileCopier.exe <source> <destination>");
                // Для удобства отладки в IDE, если аргументов нет, зададим дефолтные (если файлы есть)
                // args = new[] { "test_source.bin", "test_dest.bin" };
                return;
            }

            string sourcePath = args[0];
            string destPath = args[1];

            if (!File.Exists(sourcePath))
            {
                Console.Error.WriteLine($"Ошибка: Файл-источник не найден: {sourcePath}");
                return;
            }

            // Настройка отмены операции через Ctrl+C
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("\nЗапрошена отмена операции...");
                cts.Cancel();
                e.Cancel = true; // Предотвращаем немедленное убивание процесса
            };

            // Настройка репортера прогресса
            var progress = new Progress<double>(percent =>
            {
                // Рисуем прогресс бар в одной строке
                DrawTextProgressBar(percent);
            });

            Console.WriteLine($"Копирование: {Path.GetFileName(sourcePath)} -> {Path.GetFileName(destPath)}");
            Console.WriteLine($"Размер буфера: {BufferSize / 1024} KB");
            Console.WriteLine("Нажмите Ctrl+C для отмены.\n");

            var sw = Stopwatch.StartNew();

            try
            {
                await CopyFileAsync(sourcePath, destPath, progress, cts.Token);

                sw.Stop();
                Console.WriteLine($"\n\nУспешно завершено за {sw.Elapsed.TotalSeconds:F2} сек.");

                // Расчет средней скорости
                long fileSize = new FileInfo(destPath).Length;
                double speedMb = (fileSize / 1024.0 / 1024.0) / sw.Elapsed.TotalSeconds;
                Console.WriteLine($"Средняя скорость: {speedMb:F2} MB/s");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n\nКопирование отменено пользователем.");
                // Очистка частично скопированного файла
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                    Console.WriteLine("Частичный файл удален.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\nКритическая ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Основной метод копирования.
        /// Реализует паттерн "Истинный Асинхронный I/O".
        /// </summary>
        public static async Task CopyFileAsync(string source, string destination, IProgress<double> progress, CancellationToken token)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

            await using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, fileOptions);
            await using var destStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, fileOptions);

            long totalBytes = sourceStream.Length;
            destStream.SetLength(totalBytes);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            { 
                long totalRead = 0;
                int bytesRead;

                double lastReportedProgress = 0;

                while ((bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, BufferSize), token)) > 0)
                {
                    // Запись в целевой файл
                    await destStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);

                    totalRead += bytesRead;

                    // Расчет прогресса
                    double currentProgress = totalBytes > 0 ? (double)totalRead / totalBytes : 1.0;

                    // Репортим только если прогресс изменился хотя бы на 1% или это конец, 
                    // чтобы не блокировать поток частыми вызовами делегата
                    if (currentProgress - lastReportedProgress >= 0.01 || totalRead == totalBytes)
                    {
                        progress?.Report(currentProgress);
                        lastReportedProgress = currentProgress;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void DrawTextProgressBar(double progress)
        {
            const int barSize = 50;
            int progressChars = (int)(progress * barSize);

            Console.Write("\r["); // Возврат каретки в начало строки
            Console.Write(new string('#', progressChars));
            Console.Write(new string('-', barSize - progressChars));
            Console.Write($"] {progress * 100:F1}%");
        }
    }
}
