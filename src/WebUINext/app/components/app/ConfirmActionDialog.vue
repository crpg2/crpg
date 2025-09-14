<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'

import { errorMessagesToString, sameAs } from '~/services/validators-service'

const props = withDefaults(defineProps<{
  open?: boolean
  title?: string
  description?: string

  confirm: string

  confirmLabel?: string
  noSelect?: boolean
  undone?: boolean
}>(), {
  noSelect: true,
  undone: true,
})

const emit = defineEmits<{
  cancel: []
  confirm: []
}>()

const confirmModel = ref<string>('')

const $v = useVuelidate(
  {
    confirmModel: {
      sameAs: sameAs(props.confirm, props.confirm),
    },
  },
  { confirmModel },
)

const onCancel = () => {
  confirmModel.value = ''
  $v.value.$reset()
  emit('cancel')
}

const onConfirm = async () => {
  if (!(await $v.value.$validate())) {
    return
  }

  emit('confirm')
}
</script>

<template>
  <UModal
    :open
    :title
    :ui="{
      body: 'space-y-6 text-center',
      footer: 'flex items-center justify-center gap-4',
    }"
    @update:open="onCancel"
  >
    <slot />

    <template #body>
      <UAlert
        color="warning"
        variant="subtle"
        icon="crpg:alert-circle"
      >
        <template #description>
          <div class="flex flex-col gap-3">
            <slot name="description">
              {{ description }}
            </slot>
            <template v-if="undone">
              <h5 class="text-sm text-error">
                {{ $t('action-undone') }}
              </h5>
            </template>
          </div>
        </template>
      </UAlert>

      <div class="space-y-3">
        <i18n-t
          scope="global"
          keypath="confirm.name"
          tag="p"
        >
          <template #name>
            <span
              class="font-bold text-primary"
              :class="{ 'select-none': noSelect }"
            >
              {{ confirm }}
            </span>
          </template>
        </i18n-t>

        <UFormField
          :error="errorMessagesToString($v.confirmModel.$errors) || false"
          data-aq-confirm-field
        >
          <UInput
            v-model="confirmModel"
            :placeholder="$t('confirm.placeholder')"
            class="w-full"
            data-aq-confirm-input
          />
        </UFormField>
      </div>
    </template>

    <template #footer>
      <UButton
        variant="outline"
        size="xl"
        :label="$t('action.cancel')"
        data-aq-confirm-action="cancel"
        @click="onCancel"
      />
      <UButton
        :disabled="$v.confirmModel.$invalid"
        size="xl"
        :label="confirmLabel ?? $t('action.confirm')"
        data-aq-confirm-action="submit"
        @click="onConfirm"
      />
    </template>
  </UModal>
</template>
