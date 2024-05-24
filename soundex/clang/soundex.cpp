#include <iostream>
#include <string>
#include <cctype>
#include <stdexcept>
#include <unordered_map>

class Soundex
{
private:
    static constexpr unsigned char NO_VALUE = 255;
    static constexpr int SKIP_PHONETIC = 0;

    // Phonetic map equivalent to the C# phoneticMap array
    static const unsigned char phoneticMap[26];

    static unsigned char getPhonetic(char c)
    {
        return c < 'A' || c > 'Z' ? NO_VALUE : phoneticMap[c - 'A'];
    }

public:
    static std::string of(const std::string &word)
    {
        if (word.empty())
        {
            throw std::invalid_argument("`value` must be a non-empty string");
        }

        std::string result(1, std::toupper(word[0]));
        unsigned char previous = getPhonetic(std::toupper(word[0]));

        for (size_t i = 1; i < word.length(); ++i)
        {
            unsigned char current = getPhonetic(std::toupper(word[i]));
            if (current == SKIP_PHONETIC)
            {
                previous = SKIP_PHONETIC; // skip phonetic value
            }
            else if (current != NO_VALUE && current != previous)
            {
                result += (previous = current) + '0';
                if (result.length() == 4)
                {
                    return result;
                }
            }
        }

        return result.append(4 - result.length(), '0'); // pad with zeros
    }
};

const unsigned char Soundex::phoneticMap[26] = {
    0, 1, 2, 3, 0, 1, 2, 0, 0, 2, 2, 4, 5, 5, 0, 1, 2, 6, 2, 3, 0, 1, 0, 2, 0, 2};

int main()
{
    std::unordered_map<std::string, std::string> testCases = {
        {"TestCaseSensitivity", "T232"},
        {"testcasesensitivitY", "T232"},
        {"Washington", "W252"},
        {"Lee", "L000"},
        {"Gutierrez", "G362"},
        {"Pfister", "P236"},
        {"Jackson", "J250"},
        {"Tymczak", "T522"},
        {"VanDeusen", "V532"},
        {"Deusen", "D250"},
        {"Ashcraft", "A226"},
        {"Euler", "E460"},
        {"Gauss", "G200"},
        {"Hilbert", "H416"},
        {"Knuth", "K530"},
        {"Lloyd", "L300"},
        {"Lukasiewicz", "L222"},
        {"Ellery", "E460"},
        {"Ghosh", "G200"},
        {"Heilbronn", "H416"},
        {"Kant", "K530"},
        {"Ladd", "L300"},
        {"Lissajous", "L222"},
        {"blackberry", "B421"},
        {"calculate", "C424"},
        {"fox", "F200"},
        {"jump", "J510"},
        {"phonetics", "P532"},
        {"Soundex", "S532"},
        {"Example", "E251"},
        {"Sownteks", "S532"},
        {"Ekzampul", "E251"},
        {"Wheaton", "W350"},
        {"Burroughs", "B622"},
        {"Burrows", "B620"},
        {"O'Hara", "O600"}};

    for (const auto &[word, expectedCode] : testCases)
    {
        std::string code = Soundex::of(word);
        std::cout << "Soundex code for '" << word << "' is '" << code << "'. Expected: '" << expectedCode << "'" << std::endl;
        if (code == expectedCode)
        {
            std::cout << "Test passed!" << std::endl;
        }
        else
        {
            std::cout << "Test failed!" << std::endl;
        }
    }

    return 0;
}
