using System.Collections.Generic;
namespace WebServer.Models
{
    public class MessageListRequest
    {
        public List<Message> Messages { get; set; }
        
        public int ConversationID { get; set; }
    }
}