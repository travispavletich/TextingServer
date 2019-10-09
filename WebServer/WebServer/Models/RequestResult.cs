using System.Collections.Generic;

namespace WebServer.Models
{
    public class RequestResult
    {
        public ResultStatus Status { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorMessage { get; set; }
        
        public Dictionary<string, object> Data { get; set; }

        public RequestResult()
        {
            Data = new Dictionary<string, object>();
        }
    }
}