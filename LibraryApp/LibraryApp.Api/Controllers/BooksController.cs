using LibraryApp.Application.Requests;
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

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromForm]CreateBookRequest request)
        {
            var result = await _bookService.CreateBook(request);

            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetBook([FromRoute] long id)
        {
            var result = await _bookService.GetBook(id);

            return Ok(result);
        }
    }
}
