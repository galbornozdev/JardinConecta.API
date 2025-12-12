using JardinConecta.Common;
using System.Security.Claims;

namespace JardinConecta
{
    public static class Extensions
    {
        public static Guid GetIdUsuario(this ClaimsPrincipal claimsPrincipal)
        {
            return Guid.Parse(claimsPrincipal.FindFirst(Constants.CUSTOM_CLAIMS__ID_USUARIO)?.Value!);
        }

        public static int GetTipoUsuario(this ClaimsPrincipal claimsPrincipal)
        {
            return int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.Role)!);
        }

        public static Guid GetIdJardin(this ClaimsPrincipal claimsPrincipal)
        {
            return Guid.Parse(claimsPrincipal.FindFirst(Constants.CUSTOM_CLAIMS__ID_JARDIN)?.Value!);
        }
    }
}
