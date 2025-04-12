<script setup lang="ts">
import {
  freeRespecializeIntervalDays,
  freeRespecializePostWindowHours,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { RespecCapability } from '~/services/characters-service'

import { parseTimestamp } from '~/utils/date'

defineProps<{
  respecCapability: RespecCapability
  character: Character
}>()

defineEmits<{
  respec: []
}>()
</script>

<template>
  <Modal :disabled="!respecCapability.enabled">
    <VTooltip placement="auto">
      <OButton
        variant="info"
        size="lg"
        :disabled="!respecCapability.enabled"
        icon-left="chevron-down-double"
        data-aq-character-action="respecialize"
        class="ring-1 ring-white/25"
      >
        <div class="flex items-center gap-2">
          <span>{{ $t('character.settings.respecialize.title') }}</span>
          <Tag
            v-if="respecCapability.price === 0"
            variant="success"
            size="sm"
            label="free"
          />
          <Coin v-else />
        </div>
      </OButton>

      <template #popper>
        <div class="prose prose-invert">
          <h5>{{ $t('character.settings.respecialize.tooltip.title') }}</h5>
          <div
            v-html="
              $t('character.settings.respecialize.tooltip.desc', {
                freeRespecPostWindow: $t('dateTimeFormat.hh', {
                  hours: freeRespecializePostWindowHours,
                }),
                freeRespecInterval: $t('dateTimeFormat.dd', {
                  days: freeRespecializeIntervalDays,
                }),
              })
            "
          />

          <div
            v-if="respecCapability.freeRespecWindowRemain > 0"
            v-html="
              $t('character.settings.respecialize.tooltip.freeRespecPostWindowRemaining', {
                remainingTime: $t('dateTimeFormat.dd:hh:mm', {
                  ...parseTimestamp(respecCapability.freeRespecWindowRemain),
                }),
              })
            "
          />

          <template v-else-if="respecCapability.price > 0">
            <i18n-t
              scope="global"
              keypath="character.settings.respecialize.tooltip.paidRespec"
              tag="p"
            >
              <template #respecPrice>
                <Coin :value="respecCapability.price" />
              </template>
            </i18n-t>

            <div
              v-html="
                $t('character.settings.respecialize.tooltip.freeRespecIntervalNext', {
                  nextFreeAt: $t('dateTimeFormat.dd:hh:mm', {
                    ...parseTimestamp(respecCapability.nextFreeAt),
                  }),
                })
              "
            />
          </template>
        </div>
      </template>
    </VTooltip>

    <template #popper="{ hide }">
      <ConfirmActionForm
        :title="$t('character.settings.respecialize.dialog.title')"
        :name="character.name"
        :confirm-label="$t('action.apply')"
        @cancel="hide"
        @confirm="
          () => {
            $emit('respec')
            hide();
          }
        "
      >
        <template #description>
          <i18n-t
            scope="global"
            keypath="character.settings.respecialize.dialog.desc"
            tag="p"
          >
            <template #respecializationPrice>
              <Coin
                :value="respecCapability.price"
                :class="{ 'text-status-danger': respecCapability.price > 0 }"
              />
            </template>
          </i18n-t>
        </template>
      </ConfirmActionForm>
    </template>
  </Modal>
</template>
