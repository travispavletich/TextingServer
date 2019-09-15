using Microsoft.AspNetCore.Mvc;
using WebServer.Models;

namespace WebServer.Controllers
{
    /// <summary>
    /// Controller for incoming requests from the  client
    /// </summary>
    public class ClientController : Controller
    {

        [HttpPost]
        [Route("Client/SendMessage")]
        public ActionResult<ResultStatus> SendMessage(MessageSendRequest request)
        {
            return Ok(ResultStatus.Success);
        }
    
                
    }
}