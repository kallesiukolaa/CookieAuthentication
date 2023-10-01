using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet_graph.Models;

namespace WebApp_OpenIDConnect_DotNet_graph.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly GraphServiceClient _graphServiceClient;

        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        private readonly string[] _graphScopes;
        private List<KeyPairModel>_keys = new List<KeyPairModel>() 
            {
                new KeyPairModel()
                {
                    KeyID = 0,
                    Created = DateTime.Parse("5/1/2008 8:30:52 AM"), 
                    Expires = DateTime.Parse("5/1/2024 8:30:52 AM"), 
                    KeyName = "MyKey", 
                    KeyDescription = "This is my key for testing"
                },
                new KeyPairModel()
                {
                    KeyID = 1,
                    Created = DateTime.Parse("5/1/2008 8:30:52 AM"), 
                    Expires = DateTime.Parse("5/1/2024 8:30:52 AM"), 
                    KeyName = "MyKey1", 
                    KeyDescription = "This is my second key for testing"
                }
            };

        public HomeController(ILogger<HomeController> logger,
                            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;

            // Capture the Scopes for Graph that were used in the original request for an Access token (AT) for MS Graph as
            // they'd be needed again when requesting a fresh AT for Graph during claims challenge processing
            _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

            
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            User currentUser = null;

            try
            {
                currentUser = await _graphServiceClient.Me.GetAsync();
            }
            // Catch CAE exception from Graph SDK
            catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                try
                {
                    Console.WriteLine($"{svcex}");
                    string claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);
                    _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
                    return new EmptyResult();
                }
                catch (Exception ex2)
                {
                    _consentHandler.HandleException(ex2);
                }
            }

            try
            {
                // Get user photo
                using (var photoStream = await _graphServiceClient.Me.Photo.Content.GetAsync())
                {
                    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                    ViewData["Photo"] = Convert.ToBase64String(photoByte);
                }
            }
            catch (Exception pex)
            {
                Console.WriteLine($"{pex.Message}");
                ViewData["Photo"] = null;
            }

            ViewData["Me"] = currentUser;
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View(_keys);
        }

        [AllowAnonymous]
        public IActionResult GenerateNewKey(KeyPairModel key)
        {
            var a = _keys.RemoveAll(x => x.KeyID == key.KeyID);
            return RedirectToAction("Privacy");
        }

        private string GeneratePrivateKey()
        {
            string strCmdText;
            strCmdText= "dir";
            var process = System.Diagnostics.Process.Start("CMD.exe",strCmdText);
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            
            return output;
        }

        [AllowAnonymous]
        [Route("DownloadFile")]
        public IActionResult DownloadPrivateKey()
        {
            return DownloadFile(GeneratePrivateKey(), "rsa_key.p8");

        }

        public IActionResult DownloadFile(string fileContent, string filename)
        {
            return File(Encoding.Unicode.GetBytes(fileContent), "application/octet-stream", filename);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string GetSecretFromKeyVault()
        {
            string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential());

            Response<KeyVaultSecret> secret = client.GetSecretAsync("Graph-App-Secret").Result;

            return secret.Value.Value;
        }
    }
}