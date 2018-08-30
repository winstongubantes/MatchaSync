using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SQLite;

namespace Matcha.Sync.Mobile
{
    public interface IDataStore
    {
        void Add(string key, string data, TimeSpan expireIn);
        void Add<T>(string key, T data, TimeSpan expireIn);
        void Empty(params string[] key);
        void EmptyAll();
        void EmptyExpired();
        bool Exists(string key);
        string Get(string key);
        T Get<T>(string key);
        bool IsExpired(string key);
        DateTime? GetExpiration(string key);
        DateTime? GetLastUpdate(string key);
        IDataStore Init(string appId);
    }

    public class DataStore : IDataStore
    {
        private  SQLiteConnection _db;
        readonly object _dblock = new object();
        private JsonSerializerSettings _jsonSettings;

        static DataStore()
        {
        }

        
        private DataStore()
        { 

        }

        public static IDataStore Instance { get; } = new DataStore();


        public IDataStore Init(string appId)
        {
            var directory = Path.Combine(Utils.GetBasePath(appId), "DataStore");
            var path = Path.Combine(directory, "DataStore.db");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _db = new SQLiteConnection(path);
            _db.CreateTable<SchemaContent>();

            _jsonSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            };

            return this;
        }

        public void Add(string key, string data, TimeSpan expireIn)
        {
            if (data == null)
                return;

            var ent = new SchemaContent
            {
                Id = key,
                ExpirationDate = Utils.GetExpiration(expireIn),
                LastUpdate = DateTime.Now,
                Contents = data
            };
            lock (_dblock)
            {
                _db.InsertOrReplace(ent);
            }
        }

        public void Add<T>(string key, T data, TimeSpan expireIn)
        {
            var dataJson = JsonConvert.SerializeObject(data, _jsonSettings);
            Add(key, dataJson, expireIn);
        }

        public void Empty(params string[] key)
        {
            lock (_dblock)
            {
                _db.RunInTransaction(() =>
                {
                    foreach (var k in key)
                        _db.Delete<SchemaContent>(primaryKey: k);
                });
            }
        }

        public void EmptyAll()
        {
            lock (_dblock)
            {
                _db.DeleteAll<SchemaContent>();
            }
        }

        public void EmptyExpired()
        {
            lock (_dblock)
            {
                var entries = _db.Query<SchemaContent>($"SELECT * FROM Banana WHERE ExpirationDate < ?", DateTime.UtcNow.Ticks);
                _db.RunInTransaction(() =>
                {
                    foreach (var k in entries)
                        _db.Delete<SchemaContent>(k.Id);
                });
            }
        }

        public bool Exists(string key)
        {
            SchemaContent ent;
            lock (_dblock)
            {
                ent = _db.Find<SchemaContent>(key);
            }

            return ent != null;
        }

        public string Get(string key)
        {
            SchemaContent ent;
            lock (_dblock)
            {
                ent = _db.Query<SchemaContent>($"SELECT {nameof(ent.Contents)} FROM {nameof(SchemaContent)} WHERE {nameof(ent.Id)} = ?", key).FirstOrDefault();
            }

            return ent?.Contents;
        }

        public T Get<T>(string key)
        {
            SchemaContent ent;
            lock (_dblock)
            {
                ent = _db.Query<SchemaContent>($"SELECT {nameof(ent.Contents)} FROM {nameof(SchemaContent)} WHERE {nameof(ent.Id)} = ?", key).FirstOrDefault();
            }

            if (ent == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(ent.Contents, _jsonSettings);
        }

        public bool IsExpired(string key)
        {
            SchemaContent ent;
            lock (_dblock)
            {
                ent = _db.Find<SchemaContent>(key);
            }

            if (ent == null)
                return true;

            return DateTime.UtcNow > ent.ExpirationDate;
        }

        public DateTime? GetExpiration(string key)
        {
            SchemaContent ent;
            lock (_dblock)
            {
                ent = _db.Query<SchemaContent>($"SELECT {nameof(ent.ExpirationDate)} FROM {nameof(SchemaContent)} WHERE {nameof(ent.Id)} = ?", key).FirstOrDefault();
            }

            return ent?.ExpirationDate;
        }

        public DateTime? GetLastUpdate(string key)
        {
            SchemaContent ent;
            lock (_dblock)
            {
                ent = _db.Query<SchemaContent>($"SELECT {nameof(ent.LastUpdate)} FROM {nameof(SchemaContent)} WHERE {nameof(ent.Id)} = ?", key).FirstOrDefault();
            }

            return ent?.LastUpdate;
        }
    }
}
