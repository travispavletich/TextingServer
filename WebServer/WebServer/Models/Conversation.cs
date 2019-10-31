using System.Collections.Generic;

namespace WebServer.Models
{
    public class Conversation
    {
        public int ConversationID { get; set; }
        
        public string MostRecent { get; set; }
        
        public List<string> Participants { get;set; }
        
    }
}