using LibraryApp.Application.Responses;
using LibraryApp.Domain.Constants;
using LibraryApp.Domain.Entities;
using LibraryApp.Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace LibraryApp.Application.Services
{
    // Interface que define o contrato do serviço usado pelo painel do administrador
    public interface IAdminService
    {
        public Task<AdminDashboardResponse> GetDashboard(); // Reúne as métricas exibidas no dashboard do administrador
    }

    // Implementação do serviço do painel do administrador
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _uow; // Unit of Work - acesso aos repositórios de livros
        private readonly UserManager<User> _userManager; // UserManager do ASP.NET Identity - usado para contar usuários por papel

        public AdminService(IUnitOfWork uow, UserManager<User> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        // Reúne as métricas de livros e usuários exibidas no dashboard do administrador
        public async Task<AdminDashboardResponse> GetDashboard()
        {
            var pendingAnalysisCount = await _uow.BookRepository.CountBooksByStatus(BookStatus.PendingAnalysis);
            var publishedCount = await _uow.BookRepository.CountBooksByStatus(BookStatus.Published);
            var rejectedCount = await _uow.BookRepository.CountBooksByStatus(BookStatus.Rejected);

            var students = await _userManager.GetUsersInRoleAsync(Roles.Student);
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);

            return new AdminDashboardResponse
            {
                TotalBooks = pendingAnalysisCount + publishedCount + rejectedCount,
                PendingAnalysisCount = pendingAnalysisCount,
                PublishedCount = publishedCount,
                RejectedCount = rejectedCount,
                TotalStudents = students.Count,
                TotalAdmins = admins.Count,
                TotalDownloads = await _uow.BookRepository.CountDownloads()
            };
        }
    }
}
