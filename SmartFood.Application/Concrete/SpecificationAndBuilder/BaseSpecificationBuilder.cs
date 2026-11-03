using SmartFood.Application.Interfaces.Specification;
using System.Linq.Expressions;

namespace SmartFood.Application.Concrete.Specification
{
    public class BaseSpecificationBuilder<TEntity> : ISpecification<TEntity>
    {
        public Expression<Func<TEntity, bool>>? Criteria { get; private set; }

        public List<Expression<Func<TEntity, object>>> Include { get; } = new();

        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

        public bool OrderByDescending { get; private set; }

        public int Skip { get; private set; }

        public int Take {  get; private set; }

        public bool IsPagging { get; private set; }
       

        private BaseSpecificationBuilder()
        {

        }

        public static ISpecification<TEntity> Create()
        {
            return new BaseSpecificationBuilder<TEntity>();
        }

        public BaseSpecificationBuilder<TEntity> AddCriteria(Expression<Func<TEntity, bool>> criteria)
        {
            Criteria = criteria;
            return this;
        }
        public BaseSpecificationBuilder<TEntity> AddInclude(Expression<Func<TEntity, object>> include)  
        {
            Include.Add(include);
            return this;
        }

        public BaseSpecificationBuilder<TEntity> ApplyOrderBy(Expression<Func<TEntity, object>> orderBy , bool orderByDescending = false)
        {
            this.OrderBy = orderBy;
            this.OrderByDescending = orderByDescending;
            return this;

        }
        public BaseSpecificationBuilder<TEntity> ApplyPaging(
            int pageIndex,
            int pageSize)
        {
            Skip = (pageIndex - 1) * pageSize;
            Take = pageSize;
            IsPagging = true;
            return this;
        }
    }
}
