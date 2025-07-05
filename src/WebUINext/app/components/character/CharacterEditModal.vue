<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'

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
      maxLength: maxLength(32),
      minLength: minLength(2),
      required: required(),
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

const wasChange = computed(() => nameModel.value !== character.name)

const characterNameMaxLength = 32

const [shownConfirmDeleteDialog, toggleConfirmDeleteDialog] = useToggle()
</script>

<template>
  <div>
    <UModal
      :title="$t('character.settings.update.title')"
      :ui="{
        content: 'max-w-xl',
        body: 'space-y-6',
        footer: 'flex justify-center',
      }"
      :close="{
        size: 'sm',
        color: 'secondary',
        variant: 'solid',
      }"
    >
      <UTooltip :text="$t('character.settings.update.title')">
        <UButton
          size="xl"
          icon="crpg:edit"
          color="secondary"
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
                class="text-2xs text-muted tabular-nums"
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
            :disabled="$v.nameModel.$invalid || !wasChange"
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
            <span
              class="
                cursor-pointer text-status-danger
                hover:text-status-danger/80
              " @click="toggleConfirmDeleteDialog(true)"
            >
              {{ $t('character.settings.delete.link') }}
            </span>
          </template>
        </i18n-t>
      </template>
    </UModal>

    <AppConfirmActionDialog
      v-if="shownConfirmDeleteDialog"
      open
      :title="$t('character.settings.delete.dialog.title')"
      :description="$t('character.settings.delete.dialog.desc')"
      :name="`${character.name} - ${character.level}`"
      :confirm-label="$t('action.delete')"
      @cancel="toggleConfirmDeleteDialog(false);"
      @confirm="() => {
        $emit('delete')
        toggleConfirmDeleteDialog(false);
      }"
      @update:open="toggleConfirmDeleteDialog(false)"
    />
  </div>
</template>
