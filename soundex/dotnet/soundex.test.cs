public class SoundExTest
{
    [Fact]
    public void TestSoundex()
    {
        foreach (var testCase in __testCases)
        {
            Assert.Equal(testCase.Value, Soundex.Of(testCase.Key));
        }
    }

    private static readonly Dictionary<string, string> __testCases = new() {
        { "TestCaseSensitivity", "T232" },
        { "testcasesensitivitY", "T232" },
        { "Washington", "W252" },
        { "Lee", "L000" },
        { "Gutierrez", "G362" },
        { "Pfister", "P236" },
        { "Jackson", "J250" },
        { "Tymczak", "T522" },
        { "VanDeusen", "V532" },
        { "Deusen", "D250" },
        { "Ashcraft", "A226" },
        { "Euler", "E460" },
        { "Gauss", "G200" },
        { "Hilbert", "H416" },
        { "Knuth", "K530" },
        { "Lloyd", "L300" },
        { "Lukasiewicz", "L222" },
        { "Ellery", "E460" },
        { "Ghosh", "G200" },
        { "Heilbronn", "H416" },
        { "Kant", "K530" },
        { "Ladd", "L300" },
        { "Lissajous", "L222" },
        { "blackberry", "B421" },
        { "calculate", "C424" },
        { "fox", "F200" },
        { "jump", "J510" },
        { "phonetics", "P532" },
        { "Soundex", "S532" },
        { "Example", "E251" },
        { "Sownteks", "S532" },
        { "Ekzampul", "E251" },
        { "Wheaton", "W350" },
        { "Burroughs", "B622" },
        { "Burrows", "B620" },
        { "O'Hara", "O600" }
    };
}

