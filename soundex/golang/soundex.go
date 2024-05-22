package main

import "strings"

/**
 * Soundex algorithm implementation in Go.
 * NOTE: will panic if the given word is an empty string.
 *
 * @param {string} word - The word to convert to Soundex.
 * @returns {string} The Soundex representation of the word.
 */
func Soundex(word string) string {
	assert(len(word) > 0, "`value` must be a non-empty string")

	result := strings.ToUpper(string(word[0])) // first letter is always included
	previous := getPhonetic(word[0])           // phonetic value of the first letter

	for i := 1; i < len(word); i++ {
		c := getPhonetic(word[i])
		if c == SKIP_PHONETIC {
			previous = SKIP_PHONETIC
		} else if c != NO_VALUE && c != previous {
			result += string(c)
			if len(result) == 4 {
				return result
			}
			previous = c
		}
	}

	return (result + "000")[:4]
}

const NO_VALUE = 255
const SKIP_PHONETIC = '0'

var getPhonetic func(c byte) uint8

func init() {
	var phoneticMap = [26]uint8{
		'0', '1', '2', '3', '0', '1', '2', '0', '0', '2', '2', '4', '5', '5', '0', '1', '2', '6', '2', '3', '0', '1', '0', '2', '0', '2',
	}

	const LOOKUP_OFFSET = 65   // 'A' char code
	const UPPER_BIT_MASK = 223 // 11011111

	getPhonetic = func(c byte) uint8 {
		lookupKey := (UPPER_BIT_MASK & c) - LOOKUP_OFFSET
		if lookupKey > 25 {
			return NO_VALUE
		}

		// flip the 6th bit to make it uppercase and subtract 65 to get the index
		return phoneticMap[lookupKey]
	}
}

func assert(condition bool, message string) {
	if !condition {
		panic(message)
	}
}
