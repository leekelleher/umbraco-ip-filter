using Newtonsoft.Json;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Umbraco.IpFilter.Models
{
    [PrimaryKey("Id")]
    public abstract class Entity
    {
        [JsonProperty("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }
    }
}