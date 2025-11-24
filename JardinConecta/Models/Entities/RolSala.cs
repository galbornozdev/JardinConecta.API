namespace JardinConecta.Models.Entities
{
    public class RolSala
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }

    public enum RolSalaId
    {
        Familiar = 1,
        Educador = 2
    }
}
