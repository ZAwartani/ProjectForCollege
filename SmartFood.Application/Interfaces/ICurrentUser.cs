
namespace SmartFood.Application.Interfaces
{
    public interface ICurrentUser
    {
        public int Id { get;}
        string Email { get; }
        string Role { get; }
        bool IsAuthenticated { get; }

        bool IsInRole(string role);
    }
}
