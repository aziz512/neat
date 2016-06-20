using Newtonsoft.Json;
namespace News.Models
{
    public class Setting
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Key")]
        public string Key { get; set; }
        [JsonProperty("Value")]
        public string Value { get; set; }
        public string Type { get; set; }
    }
}