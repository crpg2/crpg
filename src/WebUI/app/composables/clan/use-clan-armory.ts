import { useAsyncState } from '@vueuse/core'

import {
  addItemToClanArmory,
  borrowItemFromClanArmory,
  getClanArmory,
  removeItemFromClanArmory,
  returnItemToClanArmory,
} from '~/services/clan-service'

import { useClan } from './use-clan'

export const useClanArmory = () => {
  const { clan } = useClan()

  const {
    state: clanArmory,
    executeImmediate: loadClanArmory,
    isLoading: isLoadingClanArmory,
  } = useAsyncState(() => getClanArmory(clan.value.id), [], { resetOnExecute: false })

  const addItem = (itemId: number) => addItemToClanArmory(clan.value.id, itemId)

  const removeItem = (itemId: number) => removeItemFromClanArmory(clan.value.id, itemId)

  const borrowItem = (itemId: number) => borrowItemFromClanArmory(clan.value.id, itemId)

  const returnItem = (itemId: number) => returnItemToClanArmory(clan.value.id, itemId)

  return {
    clanArmory,
    isLoadingClanArmory,
    loadClanArmory,
    addItem,
    borrowItem,
    removeItem,
    returnItem,
  }
}
