using Microsoft.EntityFrameworkCore;
using SmartFood.Application.Interfaces.Specification;
namespace SmartFood.Presentation.Repositories
{
    public class SpecificationEva<TEntity> where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputquery, ISpecification<TEntity> specification)
        {
            var Query = inputquery;
            if (specification.Criteria != null)
            {
                Query = Query.Where(specification.Criteria);
            }
            if (specification.Include != null)
            {
                foreach (var include in specification.Include)
                {
                    Query = Query.Include(include);
                }
            }
            if (specification.OrderBy != null)
            {
                if(specification.OrderByDescending == false)
                {
                    Query = Query.OrderBy(specification.OrderBy);
                }
                else
                {
                    Query = Query.OrderByDescending(specification.OrderBy);
                }
            }

            if(specification.IsPagging)
            {
                Query = Query.Skip(specification.Skip).Take(specification.Take);
            }
            return Query;
        }
    }
}
