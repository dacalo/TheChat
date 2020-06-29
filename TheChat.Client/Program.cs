using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheChat.Core.Helpers;
using TheChat.Core.Models;
using TheChat.Core.Services;
using TheChat.Messages;

namespace TheChat.Client
{
    class Program
    {
        static ChatService _service;
        static string _userName;
        static string _room;

        static async Task Main(string[] args)
        {
            Console.WriteLine("User Name: ");
            _userName = Console.ReadLine();

            _service = new ChatService();
            _service.OnReceivedMessage += _service_OnReceivedMessage;

            await _service.InitAsync(_userName);

            Console.WriteLine("You are now connected");
            await JoinRoom();

            var keepGoing = true;

            do
            {
                var text = Console.ReadLine();

                if(text == "exit")
                {
                    await _service.DisconnectAsync();
                    keepGoing = false;
                }
                else if (text == "leave")
                {
                    var message = new UserConnectedMessage(_userName, _room);
                    await _service.LeaveChannelAsync(message);
                    await JoinRoom();
                }
                else if(text == "private")
                {
                    Console.WriteLine("Enter UserName: ");
                    var user = Console.ReadLine();

                    Console.WriteLine("Enter private message: ");
                    text = Console.ReadLine();

                    var message = new SimpleTextMessage(_userName)
                    {
                        Text = text,
                        Recipient = user,
                    };
                    await _service.SendMessageAsync(message);
                }
                else if(text == "image")
                {
                    var imagePath = @"C:\temp\Pictures\tests\emma.jpg";
                    var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
                    var bytes = ImageHelper.ReadFully(imageStream);
                    var base64Photo = Convert.ToBase64String(bytes);
                    var message = new PhotoMessage(_userName)
                    {
                        Base64Photo = base64Photo,
                        FileEnding = imagePath.Split('.').Last(),
                        GroupName = _room
                    };
                    await _service.SendMessageAsync(message);
                }
                else
                {
                    var message = new SimpleTextMessage(_userName)
                    {
                        Text = text,
                        GroupName = _room
                    };
                    await _service.SendMessageAsync(message);
                }
            } while (keepGoing);
        }

        private static async  Task JoinRoom()
        {
            var rooms = await _service.GetRooms();
            foreach (var room in rooms)
            {
                Console.WriteLine(room.Name);
            }

            _room = Console.ReadLine();
            var message = new UserConnectedMessage(_userName, _room);
            await _service.JoinChannelAsync(message);
            var usersInRoom = await _service.GetUsersGroup(_room);
            Console.WriteLine($"There are currently {usersInRoom.Count} users in the room");
        }

        private static void _service_OnReceivedMessage(object sender, Core.EventHandlers.MessageEventArgs e)
        {
            if (e.Message.Sender == _userName)
                return;
            if (e.Message.TypeInfo.Name == nameof(SimpleTextMessage))
            {
                var simpleText = e.Message as SimpleTextMessage;
                var message = $"{simpleText.Sender}:{simpleText.Text}";
                Console.WriteLine(message);
            }
            else if (e.Message.TypeInfo.Name == nameof(UserConnectedMessage))
            {
                var userConnected = e.Message as UserConnectedMessage;
                string message = string.Empty;
                if (userConnected.IsEntering)
                {
                    message = $"{userConnected.Sender} has connected";
                }
                else
                {
                    message = $"{userConnected.Sender} has left";
                }
                Console.WriteLine(message);
            }
            else if (e.Message.TypeInfo.Name == nameof(PhotoUrlMessage))
            {
                var photoMessage = e.Message as PhotoUrlMessage;
                string message = $"{photoMessage.Sender} sent {photoMessage.Url}";
                Console.WriteLine(message);
            }
        }
    }
}
