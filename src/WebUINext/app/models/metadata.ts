import type { CharacterPublic } from '~/models/character'
import type { ClanPublic } from '~/models/clan'
import type { UserPublic } from '~/models/user'

export interface MetadataDict {
  users: UserPublic[]
  characters: CharacterPublic[]
  clans: ClanPublic[]
}
