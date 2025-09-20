<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'

import { errorMessagesToString, sameAs } from '~/services/validators-service'

const props = withDefaults(defineProps<{
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
  close: [boolean]
}>()

const confirmModel = ref<string>('')

const $v = useVuelidate(
  {
    confirmModel: {
      sameAs: sameAs(props.confirm),
    },
  },
  { confirmModel },
)

const onCancel = () => {
  emit('close', false)
}

const onConfirm = async () => {
  if (!(await $v.value.$validate())) {
    return
  }

  emit('close', true)
}
</script>

<template>
  <UModal
    :title
    :ui="{
      body: 'space-y-5 text-center',
      footer: 'flex items-center justify-center gap-4',
    }"
  >
    <slot />

    <template #body>
      <UAlert
        color="warning"
        variant="outline"
        :ui="{
          root: 'ring-5',
        }"
      >
        <template #title>
          <slot name="title">
            {{ title }}
          </slot>
        </template>

        <template v-if="undone" #description>
          <UiTextView variant="h5" class="text-error">
            {{ $t('action-undone') }}
          </UiTextView>
        </template>
      </UAlert>

      <div class="space-y-3">
        <i18n-t
          scope="global"
          keypath="confirm.name"
          tag="div"
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
          :error="errorMessagesToString($v.confirmModel.$errors)"
          data-aq-confirm-field
          size="xl"
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
