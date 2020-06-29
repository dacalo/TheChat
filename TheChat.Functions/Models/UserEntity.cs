using Microsoft.Azure.Cosmos.Table;

namespace TheChat.Functions.Models
{
    public class UserEntity : TableEntity
    {
        public string UserId { get; set; }
        public string Room { get; set; }
        public string Color { get; set; }
        public string Avatar { get; set; }
    }
}