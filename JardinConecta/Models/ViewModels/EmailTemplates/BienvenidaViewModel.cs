namespace JardinConecta.Models.ViewModels.EmailTemplates
{
    public class BienvenidaViewModel : BaseEmailModel
    {
        public BienvenidaViewModel() : base("Bienvenido a JardinConecta!")
        {

        }
        public string Name { get; set; } = default!;
    }
}
