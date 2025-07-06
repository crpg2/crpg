export const applyPolynomialFunction = (x: number, coefficients: number[]): number => {
  let r = 0
  for (let degree = 0; degree < coefficients.length; degree += 1) {
    const coeff = coefficients[coefficients.length - degree - 1]
    if (coeff !== undefined) {
      r += coeff * x ** degree
    }
  }

  return r
}

export const roundFLoat = (num: number) => Math.round((num + Number.EPSILON) * 100) / 100

export const percentOf = (val: number, of: number) => {
  if (of === 0) {
    return 0
  }

  return (val / of) * 100
}
