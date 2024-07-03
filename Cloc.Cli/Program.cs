using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cloc.Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(@"       _                         _   " + Environment.NewLine +
                                  @"   ___| | ___   ___   _ __   ___| |_ " + Environment.NewLine +
                                  @"  / __| |/ _ \ / __| | '_ \ / _ \ __|" + Environment.NewLine +
                                  @" | (__| | (_) | (__ _| | | |  __/ |_ " + Environment.NewLine +
                                  @"  \___|_|\___/ \___(_)_| |_|\___|\__|");
                Console.WriteLine();
                Console.WriteLine("Syntax:       cloc-net.exe [directory] [extensions]");
                Console.WriteLine();
                Console.WriteLine("Description:  Counts the lines of code files in the specified directory and ");
                Console.WriteLine("              its sub-directories.");
                Console.WriteLine();
                Console.WriteLine("Arguments:");
                Console.WriteLine();
                Console.WriteLine("  directory   The directory contaning the files to count.");
                Console.WriteLine();
                Console.WriteLine("  extensions  A comma-separated list of additional file extensions to include.");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine();
                Console.WriteLine("\tcloc-net C:\\source\\project");
                Console.WriteLine("\tcloc-net C:\\source\\project .xxx,.yyy");
                Console.WriteLine();
            }
            else if (args.Length >= 1)
            {
                var directory = args[0];
                var extensions = args.Length > 1 ? args[1].Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<String>();

                var validatedExtensions = new List<String>();
                foreach (var extension in extensions)
                {
                    if (!validatedExtensions.Contains(extension.Trim()) && Regex.IsMatch(extension.Trim(), @"\.\w{1,6}"))
                    {
                        validatedExtensions.Add(extension.Trim());
                    }
                }

                if (Directory.Exists(directory))
                {
                    var tempPath = Path.GetTempPath();

                    if (!System.IO.File.Exists(Path.Combine(tempPath, "vcruntime140.dll")))
                    {
                        // Redistributable according to https://learn.microsoft.com/en-us/visualstudio/releases/2022/redistribution#visual-c-runtime-files
                        using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@"Cloc.Cli.vcruntime140.dll"))
                        {
                            var bytes = new Byte[(Int32)stream.Length];
                            stream.Read(bytes, 0, bytes.Length);
                            System.IO.File.WriteAllBytes(Path.Combine(tempPath, "vcruntime140.dll"), bytes);
                        }
                    }

                    directory = Path.GetDirectoryName(directory);
                    var countedFiles = new List<Cloc.File>();

                    var allFiles = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
                    var planFiles = new List<String>();

                    using (var checker = new Checker())
                    {
                        var clocFiles = checker.Count(directory);

                        foreach (var file in allFiles)
                        {
                            File clocFile;

                            if ((clocFile = clocFiles.FirstOrDefault(f => f.Path == file)) != null)
                            {
                                countedFiles.Add(clocFile);
                            }
                            else
                            {
                                if (validatedExtensions.Any(e => file.EndsWith(e)))
                                {
                                    var line = String.Empty;
                                    var codeOrComments = 0;
                                    var blanks = 0;

                                    using (var reader = new StreamReader(file))
                                    {
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            if (!String.IsNullOrWhiteSpace(line))
                                            {
                                                codeOrComments++;
                                            }
                                            else
                                            {
                                                blanks++;
                                            }
                                        }
                                    }

                                    countedFiles.Add(new Cloc.File(Path.GetExtension(file).Replace(".", "").ToUpperInvariant(), file, blanks, codeOrComments, 0));
                                }
                            }
                        }
                    }

                    Console.WriteLine($"===============================================================================");
                    Console.WriteLine($" Language               Files       Lines        Code    Comments      Blanks");
                    Console.WriteLine($"===============================================================================");

                    foreach (var language in countedFiles.DistinctBy(f => f.Language).OrderBy(f => f.Language).Select(f => f.Language))
                    {
                        var languageFiles = countedFiles.Where(f => f.Language == language);
                        var normalizedLanguage = language.Length <= 16 ? language : language.Substring(0, 15) + "*";

                        Console.WriteLine($" {normalizedLanguage,16}{languageFiles.Count(),12}{languageFiles.Sum(f => f.Blank + f.Code + f.Comment),12}{languageFiles.Sum(f => f.Code),12}{languageFiles.Sum(f => f.Comment),12}{languageFiles.Sum(f => f.Blank),12}");
                    }

                    Console.WriteLine($"-------------------------------------------------------------------------------");
                    Console.WriteLine($" {"Total",16}{countedFiles.Count(),12}{countedFiles.Sum(f => f.Blank + f.Code + f.Comment),12}{countedFiles.Sum(f => f.Code),12}{countedFiles.Sum(f => f.Comment),12}{countedFiles.Sum(f => f.Blank),12}");
                    Console.WriteLine($"===============================================================================");
                }
                else
                {
                    Console.Error.WriteLine("The specified directory does not exist.");
                }
            }
        }
    }
}
