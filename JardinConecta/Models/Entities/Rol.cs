namespace JardinConecta.Models.Entities
{
    public class Rol
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }

    public enum RolId
    {
        Familia = 1,
        Educador = 2
    }
}
