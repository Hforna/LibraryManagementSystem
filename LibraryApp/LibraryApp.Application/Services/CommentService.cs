using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Application.Responses;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Exceptions;
using LibraryApp.Domain.Repositories;
using LibraryApp.Domain.Services;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Application.Services;

// Interface que define o contrato do serviço de comentários
public interface ICommentService
{
    public Task<CommentResponse> CreateComment(CommentRequest request, long bookId); // Cria um novo comentário para um livro específico
    public Task DeleteComment(long id); // Remove um comentário pelo ID
    public Task<CommentsPaginatedResponse> GetComments(long bookId, int page, int perPage); // Busca comentários de um livro com paginação
    public Task<CommentResponse> GetCommentById(long id); // Busca um comentário específico pelo ID
}

// Implementação concreta do serviço de comentários
public class CommentService : ICommentService
{
    private readonly IUnitOfWork _uow; // Unit of Work - gerencia transações e acesso aos repositórios
    private readonly IMapper _mapper; // AutoMapper - converte entre entidades de domínio e DTOs (Request/Response)
    private readonly ILogger<ICommentService> _logger; // Logger - registra informações, avisos e erros
    private readonly ITokenService _tokenService; // Serviço para gerenciar tokens de autenticação e obter usuário logado

    // Construtor - recebe as dependências por injeção
    public CommentService(IUnitOfWork uow, IMapper mapper, ILogger<ICommentService> logger, ITokenService tokenService)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _tokenService = tokenService;
    }

    // Método para criar um novo comentário
    public async Task<CommentResponse> CreateComment(CommentRequest request, long bookId)
    {
        var user = await _tokenService.GetUserByToken(); // Obtém o usuário autenticado através do token
        var book = await _uow.BookRepository.GetFullBook(bookId)  // Busca o livro completo no banco de dados
                   ?? throw new NotFoundException("Livro não foi encontrado");
        var comment = new Comment(user.Id, book.Id, request.Text); // Cria uma nova entidade Comment com os dados do usuário, livro e texto

        await _uow.GenericRepository.Add<Comment>(comment); // Cria uma nova entidade Comment com os dados do usuário, livro e texto
        await _uow.Commit(); // Persiste as mudanças no banco de dados

        return _mapper.Map<CommentResponse>(comment); 
    }

    // Método para deletar um comentário
    public async Task DeleteComment(long id)
    {
        var comment = await _uow.GenericRepository.GetById<Comment>(id) // Busca o comentário pelo ID
                      ?? throw new NotFoundException("Comentario não foi encontrado");
        var user = await _tokenService.GetUserByToken();  // Obtém o usuário autenticado

        // Verifica se o usuário logado é o dono do comentário  
        if (comment.UserId != user.Id)
            throw new UnauthorizedException("Usuário não tem permissão para deletar comentario");
        
        _uow.GenericRepository.Delete<Comment>(comment); // Marca o comentário para exclusão
        await _uow.Commit(); // Persiste a exclusão no banco de dados
    }

    // Método para buscar um comentário específico
    public async Task<CommentResponse> GetCommentById(long id)
    {
        var comment = await _uow.GenericRepository.GetById<Comment>(id) // Busca o comentário pelo ID
            ?? throw new NotFoundException("Comentario não foi achado");

        return _mapper.Map<CommentResponse>(comment);
    }

    // Método para buscar comentários de um livro com paginação
    public async Task<CommentsPaginatedResponse> GetComments(long bookId, int page, int perPage)
    {
        var book = await _uow.GenericRepository.GetById<Book>(bookId) // Verifica se o livro existe
                    ?? throw new NotFoundException("Livro não foi encontrado");
        var comments = await _uow.CommentRepository.GetComments(bookId, page, perPage); // Busca os comentários do livro com paginação
        var response = new CommentsPaginatedResponse() // Cria o objeto de resposta paginada
        { Comments = comments.Results.Select(comment => _mapper.Map<CommentResponse>(comment)).ToList() };
        _mapper.Map(comments, response); // Mapeia informações de paginação (total de páginas, página atual, etc.)

        return response;
    }
}