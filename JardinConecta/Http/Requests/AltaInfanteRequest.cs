namespace JardinConecta.Http.Requests
{
    public class AltaInfanteRequest : IHasIdJardin
    {
        public Guid? IdJardin { get; set; } = null;
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
