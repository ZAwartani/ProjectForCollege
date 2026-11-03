using SmartFood.Application.Concrete.Specification;
using System.Linq.Expressions;

namespace SmartFood.Application.Interfaces.Specification
{
    public interface ISpecification<TEntity>
    {
        Expression<Func<TEntity, bool>>? Criteria { get; }
        List<Expression<Func<TEntity, object>>> Include { get; }
        Expression<Func<TEntity, object>>? OrderBy { get; }
        bool OrderByDescending { get; }
        public int Skip { get; }
        public int Take { get; }
        public bool IsPagging { get; }

        BaseSpecificationBuilder<TEntity> AddCriteria(Expression<Func<TEntity, bool>> criteria);
        BaseSpecificationBuilder<TEntity> AddInclude(Expression<Func<TEntity, object>> include);
        BaseSpecificationBuilder<TEntity> ApplyOrderBy(Expression<Func<TEntity, object>> orderBy , bool orderByDescending = false);
        BaseSpecificationBuilder<TEntity> ApplyPaging(int pageIndex, int pageSize);
    }
}
