using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class AndroidController : Controller
    {
        // GET
        // Establish the token for the android client
        [HttpGet]
        [Route("Android/token")]
        public ActionResult<string> Token(string token, [FromServices] ITokens tokens, [FromServices] IConfiguration config)
        {
            if (!string.IsNullOrEmpty(token))
            {
                tokens.AndroidToken = token;
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
        [Route("Android/BulkMessages")]
        public ActionResult<string> BulkMessages([FromServices] ITokens tokens, [FromServices] IConfiguration config, List<Message> messages)
        {
            const string firebaseFunc = "bulkMessages";
            var client = new RestClient(config["FirebaseLink"]);
            var clientToken = tokens.ClientToken;
            var req = new RestRequest(firebaseFunc, Method.POST);
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", clientToken},
                {"Message", messages}
            };
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);

            var response = client.Execute(req);
            
            return Ok();
        }

        /******************************************************************
         *                                                                *
         *          Endpoints that call notification functions            *
         *                                                                *
         ******************************************************************/
        
        // Probably change the type of the status
        [HttpGet]
        [Route("Android/SendNewSMSMessageResult")]
        public ActionResult<ResultStatus> SendNewSMSMessageResult([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, string status)
        {
            
            const string firebaseFuncName = "askForBulkMessages";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.GET);

            var response = client.Execute(req);

            return Ok(ResultStatus.Success);
            // Probably check the response and return a result status based on that
        }
    }
}
