import { h } from 'vue'

import type { WeaponClass } from '~/models/item'
import type { QuestDefinition } from '~/models/quest'

import { UBadge } from '#components'
import { useI18n } from '#imports'
import { GAME_EVENT_FIELD } from '~/models/quest'
import { weaponClassToIcon } from '~/services/item-service'

function collectFilterValues(filters: Record<string, string>[]): Map<string, Set<string>> {
  const map = new Map<string, Set<string>>()

  for (const filter of filters) {
    for (const [key, value] of Object.entries(filter)) {
      if (!map.has(key)) {
        map.set(key, new Set())
      }
      map.get(key)!.add(value)
    }
  }
  return map
}

export function useQuestTitle(questDefinition: QuestDefinition) {
  const { t, n } = useI18n()

  const filtersByKey = collectFilterValues(questDefinition.eventFiltersJson)
  const { eventType, aggregationType, requiredValue } = questDefinition

  const hitType = Array.from(filtersByKey.get(GAME_EVENT_FIELD.HitType) ?? []).at(0)
  const title = hitType
    ? t(`user.quests.generate.tplByEventTypeAndAggregationTypeAndHitType.${eventType}_${aggregationType}.${hitType}`, { value: n(requiredValue) })
    : t(`user.quests.generate.tplByEventTypeAndAggregationType.${eventType}_${aggregationType}`, { value: n(requiredValue) })

  const bodyPartBadges = Array.from(filtersByKey.get(GAME_EVENT_FIELD.BodyPart) ?? [])
    .map(value => h(UBadge, {
      variant: 'subtle',
      color: 'neutral',
      label: t(`user.quests.generate.eventField.BodyPart.values.${value}`),
    }))

  const weaponClassBadges = Array.from(filtersByKey.get(GAME_EVENT_FIELD.WeaponClass) ?? [])
    .map(value => h(UBadge, {
      variant: 'subtle',
      color: 'neutral',
      icon: `crpg:${weaponClassToIcon[value as WeaponClass]}`,
      label: t(`item.weaponClass.${value}`),
    }))

  return () => {
    const nodes = [h('span', title)]

    if (bodyPartBadges.length > 0) {
      nodes.push(h('span', t('user.quests.generate.titleConnector.to')))
      nodes.push(...bodyPartBadges)
    }

    if (weaponClassBadges.length > 0) {
      nodes.push(h('span', t('user.quests.generate.titleConnector.with')))
      nodes.push(...weaponClassBadges)
    }

    return h('div', {
      class: 'flex items-center flex-wrap gap-1.5',
    }, nodes)
  }
}
