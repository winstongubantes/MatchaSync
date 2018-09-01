using System;
using SQLite;

namespace Matcha.Sync.Mobile
{
    public class SchemaContent
    {
        [PrimaryKey]
        public string Id { get; set; }

        /// <summary>
        /// Main Contents.
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// Number of records, to be used for paging
        /// </summary>
        public long RecordCount { get; set; }

        /// <summary>
        /// Expiration data of the object, stored in UTC
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
