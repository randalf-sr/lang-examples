pub fn soundex(word: &str) -> String {
    assert!(!word.is_empty(), "`value` must be a non-empty string");

    let mut result = String::new();
    let first_char = word.chars().next().unwrap().to_ascii_uppercase();
    result.push(first_char);

    let mut previous = get_phonetic(first_char);
    for c in word.chars().skip(1) {
        let phonetic = get_phonetic(c);
        if phonetic == SKIP_PHONETIC {
            previous = SKIP_PHONETIC;
        } else if phonetic != None && phonetic != previous {
            result.push_str(&phonetic.unwrap().to_string());
            previous = phonetic;
            if result.len() == 4 {
                return result;
            }
        }
    }

    return format!("{:0<4}", result);
}

const SKIP_PHONETIC: Option<char> = Some('0');

fn get_phonetic(c: char) -> Option<char> {
    const M: [char; 26] = [
        '0', '1', '2', '3', '0', '1', '2', '0', '0', '2', '2', '4', '5', '5', '0', '1', '2', '6',
        '2', '3', '0', '1', '0', '2', '0', '2',
    ];

    if !c.is_ascii_alphabetic() {
        return None;
    }

    let index: u8 = (c.to_ascii_uppercase() as u8) - 65;
    return Some(M[index as usize]);
}
