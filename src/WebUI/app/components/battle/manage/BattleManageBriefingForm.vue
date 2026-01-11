<script setup lang="ts">
import {
  strategusBattleSideBriefingNoteMaxLength,
} from '~root/data/constants.json'
import { isEqual } from 'es-toolkit'

import type { BattleSideBriefing } from '~/models/strategus/battle'

const { briefing } = defineProps<{
  briefing: BattleSideBriefing
  loading: boolean
}>()

const emit = defineEmits<{
  save: [BattleSideBriefing]
}>()

const briefingModel = ref<BattleSideBriefing>({ ...briefing })

const onConfirm = async () => {
  emit('save', briefingModel.value)
}

const reset = () => {
  briefingModel.value = { ...briefing }
}

const isDirty = computed(() => !isEqual(briefingModel.value, briefing))
</script>

<template>
  <div class="space-y-6">
    <UFormField
      :label="$t('strategus.battle.manage.briefing.form.note.label')"
      :help="$t('strategus.battle.manage.briefing.form.note.help')"
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

    <div class="flex items-center justify-center gap-4">
      <UButton
        variant="outline"
        size="xl"
        :disabled="!isDirty"
        :label="$t('action.reset')"
        @click="reset"
      />
      <UButton
        size="xl"
        :loading
        :disabled="!isDirty"
        :label="$t('action.save')"
        @click="onConfirm"
      />
    </div>
  </div>
</template>
