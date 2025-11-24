namespace JardinConecta.Models.Entities
{
    public record Telefono
    {
        public string? CaracteristicaPais { get; set; }
        public string? CodigoArea { get; set; }
        public string? Numero { get; set; }
    }
}
