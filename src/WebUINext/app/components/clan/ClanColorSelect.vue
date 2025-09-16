<script setup lang="ts">
defineProps<{
  color: number
}>()

defineModel<string>('primary')
defineModel<string>('secondary')
</script>

<template>
  <div class="flex items-center gap-3">
    <ClanTagIcon
      :color
      class="size-14 rounded-sm bg-elevated p-1 ring ring-default"
    />

    <div class="space-y-3">
      <UFormField
        :label="`${$t('clan.update.form.field.primaryColor')}:`"
        :ui="{
          root: 'flex items-center gap-2',
          container: 'mt-0 flex items-center gap-2',
        }"
      >
        <div class="text-highlighted">
          {{ primary }}
        </div>
        <input
          :value="primary"
          variant="none"
          type="color"
          @input="$emit('update:primary', ($event.target as HTMLInputElement).value)"
        >
      </UFormField>

      <UFormField
        :label="`${$t('clan.update.form.field.secondaryColor')}:`"
        :ui="{
          root: 'flex items-center gap-2',
          container: 'mt-0 flex items-center gap-2',
        }"
      >
        <div class="text-highlighted">
          {{ secondary }}
        </div>
        <input
          :value="secondary"
          variant="none"
          type="color"
          @input="$emit('update:secondary', ($event.target as HTMLInputElement).value)"
        >
      </UFormField>
    </div>
  </div>
</template>
