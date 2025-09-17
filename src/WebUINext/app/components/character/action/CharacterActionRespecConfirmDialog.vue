<script setup lang="ts">
import type { Character } from '~/models/character'
import type { RespecCapability } from '~/services/character-service'

defineProps<{
  respecCapability: RespecCapability
  character: Character
}>()

defineEmits<{
  close: [boolean]
}>()
</script>

<template>
  <AppConfirmActionDialog
    :title="$t('character.settings.respecialize.dialog.title')"
    :confirm="character.name"
    @close="(res) => $emit('close', res)"
  >
    <template #title>
      <i18n-t
        scope="global"
        keypath="character.settings.respecialize.dialog.desc"
        tag="div"
      >
        <template #character>
          <CharacterMedia :character class="font-bold text-primary" />
        </template>
        <template #respecializationPrice>
          <AppCoin
            :value="respecCapability.price"
            :class="{ '!text-error': respecCapability.price > 0 }"
          />
        </template>
      </i18n-t>
    </template>
  </AppConfirmActionDialog>
</template>
