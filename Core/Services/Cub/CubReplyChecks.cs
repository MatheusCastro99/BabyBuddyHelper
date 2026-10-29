using BabyBuddyHelper.Core.Models.Cub;
using System.Text.RegularExpressions;

namespace BabyBuddyHelper.Core.Services.Cub
{
    //Why a reply of Cub's was not shown (#97, ADR-020). Each one but Empty has its own fixed retry note in CubGuidelines.
    public enum CubReplyProblem
    {
        Empty,
        CutOff,
        Markdown,
        FailedGrade
    }

    //The code checks every reply passes before the grader sees it: they catch for free what the grader would pay to find.
    public static partial class CubReplyChecks
    {
        //The first problem found, or null when the reply may go to the grader
        public static CubReplyProblem? Check(CubReply reply)
        {
            if (reply.Text.Length == 0)
                return CubReplyProblem.Empty;

            if (reply.CutOff)
                return CubReplyProblem.CutOff;

            if (HasMarkdown(reply.Text))
                return CubReplyProblem.Markdown;

            return null;
        }

        //Bold, a code fence, or a heading at the start of a line. A lone "#" (such as "#1") is not a heading
        public static bool HasMarkdown(string text) =>
            text.Contains("**", StringComparison.Ordinal) || text.Contains("```", StringComparison.Ordinal) || Heading().IsMatch(text);

        //The grade passes only when its first word is PASS, in any case, ignoring "*", "." or ":" around it (so "**PASS**" and "Pass."
        //pass). Anything else, an unreadable grade included, is a fail (#155)
        public static bool IsPassingGrade(string grade)
        {
            ReadOnlySpan<char> text = grade.AsSpan().TrimStart();
            int end = 0;

            while (end < text.Length && !char.IsWhiteSpace(text[end]))
                end++;

            return text[..end].Trim("*.:").Equals(CubGuidelines.PassingGrade, StringComparison.OrdinalIgnoreCase);
        }

        [GeneratedRegex(@"^[ \t]{0,3}#{1,6}(?:[ \t]|\r?$)", RegexOptions.Multiline)]
        private static partial Regex Heading();
    }
}
