import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getCharacterLimitations, getRespecCapability, respecializeCharacter } from '~/services/character-service'
import { useUserStore } from '~/stores/user'
import { characterCharacteristicsKey, characterKey } from '~/symbols/character'

import { useCharacter } from './use-character'

export const useCharacterRespec = () => {
  const toast = useToast()
  const { t } = useI18n()

  const userStore = useUserStore()
  // const character = injectStrict(characterKey)
  const { character } = useCharacter()

  // const { loadCharacterCharacteristics } = injectStrict(characterCharacteristicsKey)

  const {
    state: characterLimitations,
    execute: loadCharacterLimitations,
  } = useAsyncState(
    (id: number) => getCharacterLimitations(id),
    { lastRespecializeAt: new Date() },
    {
      immediate: false,
      resetOnExecute: false,
    },
  )

  const respecCapability = computed(() => getRespecCapability(
    character.value,
    characterLimitations.value,
    userStore.user!.gold,
    userStore.isRecentUser,
  ))

  const {
    execute: onRespecializeCharacter,
    loading: respecializingCharacter,
  } = useAsyncCallback(
    async (characterId: number) => {
      // TODO:
      // userStore.replaceCharacter(await respecializeCharacter(characterId))
      // userStore.subtractGold(respecCapability.value.price)
      // await Promise.all([
      //   loadCharacterLimitations(0, { id: characterId }),
      //   loadCharacterCharacteristics(0, { id: characterId }),
      // ])
      // notify(t('character.settings.respecialize.notify.success'))
    },
  )

  return {
    characterLimitations,
    loadCharacterLimitations,
    respecCapability,
    onRespecializeCharacter,
    respecializingCharacter,
  }
}
