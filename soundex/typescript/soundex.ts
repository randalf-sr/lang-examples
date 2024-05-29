/**
 * Converts a word to its Soundex representation.
 * @param word - The word string to convert.
 * @returns The Soundex representation of the provided word.
 */
export function soundex(word: string): string {
  assert(word && word.length > 0, '`value` must be a non-empty string')

  let result = word.charAt(0).toUpperCase() // first letter is always included
  let previous = getPhonetic(word.charCodeAt(0)) // phonetic value of the first letter

  for (let i = 1; i < word.length; i++) {
    const c = getPhonetic(word.charCodeAt(i))
    if (c === SKIP_PHONETIC) {
        previous = SKIP_PHONETIC // skip phonetic value
    } else if (c !== NO_VALUE && c !== previous) {
        result += (previous = c) // append phonetic value
        if (result.length === 4) {
            return result
        }
    }
  }

  return result.padEnd(4, '0') // pad with zeros
}

const NO_VALUE = undefined
const SKIP_PHONETIC = 0
const getPhonetic = (() => {
  const m = new Uint8Array([
    0, 1, 2, 3, 0, 1, 2, 0, 0, 2, 2, 4, 5, 5, 0, 1, 2, 6, 2, 3, 0, 1, 0, 2, 0, 2
  ])

  const LOOKUP_OFFSET = 65    /* 'A' char code */
  const UPPER_BIT_MASK = 223  /* 11011111 */

  // flip the 6th bit to make it uppercase and subtract 65 to get the index
  return c => m[(UPPER_BIT_MASK & c) - LOOKUP_OFFSET]
})() as (charCode: number) => number | undefined

// deno-lint-ignore no-explicit-any
function assert(condition: any, message: string) {
  if (!condition) {
    throw new Error(message)
  }
}

