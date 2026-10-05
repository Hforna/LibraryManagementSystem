using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Application.Requests
{
    // Classe de requisição para o usuário autenticado alterar a própria senha
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Informe a senha atual")] // Senha atual, usada para confirmar que é o próprio usuário
        public string CurrentPassword { get; set; }
        [MinLength(8, ErrorMessage = "Senha deve conter 8 ou mais digitos")] // Nova senha escolhida pelo usuário
        public string NewPassword { get; set; }
    }

    // Classe de requisição para solicitar o link de redefinição de senha por e-mail
    public class ForgotPasswordRequest
    {
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "Formato invalido de e-mail")] // E-mail da conta
        public string Email { get; set; }
    }

    // Classe de requisição para redefinir a senha com o token recebido por e-mail
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Informe o e-mail")] // E-mail da conta
        public string Email { get; set; }
        [Required(ErrorMessage = "Informe o token de redefinição")] // Token recebido no link do e-mail (codificado em Base64URL)
        public string Token { get; set; }
        [MinLength(8, ErrorMessage = "Senha deve conter 8 ou mais digitos")] // Nova senha escolhida pelo usuário
        public string NewPassword { get; set; }
    }
}
