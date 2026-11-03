using SmartFood.Application.Interfaces;
using System.Security.Claims;

namespace TodoApp.Presentation.Service
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;
        public int Id
        {
            get
            {
                var userId = _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    throw new UnauthorizedAccessException();
                }
                return int.Parse(userId);
            }
        }

        public string Email =>
              User.FindFirstValue(ClaimTypes.Email);

        public string Role =>
            User.FindFirstValue(ClaimTypes.Role);

        public bool IsAuthenticated =>
            User.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }
    }
}
