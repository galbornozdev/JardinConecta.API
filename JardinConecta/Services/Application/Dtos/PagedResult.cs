namespace JardinConecta.Services.Application.Dtos;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalPages,
    int CurrentPage,
    int PageSize
);
