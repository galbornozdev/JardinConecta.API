namespace JardinConecta.Models.Http.Requests
{
    public class EditarInfanteRequest
    {
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public int Documento { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
