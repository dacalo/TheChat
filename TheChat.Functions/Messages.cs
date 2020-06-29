using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TheChat.Functions.Helpers;
using TheChat.Messages;

namespace TheChat.Functions
{
    public static class Messages
    {
        [FunctionName("Messages")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var serializedObject = new JsonSerializer().Deserialize(new JsonTextReader(new StreamReader(req.Body)));
            
            var message = JsonConvert.DeserializeObject<ChatMessage>(serializedObject.ToString());

            if (message.TypeInfo.Name == nameof(UserConnectedMessage))
            {
                message = JsonConvert.DeserializeObject<UserConnectedMessage>(serializedObject.ToString());
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        GroupName = message.GroupName,
                        Target = "ReceiveMessage",
                        Arguments = new[] { message }
                    });
            }
            else if (message.TypeInfo.Name == nameof(SimpleTextMessage))
            {
                message = JsonConvert.DeserializeObject<SimpleTextMessage>(serializedObject.ToString());
                var signalRMessage = new SignalRMessage
                {
                    Target = "ReceiveMessage",
                    Arguments = new[] { message }
                };

                if(message.GroupName != null)
                {
                    signalRMessage.GroupName = message.GroupName;
                }
                else if(message.Recipient != null)
                {
                    signalRMessage.UserId = message.Recipient;
                }

                await signalRMessages.AddAsync(signalRMessage);
            }
            else if(message.TypeInfo.Name == nameof(PhotoMessage))
            {
                var photoMessage = JsonConvert.DeserializeObject<PhotoMessage>(serializedObject.ToString());

                var bytes = Convert.FromBase64String(photoMessage.Base64Photo);
                var url = await StorageHelper.Upload(bytes, photoMessage.FileEnding);
                message = new PhotoUrlMessage(photoMessage.Sender)
                {
                    Id = photoMessage.Id,
                    Timestamp = photoMessage.Timestamp,
                    Url = url,
                    GroupName = photoMessage.GroupName
                };

                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        GroupName = message.GroupName,
                        Target = "ReceiveMessage",
                        Arguments = new[] { message }
                    });
            }
        }
    }
}
