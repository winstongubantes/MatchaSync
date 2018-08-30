using System;
using System.Collections.Generic;
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

            public Task PullAsync(string queryId, IMobileServiceTableQuery<T> paramQuery)
            {
                var rseult = paramQuery.Query;
                var t = rseult;

                return Task.FromResult(0);
                //await InvokePullData(queryId, paramQuery.ToString());
                //RegisterQueryInfo(new PullQueryInfo(queryId, paramQuery.ToString()));
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
                    await PostWebDataAsync(getAllNotSync, GetControllerNameFromType(typeof(T).Name));
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
                var url = $"{_webApiUrl}/{GetControllerNameFromType(typeof(T).Name)}/{paramQuery}";
                var listResult = await GetWebDataAsync(url);

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

            private async Task<string> PostWebDataAsync(object obj, string methodName)
            {
                string result;

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
                        result = _serializer.Deserialize<string>(json);
                    }
                }

                return result;
            }

            public async Task<IList<T>> GetWebDataAsync(string url)
            {
                IList<T> result;

                using (var client = GetHttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    using (var json = new JsonTextReader(reader))
                    {
                        result = _serializer.Deserialize<IList<T>>(json);
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
            private StringBuilder _sb = new StringBuilder();

            public IMobileServiceTableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> predicate)
            {
                string expBody = ((LambdaExpression)predicate).Body.ToString();

                var paramName = predicate.Parameters[0].Name;
                var paramTypeName = predicate.Parameters[0].Type.Name;

                return this;
            }

            public IMobileServiceTableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> predicate)
            {
                string expBody = ((LambdaExpression)predicate).Body.ToString();

                var paramName = predicate.Parameters[0].Name;
                var paramTypeName = predicate.Parameters[0].Type.Name;

                return this;
            }

            public IMobileServiceTableQuery<T> Where(Expression<Func<T, bool>> predicate)
            {
                string expBody = ((LambdaExpression)predicate).Body.ToString();

                var paramName = predicate.Parameters[0].Name;
                var paramTypeName = predicate.Parameters[0].Type.Name;

                CheckQueryAppend();
                _sb.Append(expBody);

                return this;
            }

            public IMobileServiceTableQuery<T> Skip(int count)
            {
                CheckQueryAppend();
                _sb.Append($"$skip={count}");
                return this;
            }

            public IMobileServiceTableQuery<T> Take(int count)
            {
                CheckQueryAppend();
                _sb.Append($"$top={count}");
                return this;
            }

            public string Query => _sb.ToString();

            private void CheckQueryAppend()
            {
                _sb.Append(!string.IsNullOrWhiteSpace(Query) ? "?" : "&");
            }
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
