using System.Collections.Generic;

namespace Matcha.Sync.Mobile
{
    public class PullQueryInfo
    {
        public PullQueryInfo(string queryId, string paramQuery, long recordCount, IList<long> idList)
        {
            QueryId = queryId;
            ParamQuery = paramQuery;
            RecordCount = recordCount;
            IdList = idList;
        }

        public string QueryId { get;  }
        public string ParamQuery { get;}
        public long RecordCount { get; }
        public IList<long> IdList { get; }
    }
}
