export const applyPolynomialFunction = (
  x: number,
  coefficients: number[],
) => coefficients.reduce((acc, coefficient, idx) => acc + coefficient * x ** (coefficients.length - idx - 1), 0)

export const roundFLoat = (num: number) => Math.round((num + Number.EPSILON) * 100) / 100
