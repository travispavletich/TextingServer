using System;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebServer.Controllers
{
    public class AndroidController : Controller
    {
        // GET
        [HttpGet]
        [Route("Android/index")]
        public ActionResult<string> Index()
        {
            Console.WriteLine("Success!");
            return Ok("Hello World");
        }

        [HttpGet]
        [Route("Android/token")]
        public ActionResult<string> Token(string token)
        {
            Console.WriteLine(token);
            return Ok("Token Received");
        }
    }
}
    
