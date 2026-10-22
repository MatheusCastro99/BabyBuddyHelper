namespace BabyBuddyHelper.Tests.Models
{
    // V0 (#123): a deliberately failing test, so the required "test" checks fail and the ruleset must block the merge. Never merged.
    public class V0FailingCheckTests
    {
        [Fact]
        public void V0_DeliberatelyFails() => Assert.Fail("V0: deliberately failing required check (#123)");
    }
}
