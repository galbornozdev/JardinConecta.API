namespace JardinConecta.Http.Requests
{
    public class AltaSalaRequest : IHasIdJardin
    {
        public Guid? IdJardin { get; set; } = null;
        public string Nombre { get; set; }
    }
}
