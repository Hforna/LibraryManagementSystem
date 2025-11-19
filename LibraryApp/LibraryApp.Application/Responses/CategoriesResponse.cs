namespace LibraryApp.Application.Requests;

public class CategoriesResponse
{
    public List<CategoryResponse> Categories { get; set; }   
}

public class CategoryResponse
{
    public long Id { get; set; } 
    public string Name { get; set; }
}