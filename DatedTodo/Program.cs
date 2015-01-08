using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace DatedTodo
{
    public static class Program
    {
        private static Regex Regex = new Regex(@"^\s*TODO DUE (?<date>\d{4}-\d{2}-\d{2}):*\s*(?<text>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static TimeSpan WarningSpan = TimeSpan.FromDays(-5);

        public static int Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            MainAsync(args).Wait();

            sw.Stop();
            
            Console.WriteLine("DatedTodo took " + sw.ElapsedMilliseconds + "ms");
            return 0;
        }

        public static async Task MainAsync(string[] args)
        {
            var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(args.First());

            foreach (var document in project.Documents)
            {
                var commentTrivias = await GetSingleLineCommentTrivias(document);
                foreach (var commentTrivia in commentTrivias)
                {
                    var comment = commentTrivia.ToString().Substring(2);
                    var match = Regex.Match(comment);
                    if (match.Success)
                    {
                        var dueDate = DateTime.ParseExact(match.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        var text = match.Groups["text"].Value;

                        if (dueDate <= DateTime.Today)
                        {
                            Error(commentTrivia, dueDate, text);
                        }
                        else if (dueDate.Add(WarningSpan) <= DateTime.Today)
                        {
                            Warning(commentTrivia, dueDate, text);
                        }
                    }
                }
            }
        }

        private static async Task<IEnumerable<SyntaxTrivia>> GetSingleLineCommentTrivias(Document document)
        {
            var root = await document.GetSyntaxRootAsync();
            var result = root.DescendantTrivia().Where(x => x.CSharpKind() == SyntaxKind.SingleLineCommentTrivia);
            return result;
        }

        private static LinePosition GetStartLinePosition(SyntaxTrivia trivia)
        {
            return trivia.SyntaxTree.GetLineSpan(trivia.Span).Span.Start;
        }

        private static void Error(SyntaxTrivia trivia, DateTime dueDate, string message)
        {
            var position = GetStartLinePosition(trivia);
            Console.WriteLine(
                "{0}({1},{2}): error: Todo due {3:yyyy-MM-dd}: {4}",
                trivia.SyntaxTree.FilePath,
                position.Line + 1,
                position.Character + 1,
                dueDate,
                message);
        }

        private static void Warning(SyntaxTrivia trivia, DateTime dueDate, string message)
        {
            var position = GetStartLinePosition(trivia);
            Console.WriteLine(
                "{0}({1},{2}): warning: Todo due {3:yyyy-MM-dd}: {4}",
                trivia.SyntaxTree.FilePath,
                position.Line + 1,
                position.Character + 1,
                dueDate,
                message);
        }
    }
}
