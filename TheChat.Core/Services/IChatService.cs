using System.Threading.Tasks;

namespace TheChat.Core.Services
{
    public interface IChatService
    {
        bool IsConnected { get; }
        string ConnectionToken { get; set; }

        Task DisconnectAsync();
        Task InitAsync(string userId);
    }
}
