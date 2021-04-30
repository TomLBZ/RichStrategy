using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichStrategy
{
    public class MyConfiguration
    {
        public string HostUsed { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public bool UseTestNet { get; set; }
        public MyConfiguration(string basePath, string apiV4Key, string apiV4Secret, bool useTestNet)
        {
            HostUsed = basePath;
            ApiKey = apiV4Key;
            ApiSecret = apiV4Secret;
            UseTestNet = useTestNet;
        }
        public MyConfiguration()
        {
            HostUsed = ApiKey = ApiSecret = default;
            UseTestNet = false;
        }
    }
}
