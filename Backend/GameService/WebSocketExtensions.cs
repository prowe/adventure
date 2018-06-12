using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GameService
{
    public static class WebSocketExtensions
    {
        static readonly JsonSerializer serializer = new JsonSerializer();

        public static async Task SendObject(this WebSocket webSocket, object payload)
        {
            StringWriter writer = new StringWriter(); 
            serializer.Serialize(writer, payload);

            var data = new ArraySegment<byte>(Encoding.Unicode.GetBytes(writer.ToString()));
            await webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}