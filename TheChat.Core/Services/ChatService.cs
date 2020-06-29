using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheChat.Core.EventHandlers;
using TheChat.Core.Models;
using TheChat.Messages;

namespace TheChat.Core.Services
{
    public class ChatService : IChatService
    {
        
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//Nos permite delimitar el número de hilos que puede acceder concurrentemente a un recurso
        private HttpClient httpClient;
        HubConnection hub;
        public event EventHandler<MessageEventArgs> OnReceivedMessage;
        
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


            //Conexion al SignalR
            var connectionBuilder = new HubConnectionBuilder();

            connectionBuilder.WithUrl(info.Url, (obj) =>
            {
                obj.AccessTokenProvider = () => Task.Run(() => info.AccessToken);
            });

            hub = connectionBuilder.Build();//Conexión al hub

            await hub.StartAsync();

            ConnectionToken = hub.ConnectionId;

            IsConnected = true;

            hub.On<object>("ReceiveMessage", (message) =>
            {
                var json = message.ToString();
                var obj = JsonConvert.DeserializeObject<ChatMessage>(json);
                var msg = (ChatMessage)JsonConvert.DeserializeObject(json, obj.TypeInfo);
                OnReceivedMessage?.Invoke(this, new MessageEventArgs(msg));
            });

            semaphoreSlim.Release();
        }


        //Permite enviar mensajes a cada uno de los usuarios conectados en el hub
        public async Task SendMessageAsync(ChatMessage message)
        {
            if(!IsConnected)
                throw new InvalidOperationException("Not Connected");

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await httpClient.PostAsync(Config.MessageEndpoint, content);
        }

        public async Task JoinChannelAsync(UserConnectedMessage message)
        {
            if (!IsConnected)
                return;
            
            message.Token = ConnectionToken;
            message.IsEntering = true;

            var json = JsonConvert.SerializeObject(message);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await httpClient.PostAsync(Config.AddToGroupEndpoint, content);

            await SendMessageAsync(message);
        }

        public async Task<List<Room>> GetRooms()
        {
            var rooms = new List<Room>
            {
                new Room
                {
                    Name = "C#",
                    Image = "csharp.png"
                },
                new Room
                {
                    Name = "Xamarin",
                    Image = "xamarin.ong"
                },
                new Room
                {
                    Name = ".Net",
                    Image = "dotnet.png"
                },
                new Room
                {
                    Name = "ASP .NET Core",
                    Image = "aspcore.png"
                },
                new Room
                {
                    Name = "Xamarin Forms",
                    Image = "xamforms.png"
                },
                new Room
                {
                    Name = "Visual Studio",
                    Image = "visualstudio.png"
                },
            };

            foreach (var room in rooms)
            {
                room.UsersNumber = await GetRoomCount(room.Name);
            }

            return rooms;
        }

        public async Task LeaveChannelAsync(UserConnectedMessage message)
        {
            if (!IsConnected)
                return;

            message.Token = ConnectionToken;

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await httpClient.PostAsync(Config.LeaveGroupEndpoint, content);

            await SendMessageAsync(message);
        }

        public async Task<List<User>> GetUsersGroup(string group)
        {
            var url = $"{Config.RoomsEndpoint}/{group}";
            var result = await httpClient.GetStringAsync(url);
            var users = JsonConvert.DeserializeObject<List<User>>(result);
            return users;
        }

        private async Task<int> GetRoomCount(string group)
        {
            var users = await GetUsersGroup(group);
            return users.Count;
        }

        public async  Task<User> GetUser(string userId)
        {
            var url = $"{Config.UserEndpoint}/{userId}";
            var result = await httpClient.GetStringAsync(url);
            var users = JsonConvert.DeserializeObject<List<User>>(result);

            return users.FirstOrDefault();
        }
    }
}
