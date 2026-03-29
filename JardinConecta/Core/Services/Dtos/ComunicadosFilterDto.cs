namespace JardinConecta.Core.Services.Dtos
{
    public record ComunicadosFilterDto(
        int Page,
        int? Estado,
        DateTime? FechaDesde,
        DateTime? FechaHasta
    )
    {
    }
}
