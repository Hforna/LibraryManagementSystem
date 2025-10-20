using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Infrastructure.Services.Security
{
    /// <summary>
    /// Serviço responsável por realizar a criptografia e verificação de senhas utilizando BCrypt.
    /// </summary>
    public class PasswordCryptography : IPasswordCryptography
    {
        /// <summary>
        /// Compara uma senha em texto puro com um hash criptografado.
        /// </summary>
        /// <param name="password">Senha em texto puro fornecida pelo usuário.</param>
        /// <param name="hash">Hash armazenado no banco de dados.</param>
        /// <returns><c>true</c> se a senha corresponder ao hash; caso contrário, <c>false</c>.</returns>
        public bool CompareHash(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        /// <summary>
        /// Gera um hash criptografado a partir de uma senha em texto puro.
        /// </summary>
        /// <param name="password">Senha em texto puro a ser criptografada.</param>
        /// <returns>Hash resultante da criptografia BCrypt.</returns>
        public string Encrypt(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
