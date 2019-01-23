using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RfpProxyLib.Messages
{
    public enum SubscriptionType
    {
        /// <summary>
        /// I will take care and answer with the updated message (may be empty)
        /// </summary>
        Handle,
        /// <summary>
        /// don't wait for an answer
        /// </summary>
        Listen,
        /// <summary>
        /// client is finished with subscribe
        /// </summary>
        End
    }

    public class SubscriptionFilter
    {
        /// <summary>
        /// HEX encoded binary
        /// </summary>
        [JsonProperty("filter")]
        public string Filter { get; set; }

        /// <summary>
        /// HEX encoded mask, how to match the <see cref="Filter"/>
        /// </summary>
        [JsonProperty("mask")]
        public string Mask { get; set; }
    }

    public class Subscribe
    {
        /// <summary>
        /// logging or editing
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SubscriptionType Type { get; set; }

        /// <summary>
        /// lower is better
        /// </summary>
        [JsonProperty("prio")]
        public byte Priority { get; set; }

        /// <summary>
        /// MAC Address Filter for RFP
        /// </summary>
        [JsonProperty("rfp")]
        public SubscriptionFilter Rfp { get; set; }

        /// <summary>
        /// Message Filter for the whole message
        /// </summary>
        [JsonProperty("filter")]
        public SubscriptionFilter Message { get; set; }
    }
}