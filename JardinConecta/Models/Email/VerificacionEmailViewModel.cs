namespace JardinConecta.Models.Email
{
    public class VerificacionEmailViewModel : BaseEmailModel
    {
        public VerificacionEmailViewModel() : base("Verificación de correo electrónico")
        {
            
        }

        public string Codigo { get; set; }
    }
}
