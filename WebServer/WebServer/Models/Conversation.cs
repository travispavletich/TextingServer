using System;
using System.Collections.Generic;

namespace WebServer.Models
{
    public class Conversation : IComparable<Conversation>
    {
        public int ConversationID { get; set; }
        
        public string MostRecent { get; set; }
        
        public DateTime MostRecentTimestamp { get; set; }
        
        public List<string> Participants { get;set; }
        
        public List<string> Contacts { get; set; }

        public int CompareTo(Conversation other)
        {
            return other.MostRecentTimestamp.CompareTo(this.MostRecentTimestamp);
        }
    }
}