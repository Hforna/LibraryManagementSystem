using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Domain.Repositories;

namespace LibraryApp.Application.Services;

// Interface que define os contratos (métodos) do serviço de categorias
public interface ICategoryService
{
    public Task<CategoriesResponse> GetAllCategories(); // Retorna todas as categorias disponíveis no sistema
}

// Implementação do serviço de categorias
public class CategoryService : ICategoryService
{
    // DEPENDÊNCIAS INJETADAS
    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    private readonly IUnitOfWork _uow; // Unit of Work - fornece acesso aos repositórios
    private readonly IMapper _mapper; // AutoMapper - converte automaticamente entre objetos

    // Retorna todas as categorias disponíveis no sistema
    public async Task<CategoriesResponse> GetAllCategories()
    {
        var categories = await _uow.BookRepository.GetCategories(); // BUSCA: Obter todas as categorias do banco de dados

        var response = new CategoriesResponse // CONVERSÃO: Montar resposta usando AutoMapper
        {
            Categories = _mapper.Map<List<CategoryResponse>>(categories)
        };

        return response;
    }
}