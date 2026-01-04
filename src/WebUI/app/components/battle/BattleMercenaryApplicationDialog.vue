<script setup lang="ts">
import {
  strategusMercenaryMaxWage,
  strategusMercenaryNoteMaxLength,
} from '~root/data/constants.json'
import { isEqual } from 'es-toolkit'

import type { BattleMercenaryApplication, BattleMercenaryApplicationCreation, BattleSide, BattleSideDetailed } from '~/models/strategus/battle'

const { side, sideInfo, onApply } = defineProps<{
  side: BattleSide
  sideInfo: BattleSideDetailed
  onApply: (value: {
    characterId: number
    note: string
    wage: number
  }) => void
  onDelete: () => void
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
  characterId: sideInfo.mercenaryApplication?.character.id || null,
  note: sideInfo.mercenaryApplication?.note || '',
  wage: sideInfo.mercenaryApplication?.wage || 0,
})

const isDirty = computed(() => sideInfo.mercenaryApplication
  ? !isEqual(applicationModel.value, {
      characterId: sideInfo.mercenaryApplication.character.id,
      note: sideInfo.mercenaryApplication.note,
      wage: sideInfo.mercenaryApplication.wage,
    })
  : true)

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
      content: 'max-w-xl',
      body: 'space-y-6',
      footer: 'block space-y-6',
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
        <UFormField
          label="Note"
          help="Free-form text: TODO:"
          size="xl"
        >
          <UTextarea
            v-model="applicationModel.note"
            autoresize
            :readonly="sideInfo.mercenaryApplication?.status === 'Accepted'"
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

        <UFormField size="xl">
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

          <template #hint>
            <UiInputCounter
              :current="applicationModel.wage"
              :max="strategusMercenaryMaxWage"
            />
          </template>

          <UInputNumber
            v-model="applicationModel.wage"
            :readonly="sideInfo.mercenaryApplication?.status === 'Accepted'"
            :max="strategusMercenaryMaxWage"
            :min="0"
            :step="1000"
            class="w-40"
          />
        </UFormField>

        <div class="flex items-center justify-center gap-4">
          <UButton
            variant="outline"
            size="xl"
            :label="$t('action.cancel')"
            @click="onCancel"
          />

          <UButton
            size="xl"
            :disabled="!isDirty"
            label="Apply"
            @click="apply"
          />
        </div>
      </div>
    </template>

    <template v-if="sideInfo.mercenaryApplication?.status === 'Pending'" #footer>
      <div class="text-center">
        You can
        <ULink
          class="
            cursor-pointer text-error
            hover:text-error/80
          "
          @click="onDelete"
        >
          delete your application
        </ULink>.
        You won't be able to restore it
      </div>
    </template>
  </UModal>
</template>
