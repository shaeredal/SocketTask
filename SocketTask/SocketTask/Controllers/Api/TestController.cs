using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DAL;
using MongoDB.Driver;

namespace SocketTask.Controllers.Api
{
    public class TestController : ApiController
    {
        private readonly IMongoDatabase database;
        public TestController(MongoDbConnector dbConnector)
        {
            database = dbConnector.GetDatabase();
        }
        // GET api/<controller>
        public string Get()
        {
            var result = $"В базе данных {database} имеются следующие коллекции:";

            using (var collCursor = database.ListCollectionsAsync())
            {
                var colls = collCursor.Result.ToListAsync().Result; ;
                result = colls.Aggregate(result, (current, col) => current + col["name"]);
            }

            return result;
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}