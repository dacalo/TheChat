using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using TheChat.Messages;
using TheChat.Functions.Models;

namespace TheChat.Functions
{
    public static class AddToGroup
    {
        [FunctionName("AddToGroup")]
        [return: Table("Users", Connection = "StorageConnection")]
        public static async Task<UserEntity> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "chat")]IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            var message = new JsonSerializer().Deserialize<UserConnectedMessage>(new JsonTextReader(new StreamReader(req.Body)));

            await signalRGroupActions.AddAsync(new SignalRGroupAction
            {
                ConnectionId = message.Token,
                UserId = message.Sender,
                GroupName = message.GroupName,
                Action = GroupAction.Add
            });

            Random r = new Random();
            var red = r.Next(0, 255).ToString("X2");
            var green = r.Next(0, 255).ToString("X2");
            var blue = r.Next(0, 255).ToString("X2");

            var item = new UserEntity
            {
                UserId = message.Sender,
                Room = message.GroupName,
                Color = $"#{red}{green}{blue}",
                Avatar = $"image_{r.Next(1, 51)}.png",
                PartitionKey = message.GroupName,
                RowKey = message.Sender
            };

            return item;
        }
    }
}
