using System.Text;

public static class Soundex
{
    private const byte NO_VALUE = 255;
    private const int SKIP_PHONETIC = 0;

    /**
     * Returns the Soundex code for the given word.
     * 
     * @param word The word to get the Soundex code for.
     * @return The Soundex code for the given word.
     * @throws ArgumentException If the given word is empty.
     */
    public static string Of(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            throw new ArgumentException("`value` must be a non-empty string");
        }

        var result = new StringBuilder(4).Append(char.ToUpper(word[0]));
        var previous = getPhonetic(word[0]);

        for (var i = 1; i < word.Length; i++)
        {
            var current = getPhonetic(word[i]);
            if (current == SKIP_PHONETIC)
            {
                previous = SKIP_PHONETIC; // skip phonetic value
            }
            else if (current != NO_VALUE && current != previous)
            {
                result.Append(previous = current);
                if (result.Length == 4)
                {
                    return result.ToString();
                }
            }
        }

        return result.Append('0', 4 - result.Length).ToString(); // pad with zeros
    }

    private static readonly Func<char, byte> getPhonetic;

    static Soundex()
    {
        var phoneticMap = new byte[26]
        {
            0, 1, 2, 3, 0, 1, 2, 0, 0, 2, 2, 4, 5, 5, 0, 1, 2, 6, 2, 3, 0, 1, 0, 2, 0, 2
        };

        getPhonetic = c =>
        {
            c = char.ToUpper(c);
            return c is < 'A' or > 'Z' ? NO_VALUE : phoneticMap[c - 'A'];
        };
    }
}
