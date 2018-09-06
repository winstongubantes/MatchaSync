using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SampleApi.Models
{
    public class WebApiResult<T>
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public IList<T> DataList { get; set; }
    }
}
