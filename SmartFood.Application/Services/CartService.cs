using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;
using System.Transactions;

namespace SmartFood.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IGenericRepository<Product, int> _RepoProduct;
        private readonly IGenericRepository<Carts, int> _RepoCarts;
        private readonly IGenericRepository<CartItem, int> _RepoCartItem;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly ICurrentUser _CurrentUser;

        public CartService(IGenericRepository<Product, int> repoProduct, 
            IGenericRepository<Organization, int> repoOrganization, 
            IGenericRepository<Carts, int> repoCarts, 
            IGenericRepository<CartItem, int> repoCartItem, 
            ISpecificationBuilderFactory factory,
            ICurrentUser currentUser)
        {
            _RepoProduct = repoProduct;
            _RepoCarts = repoCarts;
            _RepoCartItem = repoCartItem;
            _Factory = factory;
            _CurrentUser = currentUser;
        }
        private int OrganizationId => _CurrentUser.Id;


        public async Task ClearMyCartAsync()
        {
            var Specification = _Factory.CreateBuilder<Carts>()
                .AddCriteria(x => x.OrganizationId == OrganizationId);

            var Result = await _RepoCarts.ListAsync(Specification);
            if(Result.Count == 0 )
            {
                return;
            }
            await _RepoCarts.RemoveRangeAsync(Result);
        }

        public async Task<bool> RemoveItemAsync(int productId)
        {
            var Speicification = _Factory.CreateBuilder<Carts>()
                .AddCriteria(x => x.OrganizationId == OrganizationId);

            var ResultSpecificationOrgId = await _RepoCarts.GetAsync(Speicification);

            if(ResultSpecificationOrgId == null)
            {
                return false;
            }

            var SpecificationCartItem = _Factory.CreateBuilder<CartItem>()
                .AddCriteria(x => x.ProductId == productId 
                               && x.CartId == ResultSpecificationOrgId.Id);

            var ResultSpecificationCartItem = await _RepoCartItem.GetAsync(SpecificationCartItem);
            if(ResultSpecificationCartItem == null)
            {
                return false;
            }

            await _RepoCartItem.DeleteAsync(ResultSpecificationCartItem.Id);
            return true;
        }
        public async Task<bool> UpdateItemQuantityAsync(int productId, UpdateCartItemQuantityDto dto)
        {
            if(dto.Quantity < 1)
            {
                throw new ArgumentException("Quantity Must Be More Than One");
            }
            var Speicification = _Factory.CreateBuilder<Carts>()
                 .AddCriteria(x => x.OrganizationId == OrganizationId);

            var ResultSpecificationOrgId = await _RepoCarts.GetAsync(Speicification);

            if (ResultSpecificationOrgId == null)
            {
                return false;
            }

            var SpecificationCartItem = _Factory.CreateBuilder<CartItem>()
                .AddCriteria(x => x.ProductId == productId
                               && x.CartId == ResultSpecificationOrgId.Id)
                .AddInclude(item => item.Product.Restaurant);
                

            var ResultSpecificationCartItem = await _RepoCartItem.GetAsync(SpecificationCartItem);
            if (ResultSpecificationCartItem == null)
            {
                return false;
            }
            if(ResultSpecificationCartItem.Quantity <= 0 
                || ResultSpecificationCartItem.Product.CreateDate < DateTime.UtcNow.AddDays(-1)
                || ResultSpecificationCartItem.Product.Restaurant.Status != RegisterStatus.Approved)
            {
                throw new ArgumentException("This Product is no longer available");
            }

            if(dto.Quantity > ResultSpecificationCartItem.Product.Quantity)
            {
                throw new ArgumentException("Quantity Not Enought");
            }
            ResultSpecificationCartItem.Quantity = dto.Quantity;

            await _RepoCartItem.UpdateAsync(ResultSpecificationCartItem);
            return true;
        }
        public async Task<bool> AddItemAsync(AddCartItemDto dto)
        {
            if (dto.Quantity < 1)
                throw new ArgumentException("Quantity must be at least 1.");

            var organizationId = OrganizationId;
            var cutoff = DateTime.UtcNow.AddDays(-1);

            using var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
                TransactionScopeAsyncFlowOption.Enabled);

            var productSpecification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.Id == dto.ProductId &&
                    p.Quantity > 0 &&
                    p.CreateDate >= cutoff &&
                    p.Restaurant.Status == RegisterStatus.Approved);

            var product = await _RepoProduct.GetAsync(productSpecification);

            if (product == null)
                return false;

            if (dto.Quantity > product.Quantity)
                throw new ArgumentException("Requested quantity exceeds available stock.");

            var cartSpecification = _Factory.CreateBuilder<Carts>()
                .AddCriteria(c => c.OrganizationId == organizationId);

            var cart = await _RepoCarts.GetAsync(cartSpecification);

            if (cart == null)
            {
                cart = new Carts(organizationId);
                cart.CartItems.Add(new CartItem
                (
                   product.Id,
                   dto.Quantity
                ));

                await _RepoCarts.AddAsync(cart);
            }
            else
            {
                var itemSpecification = _Factory.CreateBuilder<CartItem>()
                    .AddCriteria(i => i.CartId == cart.Id && i.ProductId == product.Id);

                var cartItem = await _RepoCartItem.GetAsync(itemSpecification);

                if (cartItem == null)
                {
                    await _RepoCartItem.AddAsync(new CartItem
                    (
                        cart.Id,
                        product.Id,
                        dto.Quantity
                    ));
                }
                else
                {
                    var totalQuantity = (long)cartItem.Quantity + dto.Quantity;

                    if (totalQuantity > product.Quantity)
                        throw new ArgumentException("Requested quantity exceeds available stock.");

                    cartItem.Quantity = (int)totalQuantity;
                    await _RepoCartItem.UpdateAsync(cartItem);
                }
            }

            transaction.Complete();
            return true;
        }
        public async Task<CartDto> GetMyCartAsync()
        {
            var organizationId = OrganizationId;

            var specification = _Factory.CreateBuilder<CartItem>()
                .AddCriteria(item => item.Carts.OrganizationId == organizationId)
                .AddInclude(item => item.Product.Restaurant)
                .ApplyOrderBy(item => item.Id);

            var items = await _RepoCartItem.ListAsync(specification);

            var itemDtos = items.Select(item => new CartItemDto(
                item.ProductId,
                item.Product.Name,
                item.Product.RestaurantId,
                item.Product.Restaurant.RestaurantName,
                item.Product.PhotoPath,
                item.Quantity
            )).ToList();

            return new CartDto(itemDtos);
        }
    }
}
