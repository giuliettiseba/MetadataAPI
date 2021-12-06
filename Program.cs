using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace MetadataAPI
{
    internal class Program
    {
        static void Main(string[] args)
        {

            VideoOS.Platform.SDK.Environment.Initialize();              // Initialize the standalone Environment
            VideoOS.Platform.SDK.Media.Environment.Initialize();        // Initialize the Media
            VideoOS.Platform.SDK.Export.Environment.Initialize();       // Initialize the Export

            
            ConnectionManager.ConnectManagementServer();                // Connect to management server. TODO: CREATE A TASK TO RENEW TOKEN 


            var config = new HttpSelfHostConfiguration("http://localhost:8080");        // API STUFF
            config.MessageHandlers.Add(new CustomHeaderHandler());
            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional });

            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }

        }

        public class CustomHeaderHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return base.SendAsync(request, cancellationToken)
                    .ContinueWith((task) =>
                    {
                        HttpResponseMessage response = task.Result;
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        return response;
                    });
            }
        }
    }
}
