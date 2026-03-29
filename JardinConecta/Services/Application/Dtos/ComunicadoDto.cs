using JardinConecta.Models.Entities;

namespace JardinConecta.Services.Application.Dtos
{
    public record ComunicadoDto(
        string Titulo,
        string Contenido,
        string ContenidoTextoPlano,
        DateTime? FechaPrograma,
        int Estado = (int)EstadoComunicado.Publicado
    )
    {
    }
}
