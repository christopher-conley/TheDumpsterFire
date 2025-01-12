using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class to invoke signtool.exe
    /// </summary>
    static class SignToolRunner
    {
        private const string VerifyDigitalSignatureCmd = "verify /pa \"{0}\"";
        private const string SignBinaryWithPfxCmd = "sign /fd \"{0}\" /t \"{1}\" /f \"{2}\" /p \"{3}\" \"{4}\"";
        private const string SignBinaryWithCertCmd = "sign /fd \"{0}\" /sha1 \"{1}\" /s \"{2}\"{3} /a /t \"{4}\" \"{5}\"";

        private static string _signtoolPath;

        /// <summary>
        /// Invokes signtool.exe and returns exit-code along with standard-output and standard-error.
        /// </summary>
        public static int ExecuteCommand(string path, SignData arguments, out string output, out string error)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            string commandArguments;

            if (!string.IsNullOrEmpty(arguments.Thumbprint) && arguments.CertificateStore != null)
            {
                commandArguments = String.Format(SignBinaryWithCertCmd,
                                        arguments.HashAlgorithm.Name,
                                        arguments.Thumbprint,
                                        arguments.CertificateStore.Name,
                                        arguments.CertificateStore.Location == StoreLocation.LocalMachine ? " /sm" : string.Empty,
                                        arguments.TimestampServer,
                                        path);
            }
            else
            {
                commandArguments = String.Format(SignBinaryWithPfxCmd,
                                        arguments.HashAlgorithm.Name,
                                        arguments.TimestampServer,
                                        arguments.CertificatePath,
                                        arguments.CertificatePassword,
                                        path);
            }

            return ExecuteCommand(commandArguments, out output, out error);
        }

        /// <summary>
        /// Verifies digital signature of specified binary.
        /// </summary>
        public static int ExecuteSignatureVerification(string path, out string output, out string error)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            // Verify digital signature
            string commandArguments = String.Format(VerifyDigitalSignatureCmd, path);

            return ExecuteCommand(commandArguments, out output, out error);
        }

        /// <summary>
        /// Invokes signtool.exe and returns exit-code along with standard-output and standard-error.
        /// </summary>
        public static int ExecuteCommand(string arguments, out string output, out string error)
        {
            if (string.IsNullOrEmpty(_signtoolPath))
            {
                _signtoolPath = EnsureSignTool();
            }

            if (string.IsNullOrEmpty(_signtoolPath))
            {
                throw new ArgumentException("Cannot find signtool.exe");
            }

            var procStartInfo = new ProcessStartInfo(_signtoolPath, arguments);
            procStartInfo.CreateNoWindow = true;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;

            var proc = new Process();
            proc.StartInfo = procStartInfo;
            try
            {
                proc.Start();
                output = proc.StandardOutput.ReadToEnd();
                error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                return proc.ExitCode;
            }
            finally
            {
                proc.Close();
            }
        }

        private static string EnsureSignTool()
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (string.IsNullOrEmpty(programFilesX86))
            {
                programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            string matchesWin10AndAbove = @"\\\\(?<Winver>(10|10\.0|10\.\d\d|10\.1|10\.1\d|10\.\d+))\\\\bin\\\\((?<DotNetVer>\d.+)\\\\(x\d\d)|(?<Arch>x\d\d)\\\\signtool.exe)";
            string matchesWin8AndLess = @"\\\\(?<Winver>(7|7\.\d|7\.\d\d|8|8\.\d|8\.\d\d))\\\\bin\\\\(?<Arch>x\d\d)\\\\signtool.exe";

            //var blah = System.IO.Directory.GetDirectories(programFilesX86, "10.0.*", SearchOption.AllDirectories);
            string[]? possibleSignTool = Directory.GetFiles(Path.Combine(programFilesX86, "Windows Kits"), "signtool.exe", SearchOption.AllDirectories);

            string signtool = string.Empty;

            List<string> versions = ["10", "8.1", "8.0"];
            List<string> subVersions = [
                "10.0.26100.0",
                "10.0.22621.0",
                "10.0.22000.0",
                "10.0.20348.0",
                "10.0.17134.0",
                "10.0.16299.0",
                "10.0.15063.0",
                "10.0.14393.0",
                ];

            foreach (string version in versions) {
                foreach (string subVersion in subVersions) {
                    signtool = Path.Combine(programFilesX86, "Windows Kits", version, "bin", subVersion, "x64", "signtool.exe");
                    if (File.Exists(signtool)) {
                        return signtool;
                    }
                    else {
                        signtool = Path.Combine(programFilesX86, "Windows Kits", version, "bin", subVersion, "x86", "signtool.exe");
                    }
                    if (File.Exists(signtool)) {
                        return signtool;
                    }
                }
            }
            
            signtool = Path.Combine(programFilesX86, "Windows Kits", "10", "bin", "10.0.26100.0", "x64", "signtool.exe");
            if (File.Exists(signtool)) {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.1", "bin", "x64", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.1", "bin", "x86", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.0", "bin", "x64", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Windows Kits", "8.0", "bin", "x86", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            signtool = Path.Combine(programFilesX86, "Microsoft SDKs", "Windows", "v7.1A", "Bin", "signtool.exe");
            if (File.Exists(signtool))
            {
                return signtool;
            }

            return null;
        }
    }
}
