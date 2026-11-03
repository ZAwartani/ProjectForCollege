using Microsoft.EntityFrameworkCore;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Interfaces;
using SmartFood.Presentation.Persistence;

namespace SmartFood.Presentation.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity :class,  IEntity<TKey>
    {
        private readonly AppDbContext _context;
        public GenericRepository(AppDbContext context)
        {
            this._context = context;
        }
        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TKey Tkey)
        {
            var entity = await GetByIdAsync(Tkey);
                _context.Set<TEntity>().Remove(entity);
               await _context.SaveChangesAsync();
        }


        public async Task<List<TEntity>> GetAllAsync()
        {
            var list = await _context.Set<TEntity>().ToListAsync();
            return list;
        }

        public async Task<TEntity> GetAsync(ISpecification<TEntity> specification)
        {
            return await SpecificationEva<TEntity>
                .GetQuery(_context.Set<TEntity>().AsQueryable(), specification)
                .FirstOrDefaultAsync();
        }

        public async Task<TEntity?> GetByIdAsync(TKey key)
        {
            var res = await _context.Set<TEntity>().FindAsync(key);
            return res;
        }

        public async Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification)
        {
            return await SpecificationEva<TEntity>
                .GetQuery(_context.Set<TEntity>().AsQueryable(),specification)
                .ToListAsync();
        }

        public async Task RemoveRangeAsync(List<TEntity> entity)
        {

            _context.Set<TEntity>().RemoveRange(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(List<TEntity> entity)
        {
            _context.Set<TEntity>().UpdateRange(entity);
            await _context.SaveChangesAsync();
        }
    }
}
