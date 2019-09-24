namespace WebServer.Models
{
    public class RequestResult
    {
        public ResultStatus Status { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}