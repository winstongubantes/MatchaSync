using System;
using SQLite;

namespace Matcha.Sync.Mobile
{
    public class SchemaContent
    {
        [PrimaryKey]
        public string Id { get; set; }


        /// <summary>
        /// Additional ETag to set for Http Caching
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Main Contents.
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// Expiration data of the object, stored in UTC
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
