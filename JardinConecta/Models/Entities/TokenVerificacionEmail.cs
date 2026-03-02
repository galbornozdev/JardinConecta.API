namespace JardinConecta.Models.Entities
{
    public class TokenVerificacionEmail
    {
        public Guid Id { get; set; }
        public Guid IdUsuario { get; set; }
        public string Token { get; set; } = null!;
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaUtilizacion { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
