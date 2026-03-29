using JardinConecta.Core.Entities;
using JardinConecta.Models.Http;

namespace JardinConecta.Models.Http.Requests
{
    public class AltaCodigoInvitacionRequest : IHasIdJardin
    {
        public Guid IdSala { get; set; }
        public Guid? IdInfante { get; set; }
        public TipoInvitacion TipoInvitacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public Guid? IdJardin { get; set; }
    }
}
