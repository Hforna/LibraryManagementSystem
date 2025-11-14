using LibraryApp.Domain.Dtos;
using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using MimeKit;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridClient _sendGrid;
        private readonly string _fromEmail;
        private readonly string _fromUserName;

        public EmailService(SendGridClient sendGrid, string fromEmail, string fromUserName)
        {
            _sendGrid = sendGrid;
            _fromEmail = fromEmail;
            _fromUserName = fromUserName;
        }

        private string GetBodyTemplate(string body)
        {
            return $@"<!doctype html>
            <html lang=""pt-BR"">
            <head>
            <meta charset=""utf-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Email</title>
            <style>
              body {{ margin:0; padding:0; background-color:#f3f4f6; }}
              table {{ border-collapse:collapse; }}
              img {{ border:0; display:block; max-width:100%; height:auto; }}
              .wrapper {{ width:100%; table-layout:fixed; background-color:#f3f4f6; padding:20px 0; }}
              .container {{ max-width:600px; margin:0 auto; background:#ffffff; border-radius:8px; overflow:hidden; }}
              .spacer {{ height:24px; }}
              .header {{ padding:24px; text-align:center; background:linear-gradient(90deg,#6d28d9,#2563eb); color:#ffffff; }}
              .preheader {{ display:none; font-size:1px; color:#f3f4f6; line-height:1px; max-height:0; max-width:0; opacity:0; overflow:hidden; }}
              .content {{ padding:24px; color:#0f172a; font-family:Arial, Helvetica, sans-serif; font-size:16px; line-height:1.5; }}
              .h1 {{ font-size:20px; margin:0 0 12px 0; font-weight:600; }}
              .p {{ margin:0 0 16px 0; }}
              .button {{ display:inline-block; padding:12px 20px; background:#2563eb; color:#ffffff; text-decoration:none; border-radius:6px; font-weight:600; }}
              .two-col {{ width:100%; }}
              .col {{ vertical-align:top; }}
              .col-50 {{ width:50%; padding:8px; }}
              .footer {{ padding:16px; text-align:center; font-size:12px; color:#6b7280; }}
              @media screen and (max-width:480px) {{
                .container {{ width:100% !important; border-radius:0; }}
                .col-50 {{ display:block; width:100% !important; }}
                .header {{ padding:18px; }}
                .content {{ padding:16px; }}
              }}
            </style>
            </head>
            <body>
            <span class=""preheader"">Resumo curto do e-mail que aparece na caixa de entrada.</span>
            <table class=""wrapper"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
              <tr>
                <td align=""center"">
                  <table class=""container"" width=""600"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""header"">
                        <h1 style=""margin:0; font-family:Arial, Helvetica, sans-serif; font-size:24px;"">Assunto do Email</h1>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <img src=""https://via.placeholder.com/600x220.png?text=Imagem+Hero"" alt=""Imagem"" width=""600"" style=""display:block;"">
                      </td>
                    </tr>
                    <tr>
                      <td class=""content"">
                        <p class=""p"">{body}</p>
                        <div class=""spacer""></div>
                        <table class=""two-col"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td class=""col col-50"" style=""font-family:Arial, Helvetica, sans-serif; font-size:14px; color:#0f172a;"">
                              <strong>Benefício 1</strong>
                              <p style=""margin:8px 0 0 0;"">Descrição curta do benefício 1. Destaque rápido para atrair a atenção.</p>
                            </td>
                            <td class=""col col-50"" style=""font-family:Arial, Helvetica, sans-serif; font-size:14px; color:#0f172a;"">
                              <strong>Benefício 2</strong>
                              <p style=""margin:8px 0 0 0;"">Descrição curta do benefício 2. Informação prática e direta.</p>
                            </td>
                          </tr>
                        </table>
                        <div class=""spacer""></div>
                        <p class=""p"">Se tiver dúvidas, responda este e-mail. Estamos à disposição.</p>
                        <p style=""margin:0;"">Abraços,<br>Equipe</p>
                      </td>
                    </tr>
                    <tr>
                      <td class=""footer"">
                        <p style=""margin:0 0 8px 0;"">Endereço da empresa • Cidade, Estado</p>
                        <p style=""margin:0;"">Você está recebendo este e-mail porque se inscreveu em nossa lista.</p>
                        <p style=""margin:12px 0 0 0;""><a href=""#"" style=""color:#6b7280; text-decoration:underline;"">Gerenciar preferências</a> • <a href=""#"" style=""color:#6b7280; text-decoration:underline;"">Descadastrar</a></p>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
            </body>
            </html>
            ";
        }

        public async Task SendEmail(SendEmailDto dto)
        {
            var toEmail = new EmailAddress(dto.ToEmail, dto.ToUserName);
            var fromEmail = new EmailAddress(_fromEmail, _fromUserName);
            var subject = dto.Subject;
            var htmlContent = GetBodyTemplate(dto.Body);
            var msg = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, string.Empty, htmlContent);
            var response = await _sendGrid.SendEmailAsync(msg);
        }
    }
}
