using System.Collections.Generic;
using DAL.Models;
using MongoDB.Bson;

namespace DAL.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : EntityBase
    {
        TEntity Get(string id);
        TEntity Find(TEntity entity);
        IEnumerable<TEntity> GetAll();
        TEntity Add(TEntity entity);
        TEntity Update(TEntity entity);
        bool Remove(TEntity entity);
        bool Remove(string id);
    }
}
