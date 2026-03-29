namespace JardinConecta.Core.Services.Dtos;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalPages,
    int CurrentPage,
    int PageSize
);
