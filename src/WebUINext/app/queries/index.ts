export const CHARACTER_QUERY_KEYS = {
  root: ['characters'] as const,
  byId: (id: number) => [...CHARACTER_QUERY_KEYS.root, id] as const,
  items: (id: number) => [...CHARACTER_QUERY_KEYS.byId(id), { items: true }] as const,
}

export const USER_QUERY_KEYS = {
  root: ['self'] as const,
  items: () => [...USER_QUERY_KEYS.root, { items: true }] as const,
}
