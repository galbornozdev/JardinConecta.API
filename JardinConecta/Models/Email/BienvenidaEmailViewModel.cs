namespace JardinConecta.Models.Email
{
    public class BienvenidaEmailViewModel : BaseEmailModel
    {
        public BienvenidaEmailViewModel() : base("Bienvenido a JardinConecta!")
        {
        
        }
        public string Name { get; set; } = default!;
    }
}
