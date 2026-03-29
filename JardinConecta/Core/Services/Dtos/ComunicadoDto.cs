using JardinConecta.Core.Entities;

namespace JardinConecta.Core.Services.Dtos
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
