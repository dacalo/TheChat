namespace TheChat.Messages
{
    public class PhotoMessage :  ChatMessage
    {
        public string Base64Photo { get; set; }
        public string FileEnding { get; set; }

        public PhotoMessage(){}

        public PhotoMessage(string sender) : base(sender)
        {

        }
    }
}
