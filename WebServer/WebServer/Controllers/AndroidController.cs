using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebServer.Models;

namespace WebServer.Controllers
{
    public class AndroidController : Controller
    {
        // GET
        // Establish the token for the android client
        [HttpGet]
        [Route("Android/token")]
        public ActionResult<RequestResult> Token(string token, [FromServices] ITokens tokens, [FromServices] IConfiguration config)
        {
            var result = new RequestResult();
            
            if (!string.IsNullOrEmpty(token))
            {
                tokens.AndroidToken = token;
                result.Status = ResultStatus.Success;
                result.ResultMessage = "Token Received Successfully";
                return Ok(result);
            }
            else
            {
                result.Status = ResultStatus.Failure;
                result.ErrorMessage = "Failure. Null or Empty token string";
                return BadRequest(result);
            }
        }

        /******************************************************************
         *                                                                *
         *              Endpoints that call data functions                *
         *                                                                *
         ******************************************************************/
        
        [HttpPost]
        [Route("Android/BulkMessages")]
        public ActionResult<RequestResult> BulkMessages([FromServices] ITokens tokens, [FromServices] IConfiguration config, [FromBody] MessageList messageList)
        {
            var result = new RequestResult();
            
            const string firebaseFunc = "bulkMessages";
            var client = new RestClient(config["FirebaseLink"]);
            var clientToken = tokens.ClientToken;
            var req = new RestRequest(firebaseFunc, Method.POST);
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", clientToken},
                {"Messages", JsonConvert.SerializeObject(messageList)}
            };
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);

            var response = client.Execute(req);

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                result.ResultMessage = "Successfully sent Bulk Messages to firebase";
                result.Status = ResultStatus.Success;
                return Ok(result);
            }
            else
            {
                result.ErrorMessage = response.ErrorMessage;
                result.Status = ResultStatus.Failure;
                return BadRequest(result);
            } 
        }

        /******************************************************************
         *                                                                *
         *          Endpoints that call notification functions            *
         *                                                                *
         ******************************************************************/
        
        // Probably change the type of the status
        [HttpGet]
        [Route("Android/SendNewSMSMessageResult")]
        public ActionResult<RequestResult> SendNewSMSMessageResult([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, string status)
        {
            var result = new RequestResult();

            const string firebaseFuncName = "askForBulkMessages";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.GET);

            var response = client.Execute(req);

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                result.ResultMessage = "Successfully sent SMS Message result to firebase";
                result.Status = ResultStatus.Success;
                return Ok(result);
            }
            else
            {
                result.ErrorMessage = response.ErrorMessage;
                result.Status = ResultStatus.Failure;
                return BadRequest(result);
            }
        }
    }
}
