using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace BCFXML
{
    /// <summary>
    /// Extraction Class
    /// </summary>
    class ExtractZip
    {
        /// <summary>
        /// Extract BCFV Files from BCF ZIP File
        /// </summary>
        /// <param name="filepath">BCF Zip filepath</param>
        /// <returns>List of paths to BCFV files</returns>
        public static List<string> ExtractBCFVFiles(string filepath)
        {
            List<string> bcfvs = new List<string>();

            // Extract to temp path
            string extractPath = System.IO.Path.GetTempPath();
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                extractPath += Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(filepath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // locate bcfv entries
                    if (entry.FullName.EndsWith(".bcfv", StringComparison.OrdinalIgnoreCase))
                    {
                        // Gets the full path to ensure that relative segments are removed
                        string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName.Replace("/", "-")));
                        if (System.IO.File.Exists(destinationPath))
                            File.Delete(destinationPath);

                        entry.ExtractToFile(destinationPath);
                        bcfvs.Add(destinationPath);
                    }
                }
            }
            return bcfvs;
        }
    }
}
