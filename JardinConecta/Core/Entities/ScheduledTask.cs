namespace JardinConecta.Core.Entities
{
    public class ScheduledTask
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Cron { get; set; } = null!;
        public bool Habilitado { get; set; } = true;
    }

    public enum ScheduledTaskId
    {
        ComunicadosProgramados = 1
    }
}
