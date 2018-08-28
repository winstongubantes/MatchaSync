namespace Matcha.Sync.Mobile
{
    public class PullQueryInfo
    {
        public PullQueryInfo(string queryId, string paramQuery)
        {
            QueryId = queryId;
            ParamQuery = paramQuery;
        }

        public string QueryId { get;  }
        public string ParamQuery { get;}
    }
}
