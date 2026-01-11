const numberCandidate = (candidate: string) => /^[+-]?\d+(?:\.\d+)?$/.test(candidate)
const tryParseFloat = (str: string) => (numberCandidate(str) ? Number.parseFloat(str) : str)

export function tryGetNumber(str: string): [true, number]
export function tryGetNumber(str: string): [false, string]
export function tryGetNumber(str: string): [boolean, number | string] {
  const candidateToNumber = tryParseFloat(str)
  const isNumber = typeof candidateToNumber === 'number' && !Number.isNaN(candidateToNumber)
  return [isNumber, candidateToNumber]
}
