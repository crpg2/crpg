export const CHARACTER_QUERY_KEYS = {
  root: ['characters'] as const,
  byId: (id: number) => [...CHARACTER_QUERY_KEYS.root, id] as const,
  items: (id: number) => [...CHARACTER_QUERY_KEYS.byId(id), { items: true }] as const,
  characteristics: (id: number) => [...CHARACTER_QUERY_KEYS.byId(id), { characteristics: true }] as const,
  statistics: (id: number) => [...CHARACTER_QUERY_KEYS.byId(id), { statistics: true }] as const,
}

export const USER_QUERY_KEYS = {
  root: ['self'] as const,
  items: () => [...USER_QUERY_KEYS.root, { items: true }] as const,
}

export const CLAN_QUERY_KEYS = {
  root: ['clans'] as const,
  byId: (id: number) => [...CLAN_QUERY_KEYS.root, id] as const,
}

export const BATTLE_QUERY_KEYS = {
  root: ['battles'] as const,
  byId: (id: number) => [...BATTLE_QUERY_KEYS.root, id] as const,
}
