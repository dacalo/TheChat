namespace TheChat.Messages
{
    public class UserConnectedMessage : ChatMessage
    {
        public string Token { get; set; }
        public bool IsEntering { get; set; }

        public UserConnectedMessage() { }

        public UserConnectedMessage(string userName, string groupName) 
            : base(userName)
        {
            GroupName = groupName;
        }
    }
}
