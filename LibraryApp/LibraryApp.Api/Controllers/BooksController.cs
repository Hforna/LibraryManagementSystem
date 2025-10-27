using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Api.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar operações de livros no sistema de biblioteca.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="BooksController"/>.
        /// </summary>
        /// <param name="bookService">O serviço de livros para manipular operações de livros.</param>
        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Cria um novo livro no sistema de biblioteca.
        /// </summary>
        /// <param name="request">A requisição de criação do livro contendo título, descrição, arquivo, capa e IDs de categorias.</param>
        /// <returns>Um <see cref="BookResponse"/> contendo os detalhes do livro criado.</returns>
        /// <response code="200">Retorna o livro recém-criado.</response>
        /// <response code="400">Se a validação da requisição falhar (ex: título excede 100 caracteres, descrição excede 500 caracteres).</response>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/Books
        ///     Content-Type: multipart/form-data
        ///     
        ///     Title: "Dom Casmurro"
        ///     Description: "Um clássico da literatura brasileira"
        ///     File: [arquivo_livro.pdf]
        ///     Cover: [imagem_capa.jpg]
        ///     CategoriesIds: [1, 3, 5]
        /// 
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     GET /api/Books/1
        /// 
        /// </remarks>
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
        /// <param name="request">A requisição de atualização do livro contendo campos opcionais para atualizar (título, descrição, arquivo, capa, categorias).</param>
        /// <returns>Um <see cref="BookResponse"/> contendo os detalhes do livro atualizado.</returns>
        /// <response code="200">Retorna o livro atualizado.</response>
        /// <response code="400">Se a validação da requisição falhar (ex: título excede 100 caracteres, descrição excede 500 caracteres).</response>
        /// <response code="404">Se o livro com o ID especificado não for encontrado.</response>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     PUT /api/Books/1
        ///     Content-Type: application/json
        ///     
        ///     {
        ///         "title": "Dom Casmurro - Edição Revisada",
        ///         "description": "Uma descrição atualizada",
        ///         "categoriesIds": [1, 2, 3]
        ///     }
        /// 
        /// Todos os campos na requisição são opcionais. Apenas os campos fornecidos serão atualizados.
        /// </remarks>
        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook([FromRoute] long id, [FromBody] UpdateBookRequest request)
        {
            var result = await _bookService.UpdateBook(request, id);
            return Ok(result);
        }
    }
}