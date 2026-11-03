using SmartFood.Application.Interfaces.Specification;

namespace SmartFood.Application.Interfaces.Factory;

public interface ISpecificationBuilderFactory
{
    ISpecification<TEntity> CreateBuilder<TEntity>()  where TEntity : class;
}