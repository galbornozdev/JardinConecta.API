using System.ComponentModel.DataAnnotations;

namespace JardinConecta.Models.Http.Requests
{
    public class AltaUsuarioRequest
    {
        [EmailAddress]
        public string Email { get; set; } = null!;

        //public string Nombre { get; set; } = null!;
        //public string Apellido { get; set; } = null!;

        //[MinLength(8)]
        //public string Documento { get; set; } = null!;

        [MinLength(6)]
        public string Password { get; set; } = null!;

        //telefono
        //public string CaracteristicaPais { get; set; } = null!;
        //public string CodigoArea { get; set; } = null!;
        //public string Numero { get; set; } = null!;
    }
}
