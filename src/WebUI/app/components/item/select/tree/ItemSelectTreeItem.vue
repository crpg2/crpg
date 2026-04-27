<script setup lang="ts">
import type { TreeNode } from '~/composables/item/use-item-select-tree'

import { canUpgradeItem } from '~/services/item-service'

defineProps<{
  node: TreeNode
  expanded: boolean
  selected: boolean
  supportsUpgrades: boolean
  loadingUpgrade: boolean
}>()

defineEmits<{
  select: []
}>()
</script>

<template>
  <div class="flex w-full items-center gap-2">
    <UiLoading
      v-if="supportsUpgrades && node.level === 'item' && node.item"
      :active="loadingUpgrade"
    />

    <UIcon
      v-if="node.level === 'type' || node.level === 'weaponClass'"
      :name="`crpg:${node.icon}`"
      class="size-10"
    />

    <UCheckbox
      v-if="(node.level === 'item' || node.level === 'upgrade') && node.item"
      :model-value="selected"
      :disabled="node.disabled"
      tabindex="-1"
      @change="!node.disabled && $emit('select')"
      @click.stop
    >
      <template #label>
        <ItemMedia
          :base-id="node.item.baseId"
          :rank="node.item.rank"
          :name="node.item.name"
        >
          <template v-if="$slots['item-top-right']" #top-right>
            <slot name="item-top-right" />
          </template>
        </ItemMedia>
      </template>
    </UCheckbox>

    <UiTextView v-else-if="node.label" variant="h5">
      {{ node.label }}
    </UiTextView>

    <div class="ml-auto flex items-center gap-3">
      <UBadge
        v-if="node.level === 'type' || node.level === 'weaponClass'"
        color="neutral"
        :variant="node.level === 'type' ? 'subtle' : 'soft'"
      >
        {{ node.count }}
      </UBadge>

      <UButton
        v-if="node.children?.length || (supportsUpgrades && node.level === 'item' && canUpgradeItem(node.item!.type))"
        icon="i-lucide-chevron-down"
        variant="link"
        color="neutral"
        :ui="{
          leadingIcon: ['transition-transform', expanded ? 'rotate-180' : ''],
        }"
      />
    </div>
  </div>
</template>
