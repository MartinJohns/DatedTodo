using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DatedTodo
{
    public static class Program
    {
        private static Regex Regex = new Regex(@"//\s*TODO DUE (?<date>\d{4}-\d{2}-\d{2}):?\s*(?<text>.*)$", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static TimeSpan WarningSpan = TimeSpan.FromDays(-5);

        public static void Main(string[] args)
        {
            var projectPath = args.First();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var project = XDocument.Load(projectPath);
            var csharpFiles = GetCSharpFiles(project);
            foreach (var file in csharpFiles)
            {
                var filePath = Path.Combine(Path.GetDirectoryName(projectPath), file);
                var content = File.ReadAllText(filePath);
                var matches = Regex.Matches(content);
                foreach (Match match in matches)
                {
                    var dueDate = DateTime.ParseExact(match.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var text = match.Groups["text"].Value.Trim();

                    if (dueDate <= DateTime.Today)
                    {
                        Error(filePath, GetTextPosition(content, match.Index), dueDate, text);
                    }
                    else if (dueDate.Add(WarningSpan) <= DateTime.Today)
                    {
                        Warning(filePath, GetTextPosition(content, match.Index), dueDate, text);
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine("DatedTodo took " + stopwatch.ElapsedMilliseconds + "ms");
        }

        private static IEnumerable<string> GetCSharpFiles(XDocument doc)
        {
            return
                doc.Root
                    .DescendantNodes()
                    .OfType<XElement>()
                    .Where(x => x.Name == "{http://schemas.microsoft.com/developer/msbuild/2003}Compile")
                    .Select(x => x.Attribute("Include"))
                    .Where(x => x != null)
                    .Select(x => x.Value);
        }

        private static TextPosition GetTextPosition(string str, int pos)
        {
            var line = 1;
            var column = 0;
            for (var i = 0; i <= pos; i++)
            {
                column++;

                if (str[i] == '\n')
                {
                    line++;
                    column = 0;
                }
            }

            return new TextPosition(line, column);
        }

        private static void Error(string filePath, TextPosition pos, DateTime dueDate, string message)
        {
            Console.WriteLine(
                "{0}({1},{2}): error: Todo due {3:yyyy-MM-dd}: {4}",
                filePath,
                pos.Line,
                pos.Column,
                dueDate,
                message);
        }
        private static void Warning(string filePath, TextPosition pos, DateTime dueDate, string message)
        {
            Console.WriteLine(
                "{0}({1},{2}): warning: Todo due {3:yyyy-MM-dd}: {4}",
                filePath,
                pos.Line,
                pos.Column,
                dueDate,
                message);
        }
    }
}
