namespace LibraryApp.Application.Responses;

public class CommentResponse
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long BookId { get; set; }
    public string Content { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}