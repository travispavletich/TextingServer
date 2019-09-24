using System;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebServer.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebServer.Controllers
{
    /// <summary>
    /// Controller for incoming requests from the  client
    /// </summary>
    public class ClientController : Controller
    {
        
        
        
        [HttpGet]
        [Route("Client/Token")]
        public ActionResult<string> Token([FromServices]ITokens tokens, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                tokens.ClientToken = token;
                return Ok("Token Received");
            }
            else
            {
                return BadRequest("Failure. Null or Empty token string");
            }
        }
        
        /******************************************************************
         *                                                                *
         *              Endpoints that call data functions                *
         *                                                                *
         ******************************************************************/
        
        [HttpPost]
        [Route("Client/SendMessage")]
        public ActionResult<ResultStatus> SendMessage([FromServices]ITokens tokens, [FromServices] IConfiguration config, MessageSendRequest request)
        {
            const string firebaseFuncName = "textMessage";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);
            var dict = new Dictionary<string, object>()
            {
                {"Token", androidToken},
                {"Message", request.Message},
                {"Recipients", request.Recipients}
            };
            
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            req.RequestFormat = DataFormat.Json;

            var response = client.Execute(req);
                    
            return Ok(ResultStatus.Success);
        }

        /******************************************************************
         *                                                                *
         *          Endpoints that call notification functions            *
         *                                                                *
         ******************************************************************/
        
        [HttpGet]
        [Route("Client/RequestBulkMessages")]
        public ActionResult<ResultStatus> RequestBulkMessages([FromServices] ITokens tokens,
            [FromServices] IConfiguration config)
        {
            const string firebaseFuncName = "askForBulkMessages";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.GET);

            var response = client.Execute(req);
            
            // Probably check the response and return a result status based on that
            
            return Ok(ResultStatus.Success);
        }
                
    }
}