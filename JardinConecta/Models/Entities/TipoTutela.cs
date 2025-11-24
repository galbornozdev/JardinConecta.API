namespace JardinConecta.Models.Entities
{
    public class TipoTutela
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = null!;
    }
    public enum TipoTutelaId
    {
        Madre = 1,
        Padre = 2,
        Tutor = 3
    }
}
