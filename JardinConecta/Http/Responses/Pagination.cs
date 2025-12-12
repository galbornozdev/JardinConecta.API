namespace JardinConecta.Http.Responses
{
    public record Pagination<T>(IEnumerable<T> Items, int TotalPages, int CurrentPage, int PageSize);
}