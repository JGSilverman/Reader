using System.Linq.Expressions;

namespace Reader.DataAccess
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<bool> DeleteOneAsync(TEntity entity);
        Task<bool> DeleteManyAsync(List<TEntity> entities);
        Task<List<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> filter = null);
        Task<TEntity> GetOneAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> InsertOneAsync(TEntity entity);
        Task<bool> InsertManyAsync(List<TEntity> entities);
        Task<bool> UpdateOneAsync(TEntity entity);
        Task<bool> UpdateManyAsync(List<TEntity> entities);
    }
}
