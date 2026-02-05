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

export const MAP_BATTLE_QUERY_KEYS = {
  root: ['map-battles'] as const,
  byId: (id: number) => [...MAP_BATTLE_QUERY_KEYS.root, id] as const,
  fightersById: (id: number) => [...MAP_BATTLE_QUERY_KEYS.byId(id), { fightersById: true }] as const,
  fighterApplicationsById: (id: number) => [...MAP_BATTLE_QUERY_KEYS.byId(id), { figterApplications: true }] as const,
}

export const PARTY_QUERY_KEYS = {
  root: ['party'] as const,
}
