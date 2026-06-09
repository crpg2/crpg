import { computed, toValue } from 'vue'

import { getItemImage, getRankColor } from '~/services/item-service'

export const useItem = (data: MaybeRefOrGetter<{ baseId: string, rank: number }>) => {
  const rankColor = computed(() => getRankColor(toValue(data).rank))

  const thumb = computed(() => getItemImage(toValue(data).baseId))

  return {
    rankColor,
    thumb,
  }
}
