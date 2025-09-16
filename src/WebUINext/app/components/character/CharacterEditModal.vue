<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { LazyAppConfirmActionDialog } from '#components'

import type { Character } from '~/models/character'

import { errorMessagesToString, maxLength, minLength, required } from '~/services/validators-service'

const { character } = defineProps<{
  character: Character
}>()

const emit = defineEmits<{
  update: [name: string]
  delete: []
}>()

const nameModel = ref<string>(character.name)

const $v = useVuelidate(
  {
    nameModel: {
      required: required(),
      minLength: minLength(2),
      maxLength: maxLength(32),
    },
  },
  { nameModel },
)

const onConfirm = async () => {
  if (!(await $v.value.$validate())) {
    return
  }

  emit('update', nameModel.value)
}

const isDirty = computed(() => nameModel.value !== character.name)
const { t } = useI18n()

const characterNameMaxLength = 32

const overlay = useOverlay()

const confirmDeleteDialog = overlay.create(LazyAppConfirmActionDialog, {
  props: {
    title: t('character.settings.delete.dialog.title'),
    description: t('character.settings.delete.dialog.desc'),
    confirm: `${character.name} - ${character.level}`,
    confirmLabel: t('action.delete'),
    onClose: () => confirmDeleteDialog.close(),
    onConfirm: () => {
      emit('delete')
      confirmDeleteDialog.close()
    },
  },
})
</script>

<template>
  <UModal
    :title="$t('character.settings.update.title')"
    :ui="{
      content: 'max-w-xl',
      body: 'space-y-6',
      footer: 'flex justify-center',
    }"
  >
    <UTooltip :text="$t('character.settings.update.title')">
      <UButton
        size="xl"
        icon="crpg:edit"
        color="neutral"
        variant="outline"
      />
    </UTooltip>

    <template #body="{ close }">
      <UFormField
        :label="$t('character.settings.update.form.field.name')"
        :error="errorMessagesToString($v.nameModel.$errors)"
      >
        <UInput
          v-model="nameModel"
          :maxlength="characterNameMaxLength"
          aria-describedby="character-name-count"
          class="w-full"
        >
          <template #trailing>
            <div
              id="character-name-count"
              class="text-xs text-muted tabular-nums"
              aria-live="polite"
              role="status"
            >
              {{ nameModel.length }}/{{ characterNameMaxLength }}
            </div>
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
          :disabled="!isDirty"
          size="xl"
          :label="$t('action.save')"
          @click="onConfirm"
        />
      </div>
    </template>

    <template #footer>
      <i18n-t
        scope="global"
        keypath="character.settings.delete.title"
        tag="div"
      >
        <template #link>
          <ULink
            class="
              cursor-pointer text-error
              hover:text-error/80
            "
            @click="confirmDeleteDialog.open"
          >
            {{ $t('character.settings.delete.link') }}
          </ULink>
        </template>
      </i18n-t>
    </template>
  </UModal>
</template>
