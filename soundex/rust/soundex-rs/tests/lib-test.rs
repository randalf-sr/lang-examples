#[cfg(test)]
mod tests {
    use std::collections::HashMap;

    use soundex_rs::soundex;

    #[test]
    fn soundex_tests() {
        let test_cases: HashMap<&str, &str> = [
            ("TestCaseSensitivity", "T232"),
            ("testcasesensitivitY", "T232"),
            ("Washington", "W252"),
            ("Lee", "L000"),
            ("Gutierrez", "G362"),
            ("Pfister", "P236"),
            ("Jackson", "J250"),
            ("Tymczak", "T522"),
            ("VanDeusen", "V532"),
            ("Deusen", "D250"),
            ("Ashcraft", "A226"),
            ("Euler", "E460"),
            ("Gauss", "G200"),
            ("Hilbert", "H416"),
            ("Knuth", "K530"),
            ("Lloyd", "L300"),
            ("Lukasiewicz", "L222"),
            ("Ellery", "E460"),
            ("Ghosh", "G200"),
            ("Heilbronn", "H416"),
            ("Kant", "K530"),
            ("Ladd", "L300"),
            ("Lissajous", "L222"),
            ("blackberry", "B421"),
            ("calculate", "C424"),
            ("fox", "F200"),
            ("jump", "J510"),
            ("phonetics", "P532"),
            ("Soundex", "S532"),
            ("Example", "E251"),
            ("Sownteks", "S532"),
            ("Ekzampul", "E251"),
            ("Wheaton", "W350"),
            ("Burroughs", "B622"),
            ("Burrows", "B620"),
            ("O'Hara", "O600"),
        ]
        .iter()
        .cloned()
        .collect();

        for (key, expected) in &test_cases {
            let result = soundex(key);
            assert_eq!(result, *expected, "Test case failed for input: {}", key);
        }
    }
}
