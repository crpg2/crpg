import { useAsyncState } from '@vueuse/core'

import {
  addItemToClanArmory,
  borrowItemFromClanArmory,
  getClanArmory,
  removeItemFromClanArmory,
  returnItemToClanArmory,
} from '~/services/clan-service'

export const useClanArmory = (clanId: MaybeRefOrGetter<number>) => {
  const {
    state: clanArmory,
    execute: loadClanArmory,
    isLoading: isLoadingClanArmory,
  } = useAsyncState(() => getClanArmory(toValue(clanId)), [], {
    immediate: false,
    resetOnExecute: false,
    throwError: true,
  })

  const getClanArmoryItem = (userItemId: number) => clanArmory.value.find(ca => ca.userItemId === userItemId)

  const addItem = (itemId: number) => addItemToClanArmory(toValue(clanId), itemId)

  const removeItem = (itemId: number) => removeItemFromClanArmory(toValue(clanId), itemId)

  const borrowItem = (itemId: number) => borrowItemFromClanArmory(toValue(clanId), itemId)

  const returnItem = (itemId: number) => returnItemToClanArmory(toValue(clanId), itemId)

  return {
    clanArmory,
    isLoadingClanArmory,
    loadClanArmory,
    getClanArmoryItem,
    addItem,
    borrowItem,
    removeItem,
    returnItem,
  }
}
