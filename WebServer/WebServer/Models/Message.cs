using System;

namespace WebServer.Models
{
    public class Message
    {
        public string Sender { get; set; }
        public bool IsSender { get; set; }
        public string MessageBody { get; set; }
        public int ConversationID { get; set; }
        public DateTime TimeStamp { get; set; }
        
        public bool sentSuccessfully { get; set; }
        
    }
}