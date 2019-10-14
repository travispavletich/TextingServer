using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebServer.Models
{
    /// <summary>
    /// Interface for Tokens object used for dependency injection
    /// </summary>
    public interface ITokens
    {
        string AndroidToken { get; set; }
        string ClientToken { get; set; }
    }
}