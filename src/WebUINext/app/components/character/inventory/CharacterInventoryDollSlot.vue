<script setup lang="ts">
import type { CharacterArmorOverall } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

const {
  armorOverall,
  available = false,
  focused = false,
  invalid = false,
  notMeetRequirement = false,
  placeholder,
  remove = false,
  itemSlot,
  userItem,
} = defineProps<{
  itemSlot: ItemSlot
  placeholder: string
  userItem?: UserItem
  armorOverall?: CharacterArmorOverall
  notMeetRequirement?: boolean
  // slot state
  available?: boolean
  focused?: boolean
  invalid?: boolean
  remove?: boolean
}>()
</script>

<template>
  <div
    class="
      group relative flex h-28 items-center justify-center rounded-md
      bg-base-200 ring-2
    "
    :class="[
      [available ? 'ring-border-300' : `
        ring-transparent
        hover:ring-border-200
      `],
      {
        '!ring-status-success': focused,
        '!ring-status-warning': invalid,
        '!ring-status-danger': remove,
      },
    ]"
  >
    <ItemCard
      v-if="userItem !== undefined"
      :item="userItem.item"
      class="h-full w-full cursor-grab !ring-0"
      :class="{ 'bg-primary-hover/15': userItem.isPersonal }"
      data-aq-character-slot-item-thumb
    >
      <template #badges-top-right>
        <UTooltip v-if="notMeetRequirement" :text="$t('character.inventory.item.requirement.tooltip.title')">
          <UBadge
            variant="soft"
            color="error"
            icon="crpg:alert"
          />
        </UTooltip>
      </template>
    </ItemCard>

    <UTooltip v-else>
      <UIcon
        class="size-12 text-muted outline-0 select-none"
        :name="`crpg:${placeholder}`"
        data-aq-character-slot-item-placeholder
      />
      <template #content>
        <div>
          <div>
            {{ $t(`character.doll.slot.${itemSlot}.title`) }}
          </div>
          <!-- TODO: FIXME: -->
          <div v-if="$t(`character.doll.slot.${itemSlot}.description`) !== ''">
            {{ $t(`character.doll.slot.${itemSlot}.description`) }}
          </div>
        </div>
      </template>
    </UTooltip>

    <UTooltip
      v-if="armorOverall !== undefined"
      :text="$t(`character.doll.armorOverall.${armorOverall.key}`)"
    >
      <div
        class="
          absolute top-0 right-0 translate-x-1/2 -translate-y-3/4 cursor-default
        "
      >
        <UIcon
          name="crpg:shield-duotone" class="
            size-10 text-dimmed
            group-hover:text-muted
          "
        />
        <span
          class="
            absolute top-1/2 left-1/2 -translate-1/2 text-xs font-bold
            group-hover:text-highlighted
          "
        >
          {{ armorOverall.value }}
        </span>
      </div>
    </UTooltip>
  </div>
</template>
