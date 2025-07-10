export const getMinRange = (buckets: number[]): number => {
  if (buckets.length === 0) {
    return 0
  }
  return Math.floor(Math.min(...buckets))
}

export const getMaxRange = (buckets: number[]): number => {
  if (buckets.length === 0) {
    return 0
  }
  return Math.ceil(Math.max(...buckets))
}

export const getStepRange = (values: number[]): number => {
  if (values.every(Number.isInteger)) {
    return 1
  } // Ammo, stackAmount

  const [min, max] = [getMinRange(values), getMaxRange(values)]
  const diff = max - min

  if ((values.length < 20 && diff < 10) || (values.length > 20 && diff < 5)) {
    return 0.1
  }

  return 1
}
