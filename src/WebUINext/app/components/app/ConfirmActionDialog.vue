<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'

import { errorMessagesToString, sameAs } from '~/services/validators-service'

const props = withDefaults(defineProps<{
  // TODO: FIXME: use ModalProps after resolve https://github.com/nuxt/module-builder/issues/597#issuecomment-2862766112
  open?: boolean
  title?: string
  description?: string

  name: string
  confirmLabel: string
  noSelect?: boolean
}>(), {
  noSelect: false,
})

const emit = defineEmits<{
  cancel: []
  confirm: []
}>()

const confirmNameModel = ref<string>('')

const $v = useVuelidate(
  {
    confirmNameModel: {
      // TODO:
      sameAs: sameAs(props.name, props.name),
    },
  },
  { confirmNameModel },
)

const onCancel = () => {
  confirmNameModel.value = ''
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
    :description
    :ui="{
      body: 'space-y-6 text-center',
      footer: 'flex items-center justify-center gap-4',
    }"
    :close="{
      size: 'sm',
      color: 'secondary',
      variant: 'solid',
    }"
    @update:open="onCancel"
  >
    <slot />
    <template #body>
      <slot name="title" />

      <!-- TODO: FIXME: -->
      <UAlert color="error">
        <template #title>
          Are you sure you want to retire your character? This action cannot be undone
        </template>
      </UAlert>

      <slot name="description" />

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
            {{ name }}
          </span>
        </template>
      </i18n-t>

      <UFormField
        :error="errorMessagesToString($v.confirmNameModel.$errors)"
        data-aq-confirm-field
      >
        <UInput
          v-model="confirmNameModel"
          :placeholder="$t('confirm.placeholder')"
          size="sm"
          class="w-full"
          data-aq-confirm-input
        />
      </UFormField>
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
        :disabled="$v.confirmNameModel.$invalid"
        size="xl"
        :label="confirmLabel"
        data-aq-confirm-action="submit"
        @click="onConfirm"
      />
    </template>
  </UModal>
</template>
