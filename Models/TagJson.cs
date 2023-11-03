using Newtonsoft.Json;

namespace Blog.WebApi.Models;
public class TagJson
{
    [JsonProperty(PropertyName = "tag_key")]
    public Guid Key { get; set; }
    [JsonProperty(PropertyName = "tag_name")]
    public string Name { get; set; }
}
