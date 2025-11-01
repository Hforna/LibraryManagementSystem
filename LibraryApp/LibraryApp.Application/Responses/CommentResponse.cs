namespace LibraryApp.Application.Responses;

public class CommentResponse
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long BookId { get; set; }
    public string Content { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}

public class CommentsPaginatedResponse
{
    public List<CommentResponse> Comments { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}