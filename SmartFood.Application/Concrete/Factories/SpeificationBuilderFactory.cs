using SmartFood.Application.Concrete.Specification;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;

namespace SmartFood.Application.Concrete.Factories
{
    public class SpeificationBuilderFactory : ISpecificationBuilderFactory{
        public ISpecification<TEntity> CreateBuilder<TEntity>() where TEntity : class
        {
            return BaseSpecificationBuilder<TEntity>.Create();
        }
    }
}
