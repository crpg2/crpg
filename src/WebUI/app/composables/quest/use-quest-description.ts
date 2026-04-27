import { useI18n } from '#imports'

import type { QuestDefinition, UserQuest } from '~/models/quest'

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

export function useQuestDescription(quest: MaybeRefOrGetter<UserQuest>) {
  const { t, n } = useI18n()
  const questRef = toRef(quest)

  function _getQuestName(def: QuestDefinition) {
    const base = t(`user.quests.generate.eventType.${def.eventType}`)
    const activeFilters = (def.eventFiltersJson ?? []).filter(f => Object.keys(f).length > 0)

    if (activeFilters.length === 0) {
      return base
    }

    const filtersByKey = collectFilterValues(activeFilters)
    const titleParts: string[] = []

    for (const [key, values] of filtersByKey) {
      const labels = new Set<string>()
      for (const value of values) {
        if (key === 'WeaponType') {
          // console.log({ key, value })

          // weaponClassToIcon
          labels.add(t(`item.weaponClass.${value}`))
          continue
        }
        labels.add(t(`user.quests.generate.filterTitle.${key}.${value}`, value))
      }
      titleParts.push([...labels].join('/'))
    }

    return titleParts.length > 0 ? `${base} · ${titleParts.join(' · ')}` : base
  }

  function _getQuestDescription(def: QuestDefinition): string {
    const baseKey = `user.quests.generate.description.${def.eventType}_${def.aggregationType}`
    const base = t(baseKey, { value: n(def.requiredValue) })
    const activeFilters = (def.eventFiltersJson ?? []).filter(f => Object.keys(f).length > 0)

    if (activeFilters.length === 0) {
      return base
    }

    const filtersByKey = collectFilterValues(activeFilters)
    const clauses: string[] = []

    for (const [filterKey, values] of filtersByKey) {
      const labels = new Set<string>()
      for (const value of values) {
        labels.add(t(`user.quests.generate.filterDesc.${filterKey}.${value}`, value))
      }
      clauses.push([...labels].join('/'))
    }

    return `${base} ${clauses.join(' ')}`
  }

  const questName = computed(() => _getQuestName(questRef.value.questDefinition))
  const questDescription = computed(() => _getQuestDescription(questRef.value.questDefinition))

  return { questName, questDescription }
}
