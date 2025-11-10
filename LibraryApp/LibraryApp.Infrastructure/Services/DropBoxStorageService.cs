using Dropbox.Api;
using Dropbox.Api.Files;
using LibraryApp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using static Dropbox.Api.Files.PathOrLink;

namespace LibraryApp.Infrastructure.Services
{
    public class DropBoxStorageService : IStorageService
    {
        private readonly string _accessToken = "sl.u.AGHfG-F5PRjkMh2TYI4YndxylAoajunfc_SHn7sPanj-uCHM8wFoJ-ZtmPXhHFIlR_rGijr4Pn5laTPfmEEXT-2ky4ZgozMG7WeBsULBVl0arlLARnLhPio1nluFuzoPe5Xcu2xiZDFEGjqy5MhREvDDxrpr_LCVixinw-8oJmMxP50-YO0UiUjXdYCS5TweopeZpaOmQQONgnlXo3xMFh1Y4V8le2NxMh1Bcd1BXBgyPBJNEPg0r2iWPt1de09UCDK-6TMhgovFYof6nyUEuhoYoNdsi5JvVx98F-8mlrK1cuVuo559mwF8Rcr33BW7M0puk9KNmUcUqDEcBKfswMkTrrW3kjQlSOj-eKB1scAJF5etLqeLJ3m4fx3hOtyTiIt2yJrXmS1EbASvB_Zvy8N6sPliS0ZbJVhCKKzdxop9-OB-U6eBGmkR1t_XQ5nw6re4wOJuYa2dva9hOMcqFMdnAJynurB3PjQ8qm0khakdVzt89n3ZD2mfhJNDz6Yl3VNkUfEQ-U2DS9LHn40V1eYlugHGy86D9bAz3vVWDOXUzbZZAc47671BuXunvL10aZhImGZP4qrf8pBdiyGvtS48ThSm8tu8kchZWoxL9JlhTtDcitTxrRiSgK5AKp7bYOF35-0ZxrDgTviVfA8FhVJjbRUob8bnhjkN_kSMk20CcL5bavKU8Xx8jbsiaqIsqAWZqeJRpiwkNRATCScYoYAWL2Ysy-T9ndjujbBtv3Ez3BAPhpOeGe8wftDQY_0Ii-oClFKcfjFBhjjb1HDD33b8tfBjmOZwmZE9jqh02aVQo3D3Cc74nFhq3BvaVO8qzI4DdSY1-jz2GxfRvLhh15y3LP6AX13WZKFR77vx4a_VuQBvUgCpst9NCyL6LKwmZfV1gOF0G1cOAeZ-oqVMo0oU3dZtfkBNEQvx-l5wJNjbFr1MNfeELtADx-9SsVf2Wu9UBVPD0qN4MwOoOQ66yMbCaIUVO4l86XwtmDrBekp4ud06mLCZyxiYwjsszmgUDltE4NQ0CLXlr6_PIZ4RLAeMwUvEQZc8WPd_O2wPByQUXw8C7ABHvbIXcKRZqfk6sZttALPezhqwHKa4I3VaXtNMZgdWv2gewrntVGozKcwDlIodPxRl2cZmtQo9lpsk0F-lkIr3cKWWLE1vRcXA3a7Ba8wT4Fl7OjNqkxMFqrKjVpkrZaF4FCte3Q9cMyknKMI51RamVLuBTamgvlhcCrn_bcWKu5K54cCd2U6fgVTJ-5_DmqVV6mhj17WLDxm70nLYNQZUprApG5xFRCJjJvBk-DcWQFa7yA2hgcXTuGAS_wFqqTY0lpM_RxYAx2vlkJM";
        private const string BaseFilePath = "/uploads/files/";

        public DropBoxStorageService(string accessToken)
        {
            //_accessToken = accessToken;
        }

        public async Task<string> GetFileUrl(string fileName, string bookName)
        {
            using var client = new DropboxClient(_accessToken);
            try
            {
                var list = await client.Sharing.ListSharedLinksAsync($"{BaseFilePath}{fileName}", directOnly: true);
                if (list.Links.Any())
                {
                    return list.Links.FirstOrDefault()!.Url.Replace("?dl=0", "?raw=1");
                }else
                {
                    var link = await client.Sharing.CreateSharedLinkWithSettingsAsync($"{BaseFilePath}{fileName}");
                    return link.Url.Replace("?dl=0", "?raw=1");
                }
            }catch (Exception ex)
            {
                return "";
            }
        }

        public async Task DeleteFile(string fileName, string bookName)
        {
            using var client = new DropboxClient(_accessToken);
            await client.Files.DeleteV2Async(fileName);
        }

        public async Task UploadFile(Stream file, string fileName)
        {
            using var client = new DropboxClient(_accessToken);
            await client.Files.UploadAsync(
                path: $"{BaseFilePath}{fileName}",
                mode: WriteMode.Overwrite.Instance,
                body: file);
        }
    }
}
