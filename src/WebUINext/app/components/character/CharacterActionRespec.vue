<script setup lang="ts">
import {
  freeRespecializeIntervalDays,
  freeRespecializePostWindowHours,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { RespecCapability } from '~/services/character-service'

import { parseTimestamp } from '~/utils/date'

defineProps<{
  respecCapability: RespecCapability
  character: Character
}>()

defineEmits<{
  respec: []
}>()

const [shownConfirmDialog, toggleConfirmDialog] = useToggle()
</script>

<template>
  <div>
    <UTooltip
      :ui="{
        content: 'max-w-96',
      }"
    >
      <UButton
        size="lg"
        variant="outline"
        block
        :disabled="!respecCapability.enabled"
        icon="crpg:chevron-down-double"
        data-aq-character-action="respecialize"
        :label="$t('character.settings.respecialize.title')"
        @click="toggleConfirmDialog(true)"
      >
        <template #trailing>
          <UBadge
            v-if="respecCapability.price === 0"
            color="success"
            variant="soft"
            size="sm"
            label="free"
          />
          <AppCoin v-else />
        </template>
      </UButton>

      <template #content>
        <div class="prose prose-invert">
          <h5>{{ $t('character.settings.respecialize.tooltip.title') }}</h5>
          <div
            v-html="$t('character.settings.respecialize.tooltip.desc', {
              freeRespecPostWindow: $t('dateTimeFormat.hh', { hours: freeRespecializePostWindowHours }),
              freeRespecInterval: $t('dateTimeFormat.dd', { days: freeRespecializeIntervalDays }),
            })"
          />
          <div
            v-if="respecCapability.freeRespecWindowRemain > 0"
            v-html="$t('character.settings.respecialize.tooltip.freeRespecPostWindowRemaining', {
              remainingTime: $t('dateTimeFormat.dd:hh:mm', parseTimestamp(respecCapability.freeRespecWindowRemain)),
            })"
          />
          <template v-else-if="respecCapability.price > 0">
            <i18n-t
              scope="global"
              keypath="character.settings.respecialize.tooltip.paidRespec"
              tag="p"
            >
              <template #respecPrice>
                <AppCoin :value="respecCapability.price" />
              </template>
            </i18n-t>

            <div
              v-html="$t('character.settings.respecialize.tooltip.freeRespecIntervalNext', {
                nextFreeAt: $t('dateTimeFormat.dd:hh:mm', parseTimestamp(respecCapability.nextFreeAt)),
              }) "
            />
          </template>
        </div>
      </template>
    </UTooltip>

    <AppConfirmActionDialog
      v-if="shownConfirmDialog"
      open
      :title="$t('character.settings.respecialize.dialog.title')"
      :name="character.name"
      :confirm-label="$t('action.confirm')"
      @cancel="toggleConfirmDialog(false);"
      @confirm="() => {
        $emit('respec');
        toggleConfirmDialog(false);
      }"
      @update:open="toggleConfirmDialog(false)"
    >
      <template #description>
        <i18n-t
          scope="global"
          keypath="character.settings.respecialize.dialog.desc"
          tag="p"
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
  </div>
</template>
