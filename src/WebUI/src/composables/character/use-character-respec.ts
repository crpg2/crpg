import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getCharacterLimitations, getRespecCapability, respecializeCharacter } from '~/services/characters-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'
import { characterCharacteristicsKey, characterKey } from '~/symbols/character'

export const useCharacterRespec = () => {
  const userStore = useUserStore()
  const character = injectStrict(characterKey)
  const { loadCharacterCharacteristics } = injectStrict(characterCharacteristicsKey)

  const {
    state: characterLimitations,
    execute: loadCharacterLimitations,
  } = useAsyncState(
    ({ id }: { id: number }) => getCharacterLimitations(id),
    { lastRespecializeAt: new Date() },
    {
      immediate: false,
      resetOnExecute: false,
    },
  )

  const respecCapability = computed(() =>
    getRespecCapability(
      character.value,
      characterLimitations.value,
      userStore.user!.gold,
      userStore.isRecentUser,
    ),
  )

  const {
    execute: onRespecializeCharacter,
    loading: respecializingCharacter,
  } = useAsyncCallback(
    async (characterId: number) => {
      userStore.replaceCharacter(await respecializeCharacter(characterId))
      userStore.subtractGold(respecCapability.value.price)
      await Promise.all([
        loadCharacterLimitations(0, { id: characterId }),
        loadCharacterCharacteristics(0, { id: characterId }),
      ])
      notify(t('character.settings.respecialize.notify.success'))
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
