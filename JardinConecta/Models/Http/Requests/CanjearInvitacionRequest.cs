namespace JardinConecta.Models.Http.Requests
{
    public class CanjearInvitacionRequest
    {
        public string Codigo { get; set; } = null!;
        public string? DocumentoSufijo { get; set; }
        public int? IdTipoTutela { get; set; }
    }
}
