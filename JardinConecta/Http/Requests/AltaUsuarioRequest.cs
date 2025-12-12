using System.ComponentModel.DataAnnotations;

namespace JardinConecta.Http.Requests
{
    public class AltaUsuarioRequest : IHasIdJardin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Documento { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        //telefono
        public string CaracteristicaPais { get; set; } = null!;
        public string CodigoArea { get; set; } = null!;
        public string Numero { get; set; } = null!;

        public bool EsEducador { get; set; }
        public ICollection<Tutela_CreateUsuarioRequest> Tutelas { get; set; } = [];

        public ICollection<Guid> Salas { get; set; } = [];

        public class Tutela_CreateUsuarioRequest
        {
            public Guid IdInfante { get; set; }
            public int IdTipo { get; set; }
        }

        public Guid? IdJardin { get; set; }
    }
}
