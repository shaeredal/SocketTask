
using System.Collections.Generic;
using DAL.Models;
using DAL.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : EntityBase, new()
    {
        private IMongoCollection<TEntity> collection;
        public GenericRepository(MongoDbConnector dbConnector)
        {
            collection = dbConnector.GetDatabase().GetCollection<TEntity>(nameof(TEntity));
        }

        public TEntity Get(string id)
        {
            return collection.Find(x => x.Id == id ).FirstOrDefault();
        }

        public TEntity Find(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TEntity> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public TEntity Add(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public TEntity Update(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
