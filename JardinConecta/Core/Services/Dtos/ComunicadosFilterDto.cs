namespace JardinConecta.Core.Services.Dtos
{
    public record ComunicadosFilterDto(
        int Page,
        int? PageSize,
        int? Estado,
        DateTime? FechaDesde,
        DateTime? FechaHasta
    )
    {
    }
}
