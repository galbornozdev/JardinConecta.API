namespace JardinConecta.Models.ViewModels.EmailTemplates
{
    public class VerificacionEmailViewModel : BaseEmailModel
    {
        public VerificacionEmailViewModel() : base("Verificación de correo electrónico")
        {

        }
        public string BaseUrl { get; set; }
        public string Token { get; set; }
    }
}
