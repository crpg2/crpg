export function rgbHexColorToArgbInt(hexColor: string): number {
  return Number.parseInt(`FF${hexColor.substring(1)}`, 16)
}

export function argbIntToRgbHexColor(argb: number) {
  return `#${(argb & 0xFFFFFF).toString(16).padStart(6, '0')}`
}
