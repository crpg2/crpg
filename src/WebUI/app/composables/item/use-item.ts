import { computed, toValue } from 'vue'

import type { Item, ItemFlat } from '~/models/item'

import { getItemImage, getRankColor } from '~/services/item-service'

export const useItem = (item: MaybeRefOrGetter<Item | ItemFlat>) => {
  const rankColor = computed(() => getRankColor(toValue(item).rank))

  const thumb = computed(() => getItemImage(toValue(item).baseId))

  return {
    rankColor,
    thumb,
  }
}
