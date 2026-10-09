using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PromtAiPdfPro.Services
{
    public static class IpcService
    {
        private const string PipeName = "Docentra_IPC_Pipe_v21";
        private static CancellationTokenSource? _cts;

        public static void StartServer(Action<string> onMessageReceived)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                        {
                            // Bekleme işlemini token ile iptal edilebilir hale getiriyoruz
                            using (token.Register(() => server.Close()))
                            {
                                try
                                {
                                    await server.WaitForConnectionAsync(token);
                                }
                                catch (OperationCanceledException)
                                {
                                    break;
                                }
                            }

                            using (var reader = new StreamReader(server))
                            {
                                var message = await reader.ReadToEndAsync();
                                if (!string.IsNullOrEmpty(message))
                                {
                                    onMessageReceived?.Invoke(message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (token.IsCancellationRequested) break;
                        System.Diagnostics.Debug.WriteLine("IPC Server Error: " + ex.Message);
                        await Task.Delay(1000, token);
                    }
                }
            }, token);
        }

        public static void StopServer()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public static async Task<bool> SendMessage(string message)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    await client.ConnectAsync(1000);
                    using (var writer = new StreamWriter(client))
                    {
                        await writer.WriteAsync(message);
                        await writer.FlushAsync();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
