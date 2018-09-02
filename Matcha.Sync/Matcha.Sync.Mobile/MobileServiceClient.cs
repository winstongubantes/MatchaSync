using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Matcha.Sync.Model;
using Newtonsoft.Json;

namespace Matcha.Sync.Mobile
{
    public interface IMobileServiceClient
    {
        IMobileServiceCrudTable<T> GetSyncTable<T>() where T : ISynchronizable;
        void DefineSyncTable<T>() where T : ISynchronizable;
        Task SyncAllData();
    }

    public sealed class MobileServiceClient : IMobileServiceClient
    {
        private readonly Dictionary<string, IMobileServiceSyncTable> _dictionarySync;
        private string _webApiUrl;

        static MobileServiceClient()
        {
        }

        private MobileServiceClient()
        {
            _dictionarySync = new Dictionary<string, IMobileServiceSyncTable>();
        }

        public static MobileServiceClient Instance { get; } = new MobileServiceClient();

        public IMobileServiceCrudTable<T> GetSyncTable<T>() where T : ISynchronizable
        {
            CheckIfInitialized();

            if (_dictionarySync.ContainsKey(typeof(T).Name)) return _dictionarySync[typeof(T).Name] as MobileServiceSyncTable<T>;

            var mst = new MobileServiceSyncTable<T>(_webApiUrl);
            _dictionarySync.Add(typeof(T).Name, mst);

            return mst;
        }

        public void DefineSyncTable<T>() where T : ISynchronizable
        {
            if (_dictionarySync.ContainsKey(typeof(T).Name)) return;

            var mst = new MobileServiceSyncTable<T>(_webApiUrl);
            _dictionarySync.Add(typeof(T).Name, mst);
        }

        public async Task SyncAllData()
        {
            await PushAsync();
            await PullAsync();
        }

        public IMobileServiceClient Init(string webApiUrl)
        {
            _webApiUrl = webApiUrl;

            DataStore.Instance.Init(_webApiUrl
                .Replace(":", "")
                .Replace("/", "")
                .Replace(".", ""));

            return this;
        }

        private async Task PullAsync()
        {
            foreach (var syncTable in _dictionarySync.Values) await syncTable.PullAsync();
        }

        private async Task PushAsync()
        {
            foreach (var syncTable in _dictionarySync.Values) await syncTable.PushAsync();
        }

        private void CheckIfInitialized()
        {
            if (string.IsNullOrWhiteSpace(_webApiUrl)) throw new ArgumentException("Not Initialized!");
        }

        private class MobileServiceSyncTable<T> : IMobileServiceCrudTable<T> where T : ISynchronizable
        {
            private readonly JsonSerializer _serializer = new JsonSerializer();
            private readonly string _webApiUrl;

            public MobileServiceSyncTable(string webApiUrl)
            {
                _webApiUrl = webApiUrl;
            }

            #region Public Methods
            public IList<T> ToList()
            {
                return DataStore.Instance.Get<IList<T>>(typeof(T).Name) ?? new List<T>();
            }

            public IList<T> ToList(string queryId)
            {
                var resulList = ToList();
                var queryInfo = GetQueryInfo(queryId);
                return resulList == null || queryInfo == null ? 
                    new List<T>() : 
                    resulList.Where(e => queryInfo.IdList.Any(x=> x == e.Id))
                        .ToList();
            }

            public void InsertOrUpdate(T data)
            {
                var existingList = ToList();
                var existingData = existingList.FirstOrDefault(e => e.LocalId == data.LocalId);

                data.IsSynced = false; //new data must be flagged as NOT synced

                if (existingData != null)
                {
                    //Since this is just a local data no need to check if it is "IsSynced"
                    Update(data);
                    return;
                }

                if (string.IsNullOrWhiteSpace(data.LocalId)) data.LocalId = Guid.NewGuid().ToString();

                existingList.Add(data);
                DataStore.Instance.Add(typeof(T).Name, existingList, TimeSpan.FromDays(30));
            }

            public void Delete(T data)
            {
                data.IsDeleted = false;
                data.IsSynced = false;
                Update(data);
            }

            public IMobileServiceTableQuery<T> CreateQuery() => new MobileServiceTableQuery<T>();

            public async Task PullAsync(string queryId, string paramQuery = "")
            {
                var oDataResult = await InvokePullData(paramQuery);
                var idList = oDataResult.DataList.Select(e => e.Id).ToList();
                RegisterQueryInfo(new PullQueryInfo(queryId, paramQuery, oDataResult.OdataCount, idList));
            }

            public async Task PullAsync(string queryId, IMobileServiceTableQuery<T> paramQuery)
            {
                await PullAsync(queryId, paramQuery.Query);
            }

            public async Task PullAsync()
            {
                var existingList = DataStore.Instance.Get<IList<PullQueryInfo>>(nameof(PullQueryInfo)) ?? new List<PullQueryInfo>();

                foreach (var pullQueryInfo in existingList)
                {
                    await PullAsync(pullQueryInfo.QueryId, pullQueryInfo.ParamQuery);
                }
            }

            public async Task PushAsync()
            {
                var existingList = ToList();
                var getAllNotSync = existingList.Where(e => !e.IsSynced);

                if (getAllNotSync.Any())
                    await PostWebDataAsync<object>(getAllNotSync, GetControllerNameFromType(typeof(T).Name));
            }

            public async Task<ODataResult<T>> ExecuteQuery(string paramQuery)
            {
                var url = $"{_webApiUrl}/{GetControllerNameFromType(typeof(T).Name)}/{paramQuery}";
                var oDataResult = await GetWebDataAsync(url);
                return oDataResult;
            }

            public async Task<ODataResult<T>> ExecuteQuery(IMobileServiceTableQuery<T> paramQuery)
            {
                var url = $"{_webApiUrl}/{GetControllerNameFromType(typeof(T).Name)}/{paramQuery.Query}";
                var oDataResult = await GetWebDataAsync(url);
                return oDataResult;
            }

            public async Task<TF> PostWebDataAsync<TF>(object obj, string methodName)
            {
                TF result;

                using (var client = GetHttpClient())
                {
                    var requestObject = JsonConvert.SerializeObject(obj);
                    var dataContent = new StringContent(requestObject, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{_webApiUrl}/{methodName}",
                        dataContent, CancellationToken.None);

                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        result = _serializer.Deserialize<TF>(json);
                    }
                }

                return result;
            }

            public long RecordCount(string queryId)
            {
                var queryInfo = GetQueryInfo(queryId);
                return queryInfo?.RecordCount ?? 0;
            }

            #endregion

            #region Private Methods
            private void Update(T data)
            {
                var existingList = ToList();
                var firstData = existingList.FirstOrDefault(e => e.LocalId == data.LocalId);
                if (firstData == null) return;

                UpdateDataFromExistingList(data, firstData, existingList);

                DataStore.Instance.Add(typeof(T).Name, existingList, TimeSpan.FromDays(30));
            }

            private static void UpdateDataFromExistingList(T data, T firstData, IList<T> existingList)
            {
                var indexOfData = RemoveByIndex(firstData, existingList);
                data.LastUpdated = DateTime.Now;
                AddByIndex(data, existingList, indexOfData);
            }

            private static int RemoveByIndex(T firstData, IList<T> existingList)
            {
                var indexOfData = existingList.IndexOf(firstData);
                existingList.RemoveAt(indexOfData);
                return indexOfData;
            }

            private static void AddByIndex(T data, IList<T> existingList, int indexOfData)
            {
                if (existingList.Count <= indexOfData)
                    existingList.Add(data);
                else
                    existingList.Insert(indexOfData, data);
            }

            private async Task<ODataResult<T>> InvokePullData(string paramQuery)
            {
                var existingList = ToList();
                var oDataResult = await ExecuteQuery(paramQuery);
                var listResult = oDataResult.DataList;

                foreach (var resultVal in listResult)
                {
                    AggregateListByCondition(resultVal, existingList);
                }

                DataStore.Instance.Add(typeof(T).Name, existingList, TimeSpan.FromDays(30));
                return oDataResult;
            }

            private static void AggregateListByCondition(T resultVal, IList<T> existingList)
            {
                var existingData = existingList.FirstOrDefault(e => e.LocalId == resultVal.LocalId);

                resultVal.IsSynced = true; //new data must be flagged as NOT synced

                //Do Not UPDATE data that has changes(IsSynced == false)
                if (existingData == null)
                    existingList.Add(resultVal);
                else if(existingData.IsSynced)
                    UpdateDataFromExistingList(resultVal, existingData, existingList);
            }

            private void RegisterQueryInfo(PullQueryInfo queryInfo)
            {
                var existingList = DataStore.Instance.Get<IList<PullQueryInfo>>(nameof(PullQueryInfo)) ?? new List<PullQueryInfo>();
                var firstData = existingList.FirstOrDefault(e => e.QueryId == queryInfo.QueryId);

                if (firstData != null)
                {
                    var indexOfData = existingList.IndexOf(firstData);
                    existingList.RemoveAt(indexOfData);

                    if (existingList.Count <= indexOfData)
                        existingList.Add(queryInfo);
                    else
                        existingList.Insert(indexOfData, queryInfo);
                }
                else
                {
                    existingList.Add(queryInfo);
                }

                DataStore.Instance.Add(nameof(PullQueryInfo), existingList, TimeSpan.FromDays(30));
            }

            private IList<PullQueryInfo> GetQueryInfoList()
            {
                var existingList = DataStore.Instance.Get<IList<PullQueryInfo>>(nameof(PullQueryInfo)) ?? 
                                   new List<PullQueryInfo>();
                return existingList;
            }

            private PullQueryInfo GetQueryInfo(string queryId)
            {
                var existingList = GetQueryInfoList();
                var firstData = existingList.FirstOrDefault(e => e.QueryId == queryId);
                return firstData;
            }

            private string GetControllerNameFromType(string typeName)
            {
                if (typeName.Contains("y"))
                {
                    return $"{typeName.TrimEnd('y')}ies";
                }

                return $"{typeName}s";
            }

            private async Task<ODataResult<T>> GetWebDataAsync(string url)
            {
                ODataResult<T> result;

                using (var client = GetHttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        result = _serializer.Deserialize<ODataResult<T>>(json);
                    }
                }

                return result;
            }

            private HttpClient GetHttpClient()
            {
                var httpClient = new HttpClient();
                return httpClient;
            } 
            #endregion
        }

        private class MobileServiceTableQuery<T> : IMobileServiceTableQuery<T>
        {
            private readonly StringBuilder _whereQuery = new StringBuilder();
            private string _orderQuery = string.Empty;
            private string _skipQuery = string.Empty;
            private string _takeQuery = string.Empty;

            #region Public Methods
            public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
            {
                var oData = WhereBuilder.ToOdata(predicate);
                _whereQuery.Append($"{WhereAndPreFix}{oData}");

                return this;
            }

            public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> predicate)
            {
                if (predicate.Body is MemberExpression exp)
                {
                    _orderQuery = $"{ParamPreFix}$orderby={exp.Member.Name}";
                }
                else
                    throw new ArgumentException("Invalid query");

                return this;
            }

            public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> predicate)
            {
                if (predicate.Body is MemberExpression exp)
                {
                    _orderQuery = $"{ParamPreFix}$orderby={exp.Member.Name} desc";
                }
                else
                    throw new ArgumentException("Invalid query");

                return this;
            }

            public IMobileServiceTableQuery<T> Skip(int count)
            {
                _skipQuery = $"{ParamPreFix}$skip={count}";
                return this;
            }

            public IMobileServiceTableQuery<T> Take(int count)
            {
                _takeQuery = $"{ParamPreFix}$top={count}";
                return this;
            } 
            #endregion

            public string Query => $"{QueryRaw}{CountPostFix}";

            private string ParamPreFix => string.IsNullOrWhiteSpace(QueryRaw) ? "?" : "&";

            private string WhereAndPreFix => string.IsNullOrWhiteSpace(_whereQuery.ToString()) ? $"{ParamPreFix}$filter=" : " and ";

            private string QueryRaw => $"{_skipQuery}{_takeQuery}{_orderQuery}{_whereQuery}";

            private string CountPostFix => string.IsNullOrWhiteSpace(QueryRaw) ? "?$count=true" : "&$count=true";
        }
    }

    public interface IMobileServiceCrudTable<T> : IMobileServiceSyncTable
    {
        IList<T> ToList();
        IList<T> ToList(string queryId);
        void InsertOrUpdate(T data);
        void Delete(T data);
        Task PullAsync(string queryId, IMobileServiceTableQuery<T> paramQuery);
        Task PullAsync(string queryId, string paramQuery);
        IMobileServiceTableQuery<T> CreateQuery();
        Task<ODataResult<T>> ExecuteQuery(string paramQuery);
        Task<ODataResult<T>> ExecuteQuery(IMobileServiceTableQuery<T> paramQuery);
        Task<TF> PostWebDataAsync<TF>(object obj, string methodName);
        long RecordCount(string queryId);
    }

    public interface IMobileServiceSyncTable
    {
        Task PullAsync();
        Task PushAsync();
    }

    public interface IMobileServiceTableQuery<T>
    { 
        IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> predicate);
        IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> predicate);
        IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate);
        IMobileServiceTableQuery<T> Skip(int count);
        IMobileServiceTableQuery<T> Take(int count);
        string Query { get; }
    }
}
