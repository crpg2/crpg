<script setup lang="ts">
import { useUser } from '~/composables/user/use-user'

const modelValue = defineModel<number | null>({ default: null })

const { user } = useUser()

const isSelf = computed({
  get: () => {
    const uid = user.value?.id
    return uid !== undefined && modelValue.value === uid
  },
  set: (val: boolean) => {
    modelValue.value = val && user.value ? user.value.id : null
  },
})
</script>

<template>
  <div class="flex items-center gap-4">
    <UInput
      v-model="modelValue"
      size="xl"
      icon="crpg:search"
      type="text"
      inputmode="numeric"
      pattern="[0-9]*"
      :model-modifiers="{
        number: true,
        lazy: true,
        nullable: true,
        trim: true,
      }"
      :placeholder="$t('marketplace.history.filters.idPlaceholder')"
    >
      <template v-if="modelValue" #trailing>
        <UiInputClear @click="modelValue = null" />
      </template>
    </UInput>
    <USwitch
      v-model="isSelf"
      color="primary"
      size="xl"
      :label="$t('marketplace.history.filters.self')"
    />
  </div>
</template>
