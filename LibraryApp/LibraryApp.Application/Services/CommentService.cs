using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Application.Services;

public interface ICommentService
{
    public Task<CommentResponse> CreateComment(CommentRequest request, long bookId);
    public Task DeleteComment(long id);
    public Task<CommentsPaginatedResponse> GetComments(long bookId, int page, int perPage);
    public Task<CommentResponse> GetCommentById(long id);
}

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ICommentService> _logger;
    private readonly ITokenService _tokenService;
    
    public CommentService(IUnitOfWork uow, IMapper mapper, ILogger<ICommentService> logger, ITokenService tokenService)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _tokenService = tokenService;
    }
    
    public async Task<CommentResponse> CreateComment(CommentRequest request, long bookId)
    {
        var user = await _tokenService.GetUserByToken();

        var book = await _uow.BookRepository.GetFullBook(bookId)
                   ?? throw new NotFoundException("Livro não foi encontrado");

        var comment = new Comment(user.Id, book.Id, request.Text);

        await _uow.GenericRepository.Add<Comment>(comment);
        await _uow.Commit();

        return _mapper.Map<CommentResponse>(comment);
    }

    public async Task DeleteComment(long id)
    {
        var comment = await _uow.GenericRepository.GetById<Comment>(id) 
                      ?? throw new NotFoundException("Comentario não foi encontrado");

        var user = await _tokenService.GetUserByToken();

        if (comment.UserId != user.Id)
            throw new UnauthorizedException("Usuário não tem permissão para deletar comentario");
        
        _uow.GenericRepository.Delete<Comment>(comment);
        await _uow.Commit();
    }

    public async Task<CommentResponse> GetCommentById(long id)
    {
        var comment = await _uow.GenericRepository.GetById<Comment>(id) 
            ?? throw new NotFoundException("Comentario não foi achado");

        return _mapper.Map<CommentResponse>(comment);
    }

    public async Task<CommentsPaginatedResponse> GetComments(long bookId, int page, int perPage)
    {
        var book = await _uow.GenericRepository.GetById<Book>(bookId) 
                    ?? throw new NotFoundException("Livro não foi encontrado");

        var comments = await _uow.CommentRepository.GetComments(bookId, page, perPage);

        var response = new CommentsPaginatedResponse() 
                        { Comments = comments.Results.Select(comment => _mapper.Map<CommentResponse>(comment)).ToList() };
        _mapper.Map(comments, response);

        return response;
    }
}