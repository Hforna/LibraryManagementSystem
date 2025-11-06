using LibraryApp.Api.Filters;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ICommentService _commentService;

        public BooksController(IBookService bookService, ICommentService commentService)
        {
            _bookService = bookService;
            _commentService = commentService;
        }

        /// <summary>
        /// Cria um novo livro no sistema de biblioteca.
        /// </summary>
        /// <param name="request">A requisição de criação do livro contendo título, descrição, arquivo, capa e IDs de categorias.</param>
        /// <returns>Um <see cref="BookResponse"/> contendo os detalhes do livro criado.</returns>
        /// <response code="200">Retorna o livro recém-criado.</response>
        /// <response code="400">Se a validação da requisição falhar.</response>
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromForm] BookRequest request)
        {
            var result = await _bookService.CreateBook(request);
            return Ok(result);
        }

        /// <summary>
        /// Recupera um livro específico pelo seu identificador único.
        /// </summary>
        /// <param name="id">O identificador único do livro a ser recuperado.</param>
        /// <returns>Um <see cref="BookResponse"/> contendo os detalhes do livro.</returns>
        /// <response code="200">Retorna o livro solicitado.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBook([FromRoute] long id)
        {
            var result = await _bookService.GetBook(id);
            return Ok(result);
        }

        /// <summary>
        /// Atualiza um livro existente no sistema de biblioteca.
        /// </summary>
        /// <param name="id">O identificador único do livro a ser atualizado.</param>
        /// <param name="request">A requisição de atualização do livro contendo campos opcionais para atualizar.</param>
        /// <returns>Um <see cref="BookResponse"/> contendo os detalhes do livro atualizado.</returns>
        /// <response code="200">Retorna o livro atualizado.</response>
        /// <response code="400">Se a validação da requisição falhar.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpPut("{id:long}")]
        [UserAuthenticated]
        public async Task<IActionResult> UpdateBook([FromRoute] long id, [FromForm] UpdateBookRequest request)
        {
            var result = await _bookService.UpdateBook(request, id);
            return Ok(result);
        }



        /// <summary>
        /// Deleta um livro do sistema de biblioteca.
        /// </summary>
        /// <param name="bookId">O identificador único do livro a ser deletado.</param>
        /// <returns>Uma resposta 204 No Content se a operação for bem-sucedida.</returns>
        /// <response code="204">O livro foi deletado com sucesso.</response>
        /// <response code="401">Se o usuário não estiver autenticado.</response>
        /// <response code="403">Se o usuário não tiver permissão para deletar o livro (não é o proprietário).</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpDelete("{bookId:long}")]
        [UserAuthenticated]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook([FromRoute] long bookId)
        {
            await _bookService.DeleteBook(bookId);
            return NoContent();
        }

        /// <summary>
        /// Recupera os comentários de um livro de forma paginada.
        /// </summary>
        /// <param name="bookId">O identificador único do livro.</param>
        /// <param name="page">O número da página a ser recuperada (começa em 1).</param>
        /// <param name="perPage">A quantidade de comentários por página.</param>
        /// <returns>Um <see cref="CommentsPaginatedResponse"/> contendo os comentários paginados.</returns>
        /// <response code="200">Retorna a lista paginada de comentários.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpGet("{bookId}/comments")]
        [ProducesResponseType(typeof(CommentsPaginatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCommentsPaginated([FromRoute] long bookId, [FromQuery] int page, [FromQuery] int perPage)
        {
            var result = await _commentService.GetComments(bookId, page, perPage);
            return Ok(result);
        }


        /// <summary>
        /// Publica um novo comentário em um livro.
        /// </summary>
        /// <param name="request">A requisição contendo o texto do comentário (máximo 500 caracteres).</param>
        /// <param name="bookId">O identificador único do livro onde o comentário será publicado.</param>
        /// <returns>Um <see cref="CommentResponse"/> contendo os detalhes do comentário criado.</returns>
        /// <response code="200">Retorna o comentário recém-criado.</response>
        /// <response code="400">Se a validação da requisição falhar (ex: texto excede 500 caracteres).</response>
        /// <response code="401">Se o usuário não estiver autenticado.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpPost("{bookId}/comments")]
        [UserAuthenticated]
        [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PublishComment([FromBody] CommentRequest request, [FromRoute] long bookId)
        {
            var result = await _commentService.CreateComment(request, bookId);
            return Ok(result);
        }

        /// <summary>
        /// Adiciona uma curtida (like) a um livro.
        /// </summary>
        /// <param name="bookId">O identificador único do livro a ser curtido.</param>
        /// <returns>Uma resposta 200 OK se a operação for bem-sucedida.</returns>
        /// <response code="200">O livro foi curtido com sucesso.</response>
        /// <response code="400">Se o usuário já curtiu este livro anteriormente.</response>
        /// <response code="401">Se o usuário não estiver autenticado.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpPost("{bookId}/like")]
        [UserAuthenticated]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LikeBook([FromRoute] long bookId)
        {
            await _bookService.LikeBook(bookId);
            return Ok();
        }

        /// <summary>
        /// Remove uma curtida (like) de um livro.
        /// </summary>
        /// <param name="bookId">O identificador único do livro a ter a curtida removida.</param>
        /// <returns>Uma resposta 200 OK se a operação for bem-sucedida.</returns>
        /// <response code="200">A curtida foi removida com sucesso.</response>
        /// <response code="400">Se o usuário ainda não curtiu este livro.</response>
        /// <response code="401">Se o usuário não estiver autenticado.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpDelete("{bookId}/unlike")]
        [UserAuthenticated]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnlikeBook([FromRoute] long bookId)
        {
            await _bookService.UnlikeBook(bookId);
            return Ok();
        }
    }
}