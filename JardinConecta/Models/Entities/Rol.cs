namespace JardinConecta.Models.Entities
{
    public class Rol
    {
        public const string ROL_ADMIN_JARDIN = "AdminJardin";
        public const string ROL_ADMIN_SISTEMA = "AdminSistema";
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }

    public enum RolId
    {
        Usuario = 10,
        AdminJardin = 20,
        AdminSistema = 30
    }
}
