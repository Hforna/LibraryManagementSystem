using FileTypeChecker.Extensions;
using FileTypeChecker.Types;
using LibraryApp.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Application
{
    public interface IFileService
    {
        public (string ext, bool isValid) ValidateImage(Stream image);
        public (string ext, bool isValid) ValidatePdf(Stream image);
    }

    public class FileService : IFileService
    {
        public (string ext, bool isValid) ValidateImage(Stream image)
        {
            (string ext, bool isValid) = ("", false);

            try
            {
                if (image.Is<JointPhotographicExpertsGroup>())
                    (ext, isValid) = (JointPhotographicExpertsGroup.TypeExtension, true);

                if (image.Is<PortableNetworkGraphic>())
                    (ext, isValid) = (PortableNetworkGraphic.TypeExtension, true);
            }
            catch (Exception ex)
            {
                throw new FileException("Não foi possivel analizar o arquivo enviado");
            }

            image.Position = 0;

            return (ext, isValid);
        }

        public (string ext, bool isValid) ValidatePdf(Stream image)
        {
            (string ext, bool isValid) = ("", false);

            try
            {
                if (image.Is<PortableDocumentFormat>())
                    (ext, isValid) = (PortableDocumentFormat.TypeExtension, true);
            }catch(Exception ex)
            {
                throw new FileException("Não foi possivel analizar o arquivo enviado");
            }

            image.Position = 0;
            
            return (ext, isValid);
        }
    }
}
