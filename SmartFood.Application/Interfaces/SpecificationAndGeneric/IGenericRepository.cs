using SmartFood.Core.Interfaces;

namespace SmartFood.Application.Interfaces.Specification
{
    public interface IGenericRepository<TEntity , TKey> where TEntity : class , IEntity<TKey>
    {
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task UpdateRangeAsync(List<TEntity> entity);
        Task RemoveRangeAsync(List<TEntity> entity);
        Task DeleteAsync(TKey Tkey);
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(TKey key);
        Task<TEntity> GetAsync(ISpecification<TEntity> specification);
        Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification);

    }
}
