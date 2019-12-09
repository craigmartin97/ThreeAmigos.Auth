using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ThAmCo.Auth.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet("api/values")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}