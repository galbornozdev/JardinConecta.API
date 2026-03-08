namespace JardinConecta.Models.Http.Requests
{
    public class EditarInfanteRequest
    {
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Documento { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
    }
}
