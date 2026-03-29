using JardinConecta.Models.Http;

namespace JardinConecta.Models.Http.Requests
{
    public class AltaInfanteRequest : IHasIdJardin
    {
        public Guid? IdJardin { get; set; } = null;
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int Documento { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
