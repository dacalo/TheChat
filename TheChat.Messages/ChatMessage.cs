using System;

namespace TheChat.Messages
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public Type TypeInfo { get; set; }
        public DateTime Timestamp { get; set; }
        public string Sender { get; set; }
        public string GroupName { get; set; }
        public string Recipient { get; set; }
        public bool IsPrivate { get; set; }
        public string Color { get; set; }
        public string Avatar { get; set; }

        public ChatMessage(){}

        public ChatMessage(string sender)
        {
            Id = Guid.NewGuid().ToString();
            TypeInfo = GetType();
            Sender = sender;
            Timestamp = DateTime.Now;
        }
    }
}
