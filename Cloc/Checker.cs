using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Cloc
{
    public class Checker : IDisposable
    {
        private String _tempFileName = String.Empty;
        
        private Boolean _disposed = false;
        
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        public Checker()
        {
            _tempFileName = Path.GetTempFileName();

            using (var stream = GetType().Assembly.GetManifestResourceStream(@"Cloc.tokei-x86_64-pc-windows-msvc.exe"))
            {
                var bytes = new Byte[(Int32)stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                System.IO.File.WriteAllBytes(_tempFileName, bytes);
            }
        }

        public void Dispose() => Dispose(true);

        public void Dispose(Boolean disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    System.IO.File.Delete(_tempFileName);
                }
                catch
                { }

                _safeHandle?.Dispose();
            }

            _disposed = true;
        }
        
        public List<File> Count(String path)
        {
            return Count(new String[] { path }, null);
        }

        public List<File> Count(String path, String exclude)
        {
            return Count(new String[] { path }, exclude);
        }

        public List<File> Count(String[] paths)
        {
            return Count(paths, null);
        }

        public List<File> Count(String[] paths, String exclude)
        {
            var files = new List<File>();

            var pathsExist = true;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    pathsExist = false;
                    break;
                }
            }

            if (pathsExist)
            {
                var standardOutput = new StringBuilder();
                var standardError = new StringBuilder();

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo(_tempFileName, "\"" + String.Join("\" \"", paths) + "\"" + (String.IsNullOrEmpty(exclude) ? String.Empty : " --exclude \"" + exclude + "\"") + " --no-ignore --hidden --output json");
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                standardOutput.AppendLine(e.Data);
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                standardError.AppendLine(e.Data);
                            }
                        };

                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.WaitForExit();
                    }
                }

                using (var doc = JsonDocument.Parse(standardOutput.ToString()))
                {
                    foreach (var languageObject in doc.RootElement.EnumerateObject())
                    {
                        var language = languageObject.Name;

                        foreach (var reportObject in languageObject.Value.GetProperty("reports").EnumerateArray())
                        {
                            try
                            {
                                var name = reportObject.GetProperty("name").GetString();
                                var blanks = Convert.ToInt32(reportObject.GetProperty("stats").GetProperty("blanks").GetInt32());
                                var code = Convert.ToInt32(reportObject.GetProperty("stats").GetProperty("code").GetInt32());
                                var comments = Convert.ToInt32(reportObject.GetProperty("stats").GetProperty("comments").GetInt32());

                                files.Add(new File(language, name, blanks, code, comments));
                            }
                            catch { }
                        }
                    }
                }
            }

            return files;
        }
    }
}
