namespace JardinConecta.Core.Entities
{
    public record Telefono
    {
        public string? CaracteristicaPais { get; set; }
        public string? CodigoArea { get; set; }
        public string? Numero { get; set; }
        public string NumeroCompleto => $"+{CaracteristicaPais}{CodigoArea}{Numero}";
    }
}
