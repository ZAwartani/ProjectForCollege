using Microsoft.AspNetCore.Authorization;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;
using System.Security.Claims;

public class OrganizationApprovedHandler : AuthorizationHandler<OrganizationApprovedRequirement>
{
    private readonly IGenericRepository<Organization, int> _context;
    private readonly ISpecificationBuilderFactory _specificationBuilderFactory;

    public OrganizationApprovedHandler(
        IGenericRepository<Organization, int> context,
        ISpecificationBuilderFactory specificationBuilderFactory)
    {
        _context = context;
        _specificationBuilderFactory = specificationBuilderFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationApprovedRequirement requirement)
    {
        var idClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (idClaim == null)
            return;

        if (!int.TryParse(idClaim.Value, out int organizationId))
            return;

        var specification = _specificationBuilderFactory
            .CreateBuilder<Organization>()
            .AddCriteria(x => x.Id == organizationId);

        var organization = await _context.GetAsync(specification);

        if (organization?.Status == RegisterStatus.Approved &&
            organization.Longitude != null &&
            organization.Latitude != null)
        {
            context.Succeed(requirement);
        }
    }
}