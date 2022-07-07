namespace RenderExpressConnectResponse
{
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class IoExtensions
    {
        public static void NormalizeDirectory(ref string path)
        {
            if (!path.EndsWith('\\')) path += '\\';
        }

        public static async Task CopyFileAsync(this string sourceFile, string destinationFile, CancellationToken cancellationToken = default)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            int bufferSize = 65536;

            using FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);
            using FileStream destinationStream = new(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, fileOptions);

            await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        public static bool ValidateDirectory(this string path, bool checkwriteable = true)
        {
            try
            {
                DirectoryInfo di = new(path);

                if (!Directory.Exists(path))
                    throw new ArgumentException("The directory does not exist or cannot be accessed. \r\nPlease select an existing directory before proceeding.", nameof(path));
                if (checkwriteable && OperatingSystem.IsWindows())
                {
                    if (!di.CheckWriteAccess())
                        throw new SecurityException("You do not have permissions to write files to this directory. \r\nPlease select an existing directory before proceeding.");
                }

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
