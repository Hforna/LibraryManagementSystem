namespace LibraryApp.Domain.Entities
{
    // Situação de um livro dentro do fluxo de moderação
    public enum BookStatus
    {
        PendingAnalysis = 0, // Aguardando análise de um administrador
        Published = 1, // Aprovado e visível publicamente
        Rejected = 2 // Reprovado por um administrador
    }
}
