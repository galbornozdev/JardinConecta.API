using System.ComponentModel.DataAnnotations;

namespace JardinConecta.Models.Http.Requests
{
    public class EnviarMensajeRequest
    {
        [Required]
        public string Texto { get; init; } = null!;
        [Required]
        public Guid IdSala { get; init; }
    }
}
