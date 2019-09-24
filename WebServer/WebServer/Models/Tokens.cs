using System.Collections.Generic;
namespace WebServer.Models
{
    public class Tokens : ITokens
    {
        
        public string AndroidToken { get; set; }
        public string ClientToken { get; set; }
    }
}