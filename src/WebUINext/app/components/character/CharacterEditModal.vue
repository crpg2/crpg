<script setup lang="ts">
import type { Character } from '~/models/character'

const { character } = defineProps<{
  character: Character
}>()

defineEmits<{
  update: [name: string]
  delete: []
}>()
</script>

<template>
  <UiModal>
    <OButton
      v-tooltip="$t('character.settings.update.title')"
      size="xl"
      icon-right="edit"
      rounded
      variant="secondary"
      outlined
    />

    <template #popper="{ hide: hideParentModal }">
      <div class="min-w-[480px] max-w-2xl space-y-14 px-12 py-11">
        <CharacterEditForm
          :character
          @cancel="hideParentModal"
          @confirm="name => {
            $emit('update', name)
            hideParentModal();
          }"
        />
        <i18n-t
          scope="global"
          keypath="character.settings.delete.title"
          tag="div"
          class="text-center"
        >
          <template #link>
            <UiModal>
              <span class="cursor-pointer text-status-danger hover:text-opacity-80">
                {{ $t('character.settings.delete.link') }}
              </span>
              <template #popper="{ hide }">
                <AppConfirmActionForm
                  :title="$t('character.settings.delete.dialog.title')"
                  :description="$t('character.settings.delete.dialog.desc')"
                  :name="character.name"
                  :confirm-label="$t('action.delete')"
                  @cancel="hide"
                  @confirm="() => {
                    hide();
                    $emit('delete')
                  }"
                />
              </template>
            </UiModal>
          </template>
        </i18n-t>
      </div>
    </template>
  </UiModal>
</template>
