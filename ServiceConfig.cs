using MTUBankBase.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTUTxService
{
    public class ServiceConfig : ConfigBase<ServiceConfig>
    {
        public string Hostname { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8092;
        public string CoreURL { get; set; } = "http://127.0.0.1:8090";
        public string BindToken { get; set; }
        public string BaseURL { get; set; } = $"http://127.0.0.1:8092";

        public string dbHost { get; set; } = "nk.ax";
        public int dbPort { get; set; } = 3306;
        public string dbUser { get; set; } = "mtubank";
        public string dbPass { get; set; } = "_sSlf0kc84_kCX_s";
        public string dbName { get; set; } = "mtubank";

        [JsonIgnore]
        public string ConnectionString
        {
            get { return $"Server={dbHost};Database={dbName};User={dbUser};Password={dbPass};"; }
        }
    }
}
