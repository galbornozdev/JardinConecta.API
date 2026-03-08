namespace JardinConecta.Models.Http.Responses
{
    public class CodigoInvitacionResponse
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = null!;
        public Guid IdSala { get; set; }
        public Guid IdInfante { get; set; }
        public DateTime FechaExpiracion { get; set; }
    }

    public class CodigoInvitacionItemResponse
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreInfante { get; set; } = null!;
        public DateTime FechaExpiracion { get; set; }
        public bool EstaVigente { get; set; }
    }
}
