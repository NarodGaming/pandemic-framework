using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AlkalineThunder.Pandemic.CommandLine
{
    public static class ShellUtils
    {
        private static string[] _elfStrings = new[]
        {
            "AWAVAUATI",
            "ATUSH",
            "AUATUSH",
        };
        
        private static string[] _cFunctions = new string[]
        {
            "printf",
            "write",
            "memcpy",
            "memset",
            "malloc",
            "memmove",
            "abort",
            "asctime",
            "fopen",
            "gets",
            "catgets",
            "catopen",
            "clock",
            "clearerr",
            "fabs",
            "fclose",
            "fdopen",
            "fgetc"
        };
        
        public const char EscapeChar = '\\';
        public const char QuoteChar = '"';
        public const char PathSeparatorChar = '/';
        public const char HomeChar = '~';

        private static void ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new FormatException();
            
            if (!path.StartsWith(PathSeparatorChar))
                throw new FormatException();
        }

        public static string JoinPath(params string[] parts)
        {
            var path = "/";

            foreach (var part in parts)
            {
                var partMod = part;
                
                while (partMod.StartsWith(PathSeparatorChar))
                    partMod = partMod.Remove(0, 1);

                while (partMod.EndsWith(PathSeparatorChar))
                    partMod = partMod.Remove(part.Length - 1, 1);

                if (string.IsNullOrWhiteSpace(partMod))
                    continue;

                if (!path.EndsWith(PathSeparatorChar))
                    path += PathSeparatorChar;

                path += partMod;
            }
            
            return path;
        }

        public static string[] MakeAbsolutePath(string[] path)
        {
            var partList = new List<string>();

            foreach (var part in path)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                if (part == ".")
                    continue;

                if (part == ".." && partList.Count > 0)
                {
                    partList.RemoveAt(partList.Count - 1);
                    continue;
                }

                partList.Add(part);

            }

            return partList.ToArray();
        }
        
        public static bool TryTokenize(string command, out string[] tokens)
        {
            try
            {
                tokens = Tokenize(command);
                return true;
            }
            catch
            {
                tokens = null;
                return false;
            }
        }

        
        public static string[] GetPathParts(string path)
        {
            ValidatePath(path);
            
            var partList = new List<string>();
            var current = "";
            
            foreach (var c in path)
            {
                if (c == PathSeparatorChar)
                {
                    if (!string.IsNullOrWhiteSpace(current))
                        partList.Add(current);
                    current = "";
                    continue;
                }

                current += c;
            }

            if (!string.IsNullOrWhiteSpace(current))
                partList.Add(current);
            
            return partList.ToArray();
        }
        
        public static string[] Tokenize(string text)
        {
            var tokens = new List<string>();
            var word = "";
            var escaping = false;
            var inQuote = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == EscapeChar)
                {
                    if (escaping)
                    {
                        escaping = false;
                        word += c;
                    }
                    else
                    {
                        escaping = true;
                    }

                    continue;
                }

                if (escaping)
                {
                    switch (c)
                    {
                        case 'r':
                            word += '\r';
                            break;
                        case 't':
                            word += '\t';
                            break;
                        case 'n':
                            word += '\n';
                            break;
                        default:
                            word += c;
                            break;
                    }
                    
                    escaping = false;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuote)
                {
                    if (!string.IsNullOrEmpty(word))
                    {
                        tokens.Add(word);
                        word = "";
                    }

                    continue;
                }

                if (c == QuoteChar)
                {
                    inQuote = !inQuote;
                    if (!inQuote)
                    {
                        if (i + 1 < text.Length)
                        {
                            if (text[i + 1] == QuoteChar)
                                throw new ShellException($"[<string>:{i + 1}]: unexpected quote");
                        }
                    }

                    continue;
                }

                word += c;
            }

            if (escaping)
                throw new ShellException(
                    $"[<string>:{text.Length}]: unexpected end of string, expected escape sequence");
            
            if (inQuote)
                throw new ShellException(
                    $"[<string>:{text.Length}]: unexpected end of string, expected end of quote");

            if (!string.IsNullOrEmpty(word))
            {
                tokens.Add(word);
                word = "";
            }
            
            return tokens.ToArray();
        }

        public static byte[] GenerateConvincingElfData(string typeid)
        {
            using var md5 = MD5.Create();
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8);

            var seed = md5.ComputeHash(Encoding.UTF8.GetBytes(typeid)).Sum(x => x);
            var rng = new Random(seed);

            writer.Write($"\x7F" + "ELF");

            var randomDataCount = rng.Next(300, 999);
            for (var i = 0; i < randomDataCount; i++)
            {
                writer.Write((byte) rng.Next(0, 255));
            }

            var cFunctionCount = rng.Next(10, 99);
            for (var i = 0; i < cFunctionCount; i++)
            {
                writer.Write(_cFunctions[rng.Next(0, _cFunctions.Length - 1)]);
                writer.Write((byte) 0);
            }

            var moreRandomBytes = rng.Next(1000, 9999);

            for (var i = 0; i < moreRandomBytes; i++)
            {
                writer.Write((byte) rng.Next(0, 255));
                if (i % 42 == 0)
                {
                    writer.Write(_elfStrings[rng.Next(0, _elfStrings.Length - 1)]);
                }
            }

            var sectionNames = new[] {"text", "data", "dynstr", "got", "bss", "rodata"};
            foreach (var name in sectionNames)
            {
                writer.Write("." + name);
                writer.Write(0);
            }
            return ms.ToArray();
        }

        public static bool GetHereDoc(ref string[] tokens, out string path, out ShellHereDocumentMode mode)
        {
            mode = ShellHereDocumentMode.None;
            path = null;

            var inToken = "<";
            var outToken = ">";
            var appendToken = ">>";

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token == inToken)
                {
                    if (i + 1 >= tokens.Length)
                        throw new ShellException("bash: expected file name after '" + token + "', got nothing.");

                    mode = ShellHereDocumentMode.Input;
                    path = string.Join(" ", tokens.Skip(i + 1).ToArray());
                    tokens = tokens.Take(i).ToArray();
                    return true;
                }
                else if (token == outToken)
                {
                    if (i + 1 >= tokens.Length)
                        throw new ShellException("bash: expected file name after '" + token + "', got nothing.");

                    mode = ShellHereDocumentMode.Output;
                    path = string.Join(" ", tokens.Skip(i + 1).ToArray());
                    tokens = tokens.Take(i).ToArray();
                    return true;
                }
                else if (token == appendToken)
                {
                    if (i + 1 >= tokens.Length)
                        throw new ShellException("bash: expected file name after '" + token + "', got nothing.");

                    mode = ShellHereDocumentMode.Append;
                    path = string.Join(" ", tokens.Skip(i + 1).ToArray());
                    tokens = tokens.Take(i).ToArray();
                    return true;
                }
            }
            
            return false;
        }

        public static bool ProcessUnixPipes(string[] tokens, StreamReader input, StreamWriter output,
            out PipeInformation[] info)
        {
            var pipeToken = "|";
            var ret = new List<PipeInformation>();

            if (tokens.Length < 1)
            {
                info = null;
                return false;
            }

            var lastPipe = -1;

            var lastStream = input.BaseStream;

            for (var i = 0; i <= tokens.Length; i++)
            {
                var token = (i >= tokens.Length) ? pipeToken : tokens[i];

                if (token == pipeToken)
                {
                    var commandTokens = tokens.Skip(lastPipe + 1).Take((i - lastPipe) - 1).ToArray();
                    var r = new StreamReader(lastStream);
                    var w = StreamWriter.Null;

                    if (i >= tokens.Length)
                    {
                        w = output;
                    }
                    else
                    {
                        var ms = new MemoryStream();
                        w = new StreamWriter(ms);
                        w.AutoFlush = true;
                        lastStream = ms;
                    }

                    ret.Add(new PipeInformation(commandTokens, r, w));
                    lastPipe = i;
                }
            }

            info = ret.ToArray();
            return true;
        }
        
        
    }
}