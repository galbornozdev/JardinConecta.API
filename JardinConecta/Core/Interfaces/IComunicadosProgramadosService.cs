namespace JardinConecta.Core.Interfaces
{
    public interface IComunicadosProgramadosService
    {
        Task PublicarComunicadosProgramados(CancellationToken stoppingToken);
    }
}