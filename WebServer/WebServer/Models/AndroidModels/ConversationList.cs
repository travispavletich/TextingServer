using System.Collections.Generic;

namespace WebServer.Models.AndroidModels
{
    /// <summary>
    /// Parameter for the ConversationList Post endpoint
    /// </summary>
    public class ConversationList
    {    
        public List<Conversation> Conversations{ get; set; }
    }
}