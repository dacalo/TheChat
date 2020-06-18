using System;
using System.Threading.Tasks;
using TheChat.Core.Services;

namespace TheChat.Client
{
    class Program
    {
        static ChatService _service;
        static string _userName;

        static async Task Main(string[] args)
        {
            Console.WriteLine("User Name: ");
            _userName = Console.ReadLine();

            _service = new ChatService();

            await _service.InitAsync(_userName);

            Console.WriteLine("You are now connected");

            var keepGoing = true;

            do
            {
                var text = Console.ReadLine();
                if(text == "exit")
                {
                    await _service.DisconnectAsync();
                    keepGoing = false;
                }

            } while (keepGoing);
        }
    }
}
