using System;
using System.Collections.Generic;
namespace WebServer.Models
{
    /// <summary>
    /// Model for a Request to Send a message
    /// </summary>
    public class MessageSendRequest
    {
        /// <summary>
        /// Message Text
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// List of recipients (often will just have one entry)
        /// </summary>
        public List<string> Recipients { get; set; }
        
        /// <summary>
        /// Message unique ID for client use
        /// </summary>
        public Guid MessageID { get; set; }
        
        
    }
}