namespace JardinConecta.Models.Entities
{
    public class TipoUsuario
    {
        public const string ROL_ADMIN_JARDIN = "20";
        public const string ROL_ADMIN_SISTEMA = "30";
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }

    public enum TipoUsuarioId
    {
        Usuario = 10,
        AdminJardin = 20,
        AdminSistema = 30
    }
}
