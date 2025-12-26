using System.Diagnostics;

namespace envdir
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // 1. Проверка аргументов
            // Нам нужно минимум 2 аргумента: <путь к envdir> <команда>
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: envdir <directory> <command> [args...]");
                return 1;
            }

            string envDirectory = args[0];
            string commandToRun = args[1];

            // Собираем аргументы для целевой команды (все, что идет после команды)
            // Мы используем Skip(2), чтобы пропустить dir и cmd
            string[] cmdArgs = args.Skip(2).ToArray();

            // 2. Валидация директории
            if (!Directory.Exists(envDirectory))
            {
                Console.Error.WriteLine($"Error: Directory '{envDirectory}' does not exist.");
                return 111; // Стандартный код ошибки envdir при проблемах с IO
            }

            try
            {
                // 3. Настройка ProcessStartInfo
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = commandToRun,
                    UseShellExecute = false, 
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true,  
                    CreateNoWindow = true          // Не создавать отдельное окно
                };

                // Добавляем аргументы.
                foreach (var arg in cmdArgs)
                {
                    psi.ArgumentList.Add(arg);
                }

                // 4. Чтение файлов и заполнение окружения
                string[] files = Directory.GetFiles(envDirectory);

                foreach (string filePath in files)
                {
                    string varName = Path.GetFileName(filePath);

                    // envdir обычно игнорирует файлы, начинающиеся с точки (скрытые)
                    if (varName.StartsWith(".")) continue;

                    try
                    {
                        // Читаем содержимое
                        string content = await File.ReadAllTextAsync(filePath);

                        if (string.IsNullOrEmpty(content))
                        {
                            if (psi.Environment.ContainsKey(varName))
                            {
                                psi.Environment.Remove(varName);
                            }
                        }
                        else
                        {
                            // Устанавливаем или перезаписываем переменную
                            psi.Environment[varName] = content.Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Warning: Could not read file '{varName}': {ex.Message}");
                    }
                }

                // 5. Запуск процесса
                using Process process = new Process();
                process.StartInfo = psi;

                // 6. Настройка асинхронного перенаправления вывода
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.Error.WriteLine(e.Data);
                    }
                };

                process.Start();

                // Важно: начинаем чтение потоков ПОСЛЕ запуска
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 7. Ожидание завершения
                await process.WaitForExitAsync();

                // Возвращаем код завершения дочернего процесса
                return process.ExitCode;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // Ошибка возникает, если "команда" не найдена или не является исполняемым файлом
                Console.Error.WriteLine($"Error executing command '{commandToRun}': {ex.Message}");
                return 127; // Код "Command not found"
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Critical error: {ex.Message}");
                return 111;
            }
        }
    }
}
