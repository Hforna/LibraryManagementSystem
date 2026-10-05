using LibraryApp.Api.Filters;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    // Controller com os recursos exclusivos do painel do administrador: dashboard e moderação de livros enviados por estudantes
    [Route("api/admin")]
    [ApiController]
    [AdminAuthenticated]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IBookService _bookService;
        private readonly IUserService _userService;

        public AdminController(IAdminService adminService, IBookService bookService, IUserService userService)
        {
            _adminService = adminService;
            _bookService = bookService;
            _userService = userService;
        }

        /// <summary>
        /// Retorna as métricas exibidas no dashboard do administrador (livros por situação, usuários e downloads).
        /// </summary>
        /// <response code="200">Retorna as métricas do dashboard.</response>
        /// <response code="403">Se o usuário autenticado não tiver o papel de administrador.</response>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _adminService.GetDashboard();
            return Ok(result);
        }

        /// <summary>
        /// Cria a conta de um usuário (estudante ou administrador). Usuários não podem se cadastrar sozinhos:
        /// toda conta é criada por um administrador, já com o e-mail confirmado.
        /// </summary>
        /// <param name="request">E-mail, username, senha e papel ("Student" por padrão, ou "Admin") da nova conta.</param>
        /// <response code="201">Retorna os dados do usuário criado.</response>
        /// <response code="400">Se a validação da requisição falhar (ex: email já cadastrado, papel inválido).</response>
        /// <response code="403">Se o usuário autenticado não tiver o papel de administrador.</response>
        [HttpPost("users")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
        {
            var result = await _userService.CreateUserByAdmin(request);
            return Created(string.Empty, result);
        }

        /// <summary>
        /// Lista, de forma paginada, os livros enviados por estudantes que estão aguardando análise.
        /// </summary>
        /// <param name="page">Número da página (começa em 1).</param>
        /// <param name="perPage">Quantidade de livros por página.</param>
        /// <response code="200">Retorna a lista paginada de livros pendentes de análise.</response>
        /// <response code="403">Se o usuário autenticado não tiver o papel de administrador.</response>
        [HttpGet("books/pending")]
        [ProducesResponseType(typeof(AdminBooksPaginatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBooksPendingAnalysis([FromQuery] int page, [FromQuery] int perPage)
        {
            var result = await _bookService.GetBooksPendingAnalysis(page, perPage);
            return Ok(result);
        }

        /// <summary>
        /// Baixa o arquivo original enviado pelo estudante, para que o administrador possa validar o conteúdo do livro.
        /// </summary>
        /// <param name="bookId">Identificador único do livro.</param>
        /// <response code="200">Retorna o arquivo do livro para download.</response>
        /// <response code="403">Se o usuário autenticado não tiver o papel de administrador.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpGet("books/{bookId:long}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadBookForReview([FromRoute] long bookId)
        {
            var (stream, fileName) = await _bookService.DownloadBookForReview(bookId);
            return File(stream, "application/pdf", fileName);
        }

        /// <summary>
        /// Aprova um livro em análise, publicando-o para todos os usuários.
        /// </summary>
        /// <param name="bookId">Identificador único do livro.</param>
        /// <response code="200">Retorna o livro aprovado e publicado.</response>
        /// <response code="400">Se o livro não estiver aguardando análise.</response>
        /// <response code="403">Se o usuário autenticado não tiver o papel de administrador.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpPut("books/{bookId:long}/approve")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveBook([FromRoute] long bookId)
        {
            var result = await _bookService.ApproveBook(bookId);
            return Ok(result);
        }

        /// <summary>
        /// Reprova um livro em análise, informando o motivo ao estudante.
        /// </summary>
        /// <param name="bookId">Identificador único do livro.</param>
        /// <param name="request">Motivo da reprovação.</param>
        /// <response code="200">Retorna o livro reprovado.</response>
        /// <response code="400">Se o livro não estiver aguardando análise ou o motivo não for informado.</response>
        /// <response code="403">Se o usuário autenticado não tiver o papel de administrador.</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        [HttpPut("books/{bookId:long}/reject")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectBook([FromRoute] long bookId, [FromBody] RejectBookRequest request)
        {
            var result = await _bookService.RejectBook(bookId, request.Reason);
            return Ok(result);
        }
    }
}
