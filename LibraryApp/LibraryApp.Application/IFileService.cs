using FileTypeChecker.Extensions;
using FileTypeChecker.Types;
using LibraryApp.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application
{
    // Interface que define o contrato do serviço de arquivos
    public interface IFileService
    {
        public (string ext, bool isValid) ValidateImage(Stream image); // Valida se o stream é uma imagem válida (JPEG ou PNG)
        public (string ext, bool isValid) ValidatePdf(Stream image); // Valida se o stream é um PDF válido
    }

    // Implementação do serviço de validação de arquivos
    public class FileService : IFileService
    {
        // Valida se o arquivo é uma imagem válida (JPEG ou PNG)
        public (string ext, bool isValid) ValidateImage(Stream image)
        {
            (string ext, bool isValid) = ("", false); // Inicializa a tupla de retorno

            try
            {
                // Verifica se o stream é um arquivo JPEG
                if (image.Is<JointPhotographicExpertsGroup>())
                    (ext, isValid) = (JointPhotographicExpertsGroup.TypeExtension, true);

                // Verifica se o stream é um arquivo PNG
                if (image.Is<PortableNetworkGraphic>())
                    (ext, isValid) = (PortableNetworkGraphic.TypeExtension, true);
            }
            catch (Exception ex)
            {
                throw new FileException("Não foi possivel analizar o arquivo enviado"); // Se houver erro ao analisar o arquivo
            }

            image.Position = 0; // Reseta a posição do stream para o início

            return (ext, isValid);
        }

        // Valida se o arquivo é um PDF válido
        public (string ext, bool isValid) ValidatePdf(Stream image)
        {
            (string ext, bool isValid) = ("", false); // Inicializa a tupla de retorno

            try
            {
                // Verifica se o stream é um arquivo PDF
                if (image.Is<PortableDocumentFormat>())
                    (ext, isValid) = (PortableDocumentFormat.TypeExtension, true);
            }catch(Exception ex)
            {
                throw new FileException("Não foi possivel analizar o arquivo enviado"); // Se houver erro ao analisar o arquivo
            }

            image.Position = 0; // Reseta a posição do stream para o início

            return (ext, isValid);
        }
    }
}