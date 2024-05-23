public class Soundex {
    private static final byte NO_VALUE = (byte) 255;
    private static final int SKIP_PHONETIC = 0;

    /**
     * Returns the Soundex code for the given word.
     *
     * @param word The word to get the Soundex code for.
     * @return The Soundex code for the given word.
     * @throws IllegalArgumentException If the given word is empty.
     */
    public static String of(String word) {
        if (word == null || word.isEmpty()) {
            throw new IllegalArgumentException("`value` must be a non-empty string");
        }

        var result = new StringBuilder(4).append(Character.toUpperCase(word.charAt(0)));
        var previous = getPhonetic(word.charAt(0));

        for (var i = 1; i < word.length(); i++) {
            var current = getPhonetic(word.charAt(i));
            if (current == SKIP_PHONETIC) {
                previous = SKIP_PHONETIC; // skip phonetic value
            } else if (current != NO_VALUE && current != previous) {
                result.append(previous = current);
                if (result.length() == 4) {
                    return result.toString();
                }
            }
        }

        while (result.length() < 4) {
            result.append('0'); // pad with zeros
        }

        return result.toString();
    }

    private static byte getPhonetic(char c) {
        c = Character.toUpperCase(c);
        return (c < 'A' || c > 'Z') ? NO_VALUE : phoneticMap[c - 'A'];
    }

    private static final byte[] phoneticMap = {
            0, 1, 2, 3, 0, 1, 2, 0, 0, 2, 2, 4, 5, 5, 0, 1, 2, 6, 2, 3, 0, 1, 0, 2, 0, 2
    };
}
