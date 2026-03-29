namespace JardinConecta.Services.Application.Dtos
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
