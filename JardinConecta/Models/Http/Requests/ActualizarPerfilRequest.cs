using System.ComponentModel.DataAnnotations;

namespace JardinConecta.Models.Http.Requests
{
    public class ActualizarPerfilRequest
    {
        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Apellido { get; set; } = null!;

        public string? Documento { get; set; }
    }
}
