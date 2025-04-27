import type { CharacterPublic } from '~/models/character'
import type { Clan } from '~/models/clan'
import type { UserPublic } from '~/models/user'

export interface MetadataDict {
  users: UserPublic[]
  characters: CharacterPublic[]
  clans: Clan[]
}
