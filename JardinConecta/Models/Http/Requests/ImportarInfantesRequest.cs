using Microsoft.AspNetCore.Http;

namespace JardinConecta.Models.Http.Requests;

public class ImportarInfantesRequest
{
    public IFormFile Archivo { get; set; } = null!;
}
