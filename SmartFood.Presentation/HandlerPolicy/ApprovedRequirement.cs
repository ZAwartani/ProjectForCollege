using Microsoft.AspNetCore.Authorization;

public class RestaurantApprovedRequirement : IAuthorizationRequirement
{
}

public class OrganizationApprovedRequirement : IAuthorizationRequirement
{
}