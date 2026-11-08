namespace SmartFood.Application.DTOs
{
    public record LoginAdminDto(
            string Email,
            string Password
    );

    public record AdminResponceTokenDto(
        string AccessToken,
        string RefreshToken
    );

    public record RegisterAdminDto(
       string Name,
       string Email,
       string Password
    );

    public record AdminRefreshTokenDto(
         string Email,
         string RefreshToken
    );

    public record UpdateOrganizationNotApprovedDto(
            int OrganizationId
    );
    public record UpdateRestaurantNotApprovedDto(
            int RestaurantId
    );
    public record RemoveOrganizationDto(
           int OrganizationId
    );

    public record RemoveRestaurantDto(
           int RestaurantId
    );

    public record ListOrganizationDto(
        int Id,
        string OrganizationName
    );

    public record ListRestaurantDto(
        int Id,
        string RestaurantName
    );


}


/*
 
 1- I need to login the admin
 2- I need to show what i have restaurant and same thing for orgnization not approved to update it (after show it i need click button for that and return the id for it to update)
 3- I need to remove it for that i need two record seperate to do it 
 
 
 
 */
