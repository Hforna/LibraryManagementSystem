using LibraryApp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Context
{
    // Implementação do padrão Unit of Work para gerenciar transações e repositórios
    public class UnitOfWork : IUnitOfWork
    {
        // Contexto do banco de dados
        private readonly DataContext _context;
        // Repositório de usuários
        public IUserRepository UserRepository { get; }
        // Repositório de livros
        public IBookRepository BookRepository { get; }
        // Repositório genérico para entidades comuns
        public IGenericRepository GenericRepository { get; }

        // Repositório de comentários
        public ICommentRepository CommentRepository { get; }

        // Injeta o contexto e os repositórios necessários
        public UnitOfWork(DataContext context, IUserRepository userRepository, IBookRepository bookRepository, IGenericRepository genericRepository, ICommentRepository commentRepository)
        {
            // Atribuição do contexto do banco
            _context = context;
            UserRepository = userRepository;
            CommentRepository = commentRepository;
            BookRepository = bookRepository;
            GenericRepository = genericRepository;
        }

        // Persiste todas as alterações pendentes no banco de dados
        public async Task Commit()
        {
            // Salva as mudanças de forma assíncrona
            await _context.SaveChangesAsync();
        }
    }
}
