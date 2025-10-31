using LibraryApp.Api.Filters;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpDelete("{commentId}")]
        [UserAuthenticated]
        public async Task<IActionResult> DeleteComment([FromRoute]long commentId)
        {
            await _commentService.DeleteComment(commentId);

            return NoContent();
        }
    }
}
