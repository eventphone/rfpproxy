using System;
using Newtonsoft.Json;

namespace RfpProxyLib
{
    public class Hello
    {
        public Hello(){}

        public Hello(string message)
        {
            Message = message;
        }

        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
