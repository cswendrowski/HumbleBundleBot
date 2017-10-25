using HumbleBundleService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HumbleBundleService.Controllers
{
    public class CheckController : ApiController
    {
        // POST api/<controller>
        public void Post()
        {
            CheckService.CheckForNewBundles();
        }

        // PUT api/<controller>/5
        public void Put(string webhook)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(string webhook)
        {
        }
    }
}