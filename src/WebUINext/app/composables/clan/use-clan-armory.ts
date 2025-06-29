import { useAsyncState } from '@vueuse/core'

import {
  addItemToClanArmory,
  borrowItemFromClanArmory,
  getClanArmory,
  removeItemFromClanArmory,
  returnItemToClanArmory,
} from '~/services/clan-service'

export const useClanArmory = (clanId: number) => {
  const {
    state: clanArmory,
    execute: loadClanArmory,
    isLoading: isLoadingClanArmory,
  } = useAsyncState(() => getClanArmory(clanId), [], {
    immediate: false,
    resetOnExecute: false,
  })

  const addItem = (itemId: number) => addItemToClanArmory(clanId, itemId)

  const removeItem = (itemId: number) => removeItemFromClanArmory(clanId, itemId)

  const borrowItem = (itemId: number) => borrowItemFromClanArmory(clanId, itemId)

  const returnItem = (itemId: number) => returnItemToClanArmory(clanId, itemId)

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
