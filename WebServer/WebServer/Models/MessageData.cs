using System.Collections.Generic;

namespace WebServer.Models
{
    public class MessageData
    {
        public List<Conversation> Conversations { get; set; }
        
        public Dictionary<int, List<Message>> ConversationToMessages { get; set; }

        public MessageData()
        {
            Conversations = new List<Conversation>();
            ConversationToMessages = new Dictionary<int, List<Message>>();
        }

        public void SortConversations()
        {
            Conversations.Sort();
        }
    }
    
}