namespace JardinConecta.Models.ViewModels.EmailTemplates
{
    public class AsignacionSalaViewModel : BaseEmailModel
    {
        public AsignacionSalaViewModel() : base("Fuiste asignado a una sala en JardinConecta")
        {
        }

        public string NombreEducador { get; set; } = default!;
        public string NombreSala { get; set; } = default!;
        public string NombreJardin { get; set; } = default!;
    }
}
