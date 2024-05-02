using EmbedIO;
using EmbedIO.WebApi;
using MTUBankBase.Helpers;
using MTUModelContainer.ServiceManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTUTxService.ServiceUtils
{
    public class ServiceInitializer
    {
        public static ServiceStub LocalService;

        public static void InitService()
        {
            // init the local service
            LocalService = new ServiceStub();

            // spin up the webserver
            var server = new WebServer(o => o
                   .WithUrlPrefix(LocalService.BaseUrl)
                   .WithMode(HttpListenerMode.EmbedIO), Program.jwtService);

            // add the ServiceStub inherited class as the handler for API requests
            server = server.WithWebApi("/", WebControllerMethods.AsJSON, m =>
            {
                m.WithController<Controller>();
            });

            server.Start();
        }

        public static async Task BindServiceAsync()
        {
            // request service connection
            using (var http = new HttpClient())
            {
                // create request object
                var request = new RegisterRequest()
                {
                    BaseUrl = LocalService.BaseUrl,
                    Name = LocalService.Name,
                    PairToken = Program.serviceConfig.BindToken
                };

                // send to core
                var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var result = await http.PostAsync($"{Program.serviceConfig.CoreURL}/registerService", jsonContent);

                if (!result.IsSuccessStatusCode) throw new HttpException(result.StatusCode);
                return;
            }
        }
    }
}
