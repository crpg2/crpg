<script setup lang="ts">
import {
  strategusMercenaryMaxWage,
  strategusMercenaryNoteMaxLength,
} from '~root/data/constants.json'

import type { BattleMercenaryApplication, BattleMercenaryApplicationCreation, BattleSide, BattleSideDetailed } from '~/models/strategus/battle'

const { side, application, onApply } = defineProps<{
  application?: BattleMercenaryApplication
  side: BattleSide
  sideInfo: BattleSideDetailed
  onApply: (value: {
    characterId: number
    note: string
    wage: number
  }) => void
}>()

const emit = defineEmits<{
  close: [boolean]
}>()

const toast = useToast()

const applicationModel = ref<{
  characterId: number | null
  note: string
  wage: number
}>({
  characterId: application?.character.id || null,
  note: application?.note || '',
  wage: application?.wage || 0,
})

const onCancel = () => {
  emit('close', false)
}

const apply = () => {
  // if (applicationModel.value.characterId === null) {
  //   return
  // }
  // @ts-expect-error TODO:
  onApply(applicationModel.value)
}
</script>

<template>
  <UModal
    :title="`Заявка на бой за ${side}`"
    :ui="{
      body: 'space-y-6',
      footer: 'flex items-center justify-center gap-4',
    }"
  >
    <template #body>
      <UFormField
        label="Briefing"
        size="xl"
      >
        <UTextarea
          readonly
          autoresize
          class="w-full"
          :model-value="sideInfo.briefing.note"
        />
      </UFormField>

      <UiDecorSeparator />

      <div class="space-y-4">
        <!--  -->
        <UFormField
          label="Note"
          help="Free-form text: TODO:"
          size="xl"
        >
          <UTextarea
            v-model="applicationModel.note"
            autoresize
            :maxlength="strategusMercenaryNoteMaxLength"
            class="w-full"
          />
          <template #hint>
            <UiInputCounter
              :current="applicationModel.note.length"
              :max="strategusMercenaryNoteMaxLength"
            />
          </template>
        </UFormField>

        <UFormField size="xl" :help="`max ${$n(strategusMercenaryMaxWage)}`">
          <template #label>
            <UiDataCell>
              <template #leftContent>
                <UiSpriteSymbol
                  name="coin"
                  viewBox="0 0 18 18"
                  class="size-5"
                />
              </template>
              Wage
            </UiDataCell>
          </template>

          <UInputNumber
            v-model="applicationModel.wage"
            :max="strategusMercenaryMaxWage"
            color="neutral"
            :step="1000"
            class="w-40"
          />
        </UFormField>
      </div>
    </template>

    <template #footer>
      <UButton
        variant="outline"
        size="xl"
        :label="$t('action.cancel')"
        @click="onCancel"
      />

      <UButton
        size="xl"
        label="Apply"
        @click="apply"
      />
    </template>
  </UModal>
</template>
