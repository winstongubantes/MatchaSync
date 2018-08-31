using System.Collections.Generic;
using System.Threading.Tasks;
using Matcha.Sync.Model;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Matcha.Sync.Api
{
    public abstract class BaseController<T> : ODataController where T : ISynchronizable
    {
        [EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.All)]
        public abstract IActionResult Get();

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object obj)
        {
            var dataList = JsonConvert.DeserializeObject<IList<T>>(obj.ToString());
            var result = dataList;

            foreach (var data in dataList)
            {
                if (data.IsDeleted)
                    await Delete(data);
                else if (data.Id == 0)
                    await Insert(data);
                else
                    await Update(data);
            }

            return Ok("Success");
        }

        protected abstract Task Delete(T data);
        protected abstract Task Insert(T data);
        protected abstract Task Update(T data);
    }
}
