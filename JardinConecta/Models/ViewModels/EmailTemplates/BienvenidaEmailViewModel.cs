namespace JardinConecta.Models.ViewModels.EmailTemplates
{
    public class BienvenidaEmailViewModel : BaseEmailModel
    {
        public BienvenidaEmailViewModel() : base("Bienvenido a JardinConecta!")
        {

        }
        public string Name { get; set; } = default!;
    }
}
