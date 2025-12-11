namespace JardinConecta.Models.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }

    public enum RolId
    {
        Tutor = 1,
        Educador = 2
    }
}
