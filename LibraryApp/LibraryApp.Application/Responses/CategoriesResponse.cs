namespace LibraryApp.Application.Requests;

// Classe de resposta que encapsula uma lista de categorias
public class CategoriesResponse
{
    public List<CategoryResponse> Categories { get; set; } // Lista contendo todas as categorias disponíveis no sistema 
}

// Classe de resposta representando uma única categoria
public class CategoryResponse
{
    public long Id { get; set; } // Identificador único da categoria no banco de dados
    public string Name { get; set; } // Nome descritivo da categoria
}