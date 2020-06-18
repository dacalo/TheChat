using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheChat.Core.Models;

namespace TheChat.Core.Services
{
    public class ChatService : IChatService
    {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private HttpClient httpClient;
        HubConnection hub;
        public bool IsConnected { get; set; }
        public string ConnectionToken { get; set; }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;

            try
            {
                await hub.DisposeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            IsConnected = false;
        }

        public async Task InitAsync(string userId)
        {
            await semaphoreSlim.WaitAsync();
            
            if (httpClient == null)
                httpClient = new HttpClient();

            var result = await httpClient.GetStringAsync($"{Config.NegotiateEndpoint}/{userId}");

            var info = JsonConvert.DeserializeObject<ConnectionInfo>(result);

            var connectionBuilder = new HubConnectionBuilder();

            connectionBuilder.WithUrl(info.Url, (obj) =>
            {
                obj.AccessTokenProvider = () => Task.Run(() => info.AccessToken);
            });

            hub = connectionBuilder.Build();

            await hub.StartAsync();

            ConnectionToken = hub.ConnectionId;

            IsConnected = true;

            semaphoreSlim.Release();
        }
    }
}
