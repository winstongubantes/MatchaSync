using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Matcha.Sync.Mobile
{
    public class ODataResult<T>
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("@odata.count")]
        public long OdataCount { get; set; }

        [JsonProperty("value")]
        public IList<T> DataList { get; set; }
    }
}
