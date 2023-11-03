using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Blog.WebApi.Models.Entities
{
    [Table("multimedia")]
    public class Multimedia
    {

        [PrimaryKey("id", false)]
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [Column("key")]
        [JsonProperty(PropertyName = "key")]
        public Guid Key { get; set; }

        [Column("created_at")]
        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("post_id")]
        [JsonProperty(PropertyName = "post_id")]
        public int PostId { get; set; }

        [Column("bucket_url")]
        [JsonProperty(PropertyName = "bucket_url")]
        public string BucketUrl { get; set; }

        [Column("file_name")]
        [JsonProperty(PropertyName = "file_name")]
        public string FileName { get; set; }

        [Column("file_extension")]
        [JsonProperty(PropertyName = "file_extension")]
        public string FileExtension { get; set; }

        [Column("file_size_kb")]
        [JsonProperty(PropertyName = "file_size_kb")]
        public double FileSizeKb { get; set; }
    }
}