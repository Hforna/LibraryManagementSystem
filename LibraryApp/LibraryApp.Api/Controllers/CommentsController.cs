using LibraryApp.Api.Filters;
using LibraryApp.Application.Responses;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar operações de comentários no sistema de biblioteca.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="CommentsController"/>.
        /// </summary>
        /// <param name="commentService">O serviço de comentários para manipular operações de comentários.</param>
        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Deleta um comentário específico.
        /// </summary>
        /// <param name="commentId">O identificador único do comentário a ser deletado.</param>
        /// <returns>Uma resposta 204 No Content se a operação for bem-sucedida.</returns>
        /// <response code="204">O comentário foi deletado com sucesso.</response>
        /// <response code="401">Se o usuário não estiver autenticado.</response>
        /// <response code="403">Se o usuário não tiver permissão para deletar o comentário (não é o autor).</response>
        /// <response code="404">Se o comentário com o ID especificado não for encontrado.</response>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     DELETE /api/Comments/1
        ///     Authorization: Bearer {token}
        /// 
        /// Apenas o usuário que criou o comentário pode deletá-lo.
        /// </remarks>
        [HttpDelete("{commentId}")]
        [UserAuthenticated]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment([FromRoute] long commentId)
        {
            await _commentService.DeleteComment(commentId);
            return NoContent();
        }

        /// <summary>
        /// Recupera um comentário específico pelo seu identificador único.
        /// </summary>
        /// <param name="commentId">O identificador único do comentário a ser recuperado.</param>
        /// <returns>Um <see cref="CommentResponse"/> contendo os detalhes do comentário.</returns>
        /// <response code="200">Retorna o comentário solicitado.</response>
        /// <response code="404">Se o comentário com o ID especificado não for encontrado.</response>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     GET /api/Comments/1
        /// 
        /// Retorna informações como ID do comentário, ID do usuário, ID do livro,
        /// conteúdo do texto e data de publicação.
        /// </remarks>
        [HttpGet("{commentId}")]
        [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComment([FromRoute] long commentId)
        {
            var result = await _commentService.GetCommentById(commentId);
            return Ok(result);
        }
    }
}