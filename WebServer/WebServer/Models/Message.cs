using System;

namespace WebServer.Models
{
    public class Message : IComparable<Message>
    {
        public string Sender { get; set; }
        public bool IsSender { get; set; }
        public string MessageBody { get; set; }
        public int ConversationID { get; set; }
        public DateTime TimeStamp { get; set; }
        
        public bool sentSuccessfully { get; set; }
        
        public int CompareTo(Message other)
        {
            return this.TimeStamp.CompareTo(other.TimeStamp);
        }
    }
}