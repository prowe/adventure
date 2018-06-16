using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GameService
{
    public static class WebSocketExtensions
    {
        static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        static readonly JsonSerializer serializer = new JsonSerializer();

        public static async Task SendObject(this WebSocket webSocket, object payload)
        {
            var json = JsonConvert.SerializeObject(payload, serializerSettings);
            System.Console.WriteLine("sending: " + json);
            var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            await webSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}