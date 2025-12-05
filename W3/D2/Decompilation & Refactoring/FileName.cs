using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Decompilation___Refactoring
{
    internal class FileName
    {
        public void OldSchoolDownload(string url, Action<string> onSuccess, Action<Exception> onError)
        {
            var client = new WebClient(); // Устаревший класс, но для примера сойдет
            try
            {
                client.DownloadStringCompleted += (s, e) =>
                {
                    if (e.Error != null) onError(e.Error);
                    else onSuccess(e.Result);
                };
                client.DownloadStringAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                onError(ex);
            }
        }

        public Task<string> DownloadAsyncWrapper(string url)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                var client = new WebClient();

                client.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        tcs.SetException(e.Error);
                    }
                    else
                    {
                        tcs.SetResult(e.Result);
                    }
                };

                client.DownloadStringAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        private static readonly HttpClient _client = new HttpClient();

        public static async Task<string> DownloadAsync(string url)
        {
            try
            {
                
                string content = await _client.GetStringAsync(url);
                return content;
            }
            catch (HttpRequestException ex)
            {
                
                Console.WriteLine($"Ошибка при загрузке {url}: {ex.Message}");
                throw; 
            }
        }
    }
}
