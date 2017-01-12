using System.Collections.Generic;
using DAL.Models;
using MongoDB.Bson;

namespace DAL.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : EntityBase
    {
        TEntity Get(string id);
        IEnumerable<TEntity> GetAll();
        bool Add(TEntity entity);
        bool Update(TEntity entity);
        bool Remove(TEntity entity);
        bool Remove(string id);
    }
}
