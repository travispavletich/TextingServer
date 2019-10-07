using System;

namespace WebServer.Models
{
    public class MessageStatus
    {
        public Guid MessageID { get; set; }

        public string Status { get; set; }
    }
}