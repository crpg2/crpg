<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'

import { errorMessagesToString, maxLength, minLength, required } from '~/services/validators-service'

const emit = defineEmits<{
  create: [name: string]
  delete: []
}>()

const nameModel = ref<string>('')

const presetNameMaxLength = 300

const $v = useVuelidate(
  {
    nameModel: {
      required: required(),
      minLength: minLength(1),
      maxLength: maxLength(presetNameMaxLength),
    },
  },
  { nameModel },
)

const onConfirm = async () => {
  if (!(await $v.value.$validate())) {
    return
  }

  emit('create', nameModel.value)
}
</script>

<template>
  <UModal
    :title="$t('character.inventory.presets.save.title')"
    :ui="{
      content: 'max-w-xl',
      body: 'space-y-6',
      footer: 'flex justify-center',
    }"
  >
    <template #body="{ close }">
      <UFormField
        :label="$t('character.inventory.presets.save.form.field.name')"
        :error="errorMessagesToString($v.nameModel.$errors)"
        size="xl"
      >
        <UInput
          v-model="nameModel"
          :maxlength="presetNameMaxLength"
          aria-describedby="preset-name-count"
          class="w-full"
        >
          <template #trailing>
            <UiInputCounter
              id="preset-name-count"
              :max="presetNameMaxLength"
              :current="nameModel.length "
            />
          </template>
        </UInput>
      </UFormField>

      <div class="flex items-center justify-center gap-4">
        <UButton
          variant="outline"
          size="xl"
          :label="$t('action.cancel')"
          @click="close"
        />
        <UButton
          :disabled="!nameModel.trim()"
          size="xl"
          :label="$t('action.save')"
          @click="onConfirm"
        />
      </div>
    </template>
  </UModal>
</template>
