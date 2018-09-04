using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SampleApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Custom")]
    public class CustomController : Controller
    {
        [HttpPost]
        [Route("GetSalesTaxRate")]
        public IActionResult GetSalesTaxRate([FromBody] int postalCode)
        {
            double rate = 5.6;  // Use a fake number for the sample.
            return Ok(rate);
        }
    }
}