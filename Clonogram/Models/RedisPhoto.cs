using System;
using Newtonsoft.Json;

namespace Clonogram.Models
{
    public class RedisPhoto
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }
}
