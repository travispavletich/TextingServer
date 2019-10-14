using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Cors;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebServer.Models;
using WebServer.Models.AndroidModels;

namespace WebServer.Controllers
{
    public class AndroidController : Controller
    {
        /// <summary>
        /// Gets the Token for the Android app 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <returns></returns>
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
       
        
        /// <summary>
        /// Endpoint for Android to hit to send ConversationList information
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="data"></param>
        /// <param name="conversations"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/ConversationList")]
        public ActionResult<RequestResult> ConversationList([FromServices] ITokens tokens, [FromServices] IConfiguration config, 
            [FromServices] MessageData data, [FromBody] ConversationList conversations)
        {
            var result = new RequestResult();
            
            // Update state 
            data.Conversations = conversations.Conversations;
            
            // Notify the client that the data has been updated and is ready to retrieve
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.ClientToken}
            };
        
            var response = Utilities.FirebaseUtilities.Notify(config, tokens.ClientToken, "ConversationList", dict);

            if (response.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// Endpoint for the android app to hit to send a list of messages to the server
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="data"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/MessageList")]
        public ActionResult<RequestResult> MessageList([FromServices] ITokens tokens, [FromServices] IConfiguration config, 
               [FromServices] MessageData data, [FromBody] MessageListRequest messages)
        {
            var result = new RequestResult();
                        
            // Update state 
            data.ConversationToMessages[messages.ConversationID] = messages.Messages;
            
            // Notify the client that the data has been updated and is ready to retrieve
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.ClientToken},
                {"ConversationID", messages.ConversationID.ToString()}
            };
        
            var response = Utilities.FirebaseUtilities.Notify(config, tokens.ClientToken, "MessageList", dict);

            if (response.StatusCode == HttpStatusCode.OK)
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
        
        /// <summary>
        /// Endpoint for the Android app to hit when it has sent a message (which the client asked it to send)
        /// And wants to deliver a status on that message back to the client
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="messageStatus"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/SentMessageStatus")]
        public ActionResult<RequestResult> SentMessageStatus([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, MessageStatus messageStatus)
        {
            var result = new RequestResult();

            const string firebaseFuncName = "SentMessageStatus";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);

            var dict = new Dictionary<string, object>
            {
                {"Token", androidToken},
                {"MessageID", messageStatus.MessageID},
                {"MessageStatus", messageStatus.Status}
            };
            
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            var response = client.Execute(req);

            if (response.StatusCode == HttpStatusCode.OK)
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
