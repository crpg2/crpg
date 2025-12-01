type JSONPrimitive = string | number | boolean | null
type JSONValue = JSONPrimitive | JSONObject | JSONArray
interface JSONObject {
  readonly [key: string]: JSONValue | undefined
}
interface JSONArray extends Array<JSONValue> {}
export type EntryKey = readonly JSONValue[]

export function toCacheKey(key: undefined): undefined
export function toCacheKey(key: EntryKey): string
export function toCacheKey(key: EntryKey | undefined): string | undefined
export function toCacheKey(key: EntryKey | undefined): string | undefined {
  return (
    key
    && JSON.stringify(key, (_, val) =>
      !val || typeof val !== 'object' || Array.isArray(val)
        ? val
        : Object.keys(val)
            .sort()
            .reduce((result, key) => {
              result[key] = val[key]
              return result
            }, {} as any))
  )
}
