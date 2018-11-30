using System;
using System.Collections.Generic;
using System.IO;

namespace SharedLibrary.Models
{
    public class FileSettingsModel
    {
        public DirectoryInfo ErrorFolder { get; set; }
        public DirectoryInfo InboundFolder { get; set; }
        public DirectoryInfo ProcessedFolder { get; set; }
        public IEnumerable<string> SiteAdminEmailAddresses { get; set; }
        public Guid IdentityId { get; set; }

        public FileSettingsModel()
        {

        }
        public FileSettingsModel(string errorFolder, string inboundFolder, string processedFolder)
        {
            SetPath(dir => ErrorFolder = dir, errorFolder);
            SetPath(dir => InboundFolder = dir, inboundFolder);
            SetPath(dir => ProcessedFolder = dir, processedFolder);
        }
        private void SetPath(Action<DirectoryInfo> setProperty, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(path);
            }

            var directoryExists = Directory.Exists(path);

            if (!directoryExists)
            {
                Directory.CreateDirectory(path);
                setProperty(new DirectoryInfo(path));
            }
            else
            {
                setProperty(new DirectoryInfo(path));
            }
        }
    }
}
