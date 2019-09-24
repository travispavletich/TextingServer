using System;

namespace WebServer.Models
{
    public class Message
    {
        public string MessageText { get; set; }
        public string Recipient { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}