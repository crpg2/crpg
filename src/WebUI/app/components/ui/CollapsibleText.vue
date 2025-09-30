<script setup lang="ts">
const { charsLength = 50 } = defineProps<{ text: string, charsLength?: number }>()
</script>

<template>
  <template v-if="text.length <= charsLength">
    {{ text }}
  </template>

  <UCollapsible
    v-else :ui="{
      root: 'flex data-[state=open]:flex-col-reverse flex-col items-start gap-1',
    }"
  >
    <template #default="{ open }">
      <div v-if="!open">
        {{ text.substring(0, charsLength) }}...
        <UButton
          color="neutral"
          variant="subtle"
          size="xs"
          :label="$t('action.expand')"
          :aria-expanded="open"
          icon="crpg:chevron-down"
        />
      </div>
      <UButton
        v-else
        color="neutral"
        size="xs"
        variant="subtle"
        :label="$t('action.collapse')"
        icon="crpg:chevron-up"
      />
    </template>

    <template #content>
      {{ text }}
    </template>
  </UCollapsible>
</template>
