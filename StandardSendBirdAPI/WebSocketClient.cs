using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendBirdAPI
{
    class WSMessage
    {
        public bool Received { set; get; }
        public string Body { set; get; }
    }
    class WebSocketClient
    {
        private static object consoleLock = new object();
        private const int receiveChunkSize = 256;
        private static readonly TimeSpan delay = TimeSpan.FromMilliseconds(30000);
        public List<WSMessage> Messages { set; get; }
        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        public WebSocketClient()
        {
            Messages = new List<WSMessage>();
        }
        public async Task Connect(string uri)
        {
            ClientWebSocket webSocket = null;

            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                await Task.WhenAll(Receive(webSocket), Send(webSocket));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (webSocket != null)
                    webSocket.Dispose();
            }
        }
        static UTF8Encoding encoder = new UTF8Encoding();

        private async Task Send(ClientWebSocket webSocket)
        {
            byte[] buffer = encoder.GetBytes("{\"op\":\"unconfirmed_sub\"}");
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            while (webSocket.State == WebSocketState.Open)
            {
                AddToMessages(false, buffer);
                await Task.Delay(delay);
            }
        }

        private async Task Receive(ClientWebSocket webSocket)
        {
            byte[] buffer = new byte[receiveChunkSize];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    AddToMessages(true, buffer);
                    if (Messages.Any(x => x.Body.Contains("\"key\":")))
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                }
            }
        }

        private void AddToMessages(bool receiving, byte[] buffer)
        {
            Messages.Add(new WSMessage() { Body = encoder.GetString(buffer), Received = receiving });
        }
    }
}
