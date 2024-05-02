using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using MTUBankBase.Helpers;
using MTUBankBase.ServiceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTUTxService.ServiceUtils
{
    public class ServiceStub : WebApiController
    {
        public string Name { get; } = "MTUBank Transaction Service";
        public string Description { get; } = "This service concludes financial operations between users, such as: opening and closing accounts, transactions between accounts, etc.";
        public string BaseUrl { get; } = WebControllerMethods.BindString(Program.serviceConfig.Hostname, Program.serviceConfig.Port);
        public ServiceType ServiceType { get; } = ServiceType.Transaction;

        [Route(HttpVerbs.Get, "/getStatus")]
        public async Task<string> GetStatus() => "OK";

        [Route(HttpVerbs.Get, "/getServiceInfo")]
        public async Task<Service> GetServiceInfo() => new Service(ServiceInitializer.LocalService);

        [Route(HttpVerbs.Get, "/disconnectService")]
        public async Task<string> DisconnectService() => "OK";
    }
}
