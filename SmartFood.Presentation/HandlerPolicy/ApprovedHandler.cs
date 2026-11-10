using Microsoft.AspNetCore.Authorization;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;
using SmartFood.Presentation.Persistence;
using System.Security.Claims;

public class ApprovedHandler : AuthorizationHandler<ApprovedRequirement>
{
    private readonly IGenericRepository<Restaurant, int> _context;
    private readonly ISpecificationBuilderFactory specificationBuilderFactory;

    private readonly ICurrentUser currentUser;
    public ApprovedHandler()
    {
    }

    public ApprovedHandler(IGenericRepository<Restaurant, int> context, ISpecificationBuilderFactory specificationBuilderFactory, ICurrentUser currentUser)
    {
        _context = context;
        this.specificationBuilderFactory = specificationBuilderFactory;
        this.currentUser = currentUser;
    }

    private int Id => currentUser.Id;
    protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    ApprovedRequirement requirement)
    {
        var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (idClaim == null)
            return;

        if (!int.TryParse(idClaim.Value, out int restaurantId))
            return;

        var specification = specificationBuilderFactory
            .CreateBuilder<Restaurant>()
            .AddCriteria(x => x.Id == restaurantId);

        var restaurant = await _context.GetAsync(specification);

        if (restaurant?.Status == RegisterStatus.Approved && restaurant?.Longitude != null && restaurant?.Latitude != null)
        {
            context.Succeed(requirement);
        }
    }
}