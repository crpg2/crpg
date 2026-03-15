<script setup lang="ts">
interface ResourceOptions {
  max: number
  disabled: boolean
  tooltipText: string
}

defineProps<{
  label: string
  goldOptions: ResourceOptions
  heirloomPointsOptions: ResourceOptions
}>()

defineModel<number>('gold', { default: 0 })
defineModel<number>('heirloomPoints', { default: 0 })

const { t } = useI18n()
</script>

<template>
  <UiCard
    variant="subtle"
    :label="label"
    :ui="{
      body: 'space-y-4',
    }"
  >
    <UFormField>
      <slot name="item-select" />
    </UFormField>

    <div class="grid grid-cols-2 gap-4">
      <UFormField>
        <template #label>
          <UiDataMedia :label="t('marketplace.currency.gold')">
            <template #icon="{ classes }">
              <AppCoin :class="classes()" />
            </template>
          </UiDataMedia>
        </template>

        <UTooltip
          :disabled="!goldOptions.disabled"
          :text="goldOptions.tooltipText"
        >
          <UInputNumber
            :min="0"
            :max="goldOptions.max"
            variant="outline"
            color="neutral"
            size="xl"
            class="w-full"
            :disabled="goldOptions.disabled"
            :model-value="gold"
            @update:model-value="(val) => $emit('update:gold', val || 0)"
          />
        </UTooltip>
      </UFormField>

      <UFormField>
        <template #label>
          <UiDataMedia :label="t('marketplace.currency.heirloomPoints')">
            <template #icon="{ classes }">
              <AppLoom :class="classes()" />
            </template>
          </UiDataMedia>
        </template>

        <UTooltip
          :disabled="!heirloomPointsOptions.disabled"
          :text="heirloomPointsOptions.tooltipText"
        >
          <UInputNumber
            :min="0"
            :max="heirloomPointsOptions.max"
            variant="outline"
            color="neutral"
            size="xl"
            class="w-full"
            :disabled="heirloomPointsOptions.disabled"
            :model-value="heirloomPoints"
            @update:model-value="(val) => $emit('update:heirloomPoints', val || 0)"
          />
        </UTooltip>
      </UFormField>
    </div>
  </UiCard>
</template>
