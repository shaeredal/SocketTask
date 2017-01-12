

using System;
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
            return collection.Find(x => x.Id == id).FirstOrDefault();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return collection.Find(new BsonDocument()).ToList();
        }

        public bool Add(TEntity entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = ObjectId.GenerateNewId().ToString();
                }
                collection.InsertOne(entity);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool Update(TEntity entity)
        {
            try
            {
                collection.ReplaceOne(Builders<TEntity>.Filter.Eq("_id", entity.Id), entity);
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool Remove(TEntity entity)
        {
            return Remove(entity.Id);
        }

        public bool Remove(string id)
        {
            try
            {
                collection.DeleteOne(x => x.Id == id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
