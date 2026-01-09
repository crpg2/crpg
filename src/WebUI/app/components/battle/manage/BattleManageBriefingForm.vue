<script setup lang="ts">
import {
  strategusBattleSideBriefingNoteMaxLength,
} from '~root/data/constants.json'

import type { BattleSideBriefing } from '~/models/strategus/battle'

const { briefing } = defineProps<{
  briefing: BattleSideBriefing
}>()

const emit = defineEmits<{
  save: [BattleSideBriefing]
}>()

const briefingModel = ref<BattleSideBriefing>({ ...briefing })

const onConfirm = async () => {
  emit('save', briefingModel.value)
}
</script>

<template>
  <div class="space-y-6">
    <UFormField
      label="Note"
      help="Free-form text: Discord, equipment, requirements, etc."
      size="xl"
    >
      <UTextarea
        v-model="briefingModel.note"
        autoresize
        :rows="7"
        :maxlength="strategusBattleSideBriefingNoteMaxLength"
        class="w-full"
      />
      <template #hint>
        <UiInputCounter
          :current="briefingModel.note.length"
          :max="strategusBattleSideBriefingNoteMaxLength"
        />
      </template>
    </UFormField>

    <div class="flex justify-end">
      <UButton
        size="xl"
        :label="$t('action.save')"
        @click="onConfirm"
      />
    </div>
  </div>
</template>
