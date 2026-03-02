namespace JardinConecta.Models.ViewModels.EmailTemplates
{
    public class BaseEmailModel
    {
        public string Subject { get; } = default!;

        public BaseEmailModel(string subject)
        {
            Subject = subject;
        }
    }
}