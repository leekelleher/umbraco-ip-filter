using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Umbraco.IpFilter.Models
{
    [TableName("ourIpFilterEntry")]
    public class IpFilterEntry : Entity
    {
        [JsonProperty("nodeId")]
        public int NodeId { get; set; }

        [JsonProperty("whitelist")]
        [Column("Whitelist")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string __RawWhitelist { get; set; }

        [JsonIgnore]
        [Ignore]
        public IEnumerable<string> Whitelist
        {
            get
            {
                return __RawWhitelist.Split(new[] { "\n", "\r", ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !x.IsNullOrWhiteSpace());
            }
            set { __RawWhitelist = string.Join("\n", value.Select(x => x.Trim()).Where(x => !x.IsNullOrWhiteSpace())); }
        }
            
        [JsonProperty("blacklist")]
        [Column("Blacklist")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string __RawBlacklist { get; set; }

        [JsonIgnore]
        [Ignore]
        public IEnumerable<string> Blacklist
        {
            get
            {
                return __RawBlacklist.Split(new[] { "\n", "\r", ",", ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !x.IsNullOrWhiteSpace());
            }
            set { __RawBlacklist = string.Join("\n", value.Select(x => x.Trim()).Where(x => !x.IsNullOrWhiteSpace())); }
        }

        [JsonProperty("errorPageNodeId")]
        public int ErrorPageNodeId { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
