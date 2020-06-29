namespace TheChat.Messages
{
    public class PhotoUrlMessage : ChatMessage
    {
        public string Url { get; set; }

        public PhotoUrlMessage(){}

        public PhotoUrlMessage(string sender) : base(sender)
        {

        }
    }
}
