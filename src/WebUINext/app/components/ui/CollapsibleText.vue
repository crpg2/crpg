<script setup lang="ts">
const { charsLength = 50 } = defineProps<{ text: string, charsLength?: number }>()
</script>

<template>
  <template v-if="text.length <= charsLength">
    {{ text }}
  </template>

  <OCollapse
    v-else
    :open="false"
    position="bottom"
  >
    <template #trigger="props">
      <template v-if="!props.open">
        {{ text.substring(0, charsLength) }}...
        <OButton
          v-tooltip="$t('action.expand')"
          variant="secondary"
          rounded
          size="2xs"
          :aria-expanded="props.open"
          icon-right="chevron-down"
        />
      </template>
      <OButton
        v-else
        v-tooltip="$t('action.collapse')"
        variant="secondary"
        size="2xs"
        rounded
        :aria-expanded="props.open"
        icon-right="chevron-up"
        class="mt-1"
      />
    </template>
    {{ text }}
  </OCollapse>
</template>
