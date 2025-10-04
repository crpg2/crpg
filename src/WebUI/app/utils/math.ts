export const applyPolynomialFunction = (
  x: number,
  coefficients: number[],
) => coefficients.reduce((acc, coefficient, idx) => acc + coefficient * x ** (coefficients.length - idx - 1), 0)

export const roundFLoat = (num: number) => Math.round((num + Number.EPSILON) * 100) / 100

export const percentOf = (val: number, of: number) => {
  if (of === 0) {
    return 0
  }

  return (val / of) * 100
}
