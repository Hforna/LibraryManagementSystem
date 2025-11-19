using AutoMapper;
using LibraryApp.Application.Requests;
using LibraryApp.Domain.Repositories;

namespace LibraryApp.Application.Services;

public interface ICategoryService
{
    public Task<CategoriesResponse> GetAllCategories();
}

public class CategoryService : ICategoryService
{
    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    
    
    public async Task<CategoriesResponse> GetAllCategories()
    {
        var categories = await _uow.BookRepository.GetCategories();

        var response = new CategoriesResponse
        {
            Categories = _mapper.Map<List<CategoryResponse>>(categories)
        };

        return response;
    }
}