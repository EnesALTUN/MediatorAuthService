using System.Security.Claims;

namespace MediatorAuthService.Application.Extensions
{
    public static class UserContextExtensions
    {
        public static Guid Id(this ClaimsPrincipal claimsPrincipal)
        {
            string userId = claimsPrincipal.Claims.First(x => x.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.Ordinal)).Value;

            return new Guid(userId);
        }

        public static string Email(this ClaimsPrincipal claimsPrincipal)
        {
            string email = claimsPrincipal.Claims.First(x => x.Type.Equals(ClaimTypes.Email, StringComparison.Ordinal)).Value;

            return email;
        }

        public static string Name(this ClaimsPrincipal claimsPrincipal)
        {
            string name = claimsPrincipal.Claims.First(x => x.Type.Equals(ClaimTypes.Name, StringComparison.Ordinal)).Value;

            return name;
        }

        public static string SurName(this ClaimsPrincipal claimsPrincipal)
        {
            string surname = claimsPrincipal.Claims.First(x => x.Type.Equals(ClaimTypes.Surname, StringComparison.Ordinal)).Value;

            return surname;
        }

        public static string FullName(this ClaimsPrincipal claimsPrincipal)
        {
            return $"{Name} {SurName}";
        }
    }
}
