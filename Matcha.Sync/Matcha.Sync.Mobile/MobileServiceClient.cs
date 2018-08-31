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
            DataStore.Instance.Init(_webApiUrl.Replace(":", "").Replace("/", "").Replace(".", ""));
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

            public IList<T> ToList()
            {
                return DataStore.Instance.Get<IList<T>>(typeof(T).Name) ?? new List<T>();
            }

            public IList<T> ToList(string queryId)
            {
                var resulList = ToList();
                return resulList == null ? new List<T>() : resulList.Where(e=> e.QueryId == queryId).ToList();
            }

            public void InsertOrUpdate(T data)
            {
                var existingList = ToList();
                if (existingList.Any(e => e.LocalId == data.LocalId))
                {
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
                Update(data);
            }

            public IMobileServiceTableQuery<T> CreateQuery() => new MobileServiceTableQuery<T>();

            public async Task PullAsync(string queryId, string paramQuery = "")
            {
                await InvokePullData(queryId, paramQuery);
                RegisterQueryInfo(new PullQueryInfo(queryId, paramQuery));
            }

            public async Task PullAsync(string queryId, IMobileServiceTableQuery<T> paramQuery)
            {
                await PullAsync(queryId, paramQuery.Query);
            }

            public async Task PullAsync()
            {
                var existingList = DataStore.Instance.Get<IList<PullQueryInfo>>(nameof(PullQueryInfo)) ?? new List<PullQueryInfo>();

                if (!existingList.Any())
                {
                    await InvokePullData(typeof(T).Name, string.Empty);
                    RegisterQueryInfo(new PullQueryInfo(typeof(T).Name, string.Empty));
                }
                else
                {
                    foreach (var pullQueryInfo in existingList)
                    {
                        await InvokePullData(pullQueryInfo.QueryId, pullQueryInfo.ParamQuery);
                    }
                }
            }

            public async Task PushAsync()
            {
                var existingList = ToList();
                var getAllNotSync = existingList.Where(e => !e.IsSynced);

                if(getAllNotSync.Any())
                    await PostWebDataAsync<string>(getAllNotSync, GetControllerNameFromType(typeof(T).Name));
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


            private void Update(T data)
            {
                var existingList = ToList();
                var firstData = existingList.FirstOrDefault(e => e.LocalId == data.LocalId);
                if (firstData == null) return;

                var indexOfData = existingList.IndexOf(firstData);
                existingList.RemoveAt(indexOfData);

                data.IsSynced = false; //LET THE SYSTEM KNOW THIS IS FOR SYNC
                data.LastUpdated = DateTime.Now;

                if (existingList.Count <= indexOfData)
                    existingList.Add(data);
                else
                    existingList.Insert(indexOfData, data);

                DataStore.Instance.Add(typeof(T).Name, existingList, TimeSpan.FromDays(30));
            }

            private async Task InvokePullData(string queryId, string paramQuery)
            {
                var existingList = ToList().Where(e=> e.QueryId != queryId).ToList();
                var oDataResult = await ExecuteQuery(paramQuery);
                var listResult = oDataResult.DataList;

                listResult.ForEach(e =>
                {
                    e.IsSynced = true;
                    e.QueryId = queryId;
                });

                existingList.AddRange(listResult);

                DataStore.Instance.Add(typeof(T).Name, existingList, TimeSpan.FromDays(30));
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
        }

        private class MobileServiceTableQuery<T> : IMobileServiceTableQuery<T>
        {
            private readonly StringBuilder _whereQuery = new StringBuilder();
            private string _orderQuery = string.Empty;
            private string _skipQuery = string.Empty;
            private string _takeQuery = string.Empty;

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
