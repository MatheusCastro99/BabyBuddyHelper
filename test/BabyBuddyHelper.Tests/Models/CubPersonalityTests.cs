using BabyBuddyHelper.Core.Models;

namespace BabyBuddyHelper.Tests.Models
{
    //Reading the saved style and reply length back (#97). The Preferences store itself needs MAUI, so its fallback lives here.
    public class CubPersonalityTests
    {
        [Theory]
        [InlineData("Supportive", "Short", CubStyle.Supportive, CubVerbosity.Short)]
        [InlineData("Neutral", "Balanced", CubStyle.Neutral, CubVerbosity.Balanced)]
        [InlineData("Direct", "Detailed", CubStyle.Direct, CubVerbosity.Detailed)]
        public void FromSaved_KnownNames_ReadsThem(string style, string verbosity, CubStyle expectedStyle, CubVerbosity expectedVerbosity)
        {
            Assert.Equal(new CubPersonality(expectedStyle, expectedVerbosity), CubPersonality.FromSaved(style, verbosity));
        }

        //What Save writes, Load reads back
        [Fact]
        public void FromSaved_EveryPairSavedByName_RoundTrips()
        {
            foreach (CubStyle style in Enum.GetValues<CubStyle>())
            {
                foreach (CubVerbosity verbosity in Enum.GetValues<CubVerbosity>())
                {
                    Assert.Equal(new CubPersonality(style, verbosity), CubPersonality.FromSaved(style.ToString(), verbosity.ToString()));
                }
            }
        }

        //First run, or a name from another version: each half falls back on its own
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("Playful", "Chatty")]
        [InlineData("supportive", "balanced")]
        [InlineData("7", "-1")]
        public void FromSaved_MissingOrUnknown_IsTheDefault(string? style, string? verbosity)
        {
            Assert.Equal(CubPersonality.Default, CubPersonality.FromSaved(style, verbosity));
        }

        [Fact]
        public void FromSaved_OneHalfUnknown_KeepsTheOther()
        {
            Assert.Equal(new CubPersonality(CubStyle.Direct, CubVerbosity.Balanced), CubPersonality.FromSaved("Direct", "Chatty"));
            Assert.Equal(new CubPersonality(CubStyle.Supportive, CubVerbosity.Short), CubPersonality.FromSaved(null, "Short"));
        }
    }
}
