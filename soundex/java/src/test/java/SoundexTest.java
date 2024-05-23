import java.util.HashMap;
import java.util.Map;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class SoundexTest {
    @Test
    void of() {
        for (var entry : testCases.entrySet()) {
            assertEquals(entry.getValue(), Soundex.of(entry.getKey()));
        }
    }

    private static final Map<String, String> testCases = new HashMap<>();

    static {
        testCases.put("TestCaseSensitivity", "T232");
        testCases.put("testcasesensitivitY", "T232");
        testCases.put("Washington", "W252");
        testCases.put("Lee", "L000");
        testCases.put("Gutierrez", "G362");
        testCases.put("Pfister", "P236");
        testCases.put("Jackson", "J250");
        testCases.put("Tymczak", "T522");
        testCases.put("VanDeusen", "V532");
        testCases.put("Deusen", "D250");
        testCases.put("Ashcraft", "A226");
        testCases.put("Euler", "E460");
        testCases.put("Gauss", "G200");
        testCases.put("Hilbert", "H416");
        testCases.put("Knuth", "K530");
        testCases.put("Lloyd", "L300");
        testCases.put("Lukasiewicz", "L222");
        testCases.put("Ellery", "E460");
        testCases.put("Ghosh", "G200");
        testCases.put("Heilbronn", "H416");
        testCases.put("Kant", "K530");
        testCases.put("Ladd", "L300");
        testCases.put("Lissajous", "L222");
        testCases.put("blackberry", "B421");
        testCases.put("calculate", "C424");
        testCases.put("fox", "F200");
        testCases.put("jump", "J510");
        testCases.put("phonetics", "P532");
        testCases.put("Soundex", "S532");
        testCases.put("Example", "E251");
        testCases.put("Sownteks", "S532");
        testCases.put("Ekzampul", "E251");
        testCases.put("Wheaton", "W350");
        testCases.put("Burroughs", "B622");
        testCases.put("Burrows", "B620");
        testCases.put("O'Hara", "O600");
    }
}