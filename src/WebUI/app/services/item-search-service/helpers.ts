export const getMinRange = (buckets: number[]): number =>
  Math.floor(buckets.length ? Math.min(...buckets) : 0)

export const getMaxRange = (buckets: number[]): number =>
  Math.ceil(buckets.length ? Math.max(...buckets) : 0)

export const getStepRange = (values: number[]): number => {
  if (values.every(Number.isInteger)) {
    return 1 // Ammo, StackAmount...
  }

  const diff = getMaxRange(values) - getMinRange(values)
  return (values.length < 20 && diff < 10) || (values.length >= 20 && diff < 5)
    ? 0.1
    : 1
}
