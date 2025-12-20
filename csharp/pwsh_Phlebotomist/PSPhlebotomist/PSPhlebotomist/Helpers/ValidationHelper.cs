using Microsoft.Extensions.Logging;

namespace PSPhlebotomist.Core.Helpers
{
    /// <summary>
    /// Helper methods for validating DLL files and paths.
    /// </summary>
    public class ValidationHelper
    {
        private readonly ILogger<ValidationHelper> _logger;

        public ValidationHelper(ILogger<ValidationHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

       /// <summary>
       /// Determines whether the specified file path refers to a valid DLL file.
       /// </summary>
       /// <remarks>This method performs several checks to ensure the file is a valid DLL, including
       /// verifying the path is not empty, the file exists, the path is not a directory, and the file is a valid PE
       /// (DLL/EXE) file. If <paramref name="throwOnError"/> is <see langword="false"/>, validation failures are logged
       /// and the method returns <see langword="false"/> instead of throwing exceptions.</remarks>
       /// <param name="dllPath">The full path to the DLL file to validate. Cannot be null, empty, or whitespace.</param>
       /// <param name="throwOnError">Specifies whether to throw an exception if validation fails. If <see langword="true"/>, an exception is
       /// thrown on error; otherwise, the method returns <see langword="false"/>.</param>
       /// <returns>Returns <see langword="true"/> if the file at <paramref name="dllPath"/> is a valid DLL; otherwise, <see
       /// langword="false"/>.</returns>
       /// <exception cref="ArgumentException">Thrown if <paramref name="dllPath"/> is null, empty, whitespace, refers to a directory, or is not a valid PE
       /// (DLL/EXE) file and <paramref name="throwOnError"/> is <see langword="true"/>.</exception>
       /// <exception cref="FileNotFoundException">Thrown if the file specified by <paramref name="dllPath"/> does not exist and <paramref name="throwOnError"/>
       /// is <see langword="true"/>.</exception>
        public bool IsValidDll(string dllPath, bool throwOnError = false)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                var message = "DLL path cannot be empty";
                _logger.LogError(message);
                if (throwOnError) throw new ArgumentException(message);
                return false;
            }

            if (!File.Exists(dllPath))
            {
                var message = $"DLL file does not exist: {dllPath}";
                _logger.LogError(message);
                if (throwOnError) throw new FileNotFoundException(message, dllPath);
                return false;
            }

            if (Directory.Exists(dllPath))
            {
                var message = $"Path is a directory, not a file: {dllPath}";
                _logger.LogError(message);
                if (throwOnError) throw new ArgumentException(message);
                return false;
            }

            if (!IsValidPEFile(dllPath))
            {
                var message = $"File is not a valid PE (DLL/EXE) file: {dllPath}";
                _logger.LogWarning(message);
                if (throwOnError) throw new ArgumentException(message);
                return false;
            }

            _logger.LogDebug("DLL validation passed: {DllPath}", dllPath);
            return true;
        }

        /// <summary>
        /// Determines whether the specified file is a valid Portable Executable (PE) file.
        /// </summary>
        /// <remarks>This method checks for the presence of standard PE file headers. If the file does not
        /// exist, is inaccessible, or is not a valid PE file, the method returns false. No exceptions are thrown for
        /// invalid files or I/O errors; errors are logged internally.</remarks>
        /// <param name="filePath">The full path to the file to validate. Cannot be null or empty.</param>
        /// <returns>true if the file at the specified path is a valid PE file; otherwise, false.</returns>
        public bool IsValidPEFile(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var br = new BinaryReader(fs);

                if (br.ReadUInt16() != 0x5A4D) return false;

                fs.Seek(0x3C, SeekOrigin.Begin);
                var peHeaderOffset = br.ReadInt32();

                fs.Seek(peHeaderOffset, SeekOrigin.Begin);
                return br.ReadUInt32() == 0x00004550;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating PE file: {FilePath}", filePath);
                return false;
            }
        }
    }
}
