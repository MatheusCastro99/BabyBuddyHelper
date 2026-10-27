using BabyBuddyHelper.Core.Models.Cub;
using BabyBuddyHelper.Core.Services.Cub;

namespace BabyBuddyHelper.Tests.Services
{
    //The code checks every reply passes before the grader sees it (#97, ADR-020).
    public class CubReplyChecksTests
    {
        [Fact]
        public void Check_PlainWholeReply_FindsNothing()
        {
            Assert.Null(CubReplyChecks.Check(new CubReply("Naps vary a lot at this age. Watch for sleepy cues.", CutOff: false)));
        }

        [Fact]
        public void Check_EmptyReply_IsEmpty()
        {
            Assert.Equal(CubReplyProblem.Empty, CubReplyChecks.Check(new CubReply(string.Empty, CutOff: false)));
            Assert.Equal(CubReplyProblem.Empty, CubReplyChecks.Check(new CubReply(string.Empty, CutOff: true)));
        }

        [Fact]
        public void Check_ReplyStoppedAtTheCap_IsCutOff()
        {
            Assert.Equal(CubReplyProblem.CutOff, CubReplyChecks.Check(new CubReply("Naps vary a lot at", CutOff: true)));
        }

        [Theory]
        [InlineData("Try **one** nap.")]
        [InlineData("Here:\n```\nnap\n```")]
        [InlineData("# Naps\nThey vary.")]
        [InlineData("They vary.\n## Tips\nWatch cues.")]
        [InlineData("They vary.\r\n   ### Tips\r\nWatch cues.")]
        [InlineData("They vary.\n#")]
        public void Check_ReplyWithMarkdown_IsMarkdown(string text)
        {
            Assert.Equal(CubReplyProblem.Markdown, CubReplyChecks.Check(new CubReply(text, CutOff: false)));
        }

        //Plain text that merely holds a "#" or a single "*" is not formatting
        [Theory]
        [InlineData("#1 tip: keep a routine.")]
        [InlineData("Your baby is #1 to you.")]
        [InlineData("Use a 5*5 cm swatch.")]
        [InlineData("Some parents use #hashtags.")]
        [InlineData("    # indented four spaces is code, not a heading")]
        public void Check_PlainTextWithSymbols_FindsNothing(string text)
        {
            Assert.Null(CubReplyChecks.Check(new CubReply(text, CutOff: false)));
        }

        [Theory]
        [InlineData("PASS", true)]
        [InlineData("PASS - follows every rule", true)]
        [InlineData("FAIL: too long", false)]
        [InlineData("pass", false)]
        [InlineData("**PASS**", false)]
        [InlineData("I think this is a PASS", false)]
        [InlineData("", false)]
        public void IsPassingGrade_OnlyALeadingPass(string grade, bool passes)
        {
            Assert.Equal(passes, CubReplyChecks.IsPassingGrade(grade));
        }
    }
}
