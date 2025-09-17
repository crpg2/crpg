<script setup lang="ts">
import { LazyCharacterActionRespecConfirmDialog } from '#components'
import { freeRespecializeIntervalDays, freeRespecializePostWindowHours } from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { RespecCapability } from '~/services/character-service'

import { parseTimestamp } from '~/utils/date'

const { character, respecCapability } = defineProps<{
  respecCapability: RespecCapability
  character: Character
}>()

const emit = defineEmits<{
  respec: []
}>()

const overlay = useOverlay()

const confirmDialog = overlay.create(LazyCharacterActionRespecConfirmDialog, {
  props: {
    character,
    respecCapability,
  },
})

async function respec() {
  const confirm = await confirmDialog.open()

  if (!confirm) {
    return
  }

  emit('respec')
}
</script>

<template>
  <UTooltip
    :ui="{
      content: 'max-w-96',
    }"
  >
    <UButton
      size="xl"
      variant="outline"
      block
      :disabled="!respecCapability.enabled"
      icon="crpg:chevron-down-double"
      data-aq-character-action="respecialize"
      :label="$t('character.settings.respecialize.title')"
      @click="respec"
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
      <div class="prose">
        <h3>
          {{ $t('character.settings.respecialize.tooltip.title') }}
        </h3>

        <i18n-t
          scope="global"
          keypath="character.settings.respecialize.tooltip.desc[0]"
          tag="p"
        >
          <template #freeRespecPostWindow>
            <strong>{{ $t('dateTimeFormat.hh', { hours: freeRespecializePostWindowHours }) }}</strong>
          </template>
          <template #freeRespecInterval>
            <strong>{{ $t('dateTimeFormat.hh', { hours: freeRespecializeIntervalDays }) }}</strong>
          </template>
        </i18n-t>

        <p>{{ $t('character.settings.respecialize.tooltip.desc[1]') }}</p>

        <i18n-t
          v-if="respecCapability.freeRespecWindowRemain > 0"
          scope="global"
          keypath="character.settings.respecialize.tooltip.freeRespecPostWindowRemaining"
          tag="p"
        >
          <template #remainingTime>
            <strong>{{ $t('dateTimeFormat.dd:hh:mm', { ...parseTimestamp(respecCapability.freeRespecWindowRemain) }) }}</strong>
          </template>
        </i18n-t>

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

          <i18n-t
            scope="global"
            keypath="character.settings.respecialize.tooltip.freeRespecIntervalNext"
            tag="p"
          >
            <template #nextFreeAt>
              <strong>{{ $t('dateTimeFormat.dd:hh:mm', { ...parseTimestamp(respecCapability.nextFreeAt) }) }}</strong>
            </template>
          </i18n-t>
        </template>
      </div>
    </template>
  </UTooltip>
</template>
