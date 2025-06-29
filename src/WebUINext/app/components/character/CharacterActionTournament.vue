<script setup lang="ts">
import type { Character } from '~/models/character'

import { canSetCharacterForTournamentValidate, tournamentLevelThreshold } from '~/services/character-service'

const props = defineProps<{
  character: Character
}>()

defineEmits<{
  tournament: []
}>()

const [shownConfirmDialog, toggleConfirmDialog] = useToggle()

const canSetCharacterForTournament = computed(() => canSetCharacterForTournamentValidate(props.character))
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
        icon="crpg:member"
        :disabled="!canSetCharacterForTournament"
        data-aq-character-action="forTournament"
        :label="$t('character.settings.tournament.title')"
        @click="toggleConfirmDialog(true)"
      />

      <template #content>
        <div class="prose prose-invert">
          <h5 class="text-content-100">
            {{ $t('character.settings.tournament.tooltip.title') }}
          </h5>

          <i18n-t
            scope="global"
            keypath="character.settings.tournament.tooltip.desc"
            tag="p"
          >
            <template #tournamentLevel>
              <span class="text-sm font-bold text-content-100">
                {{ tournamentLevelThreshold }}
              </span>
            </template>
          </i18n-t>

          <i18n-t
            v-if="!canSetCharacterForTournament"
            scope="global"
            keypath="character.settings.tournament.tooltip.requiredDesc"
            class="text-status-danger"
            tag="p"
          >
            <template #requiredLevel>
              <span class="text-xs font-bold">{{ `<${tournamentLevelThreshold}` }}</span>
            </template>
          </i18n-t>
        </div>
      </template>
    </UTooltip>

    <AppConfirmActionDialog
      v-if="shownConfirmDialog"
      open
      :title="$t('character.settings.tournament.dialog.title')"
      :name="character.name"
      :confirm-label="$t('action.confirm')"
      @cancel="toggleConfirmDialog(false);"
      @confirm="() => {
        $emit('tournament');
        toggleConfirmDialog(false);
      }"
      @update:open="toggleConfirmDialog(false)"
    >
      <template #description>
        <i18n-t
          scope="global"
          keypath="character.settings.tournament.dialog.desc"
          tag="p"
        >
          <template #character>
            <CharacterMedia :character class="font-bold text-primary" />
          </template>
        </i18n-t>
      </template>
    </AppConfirmActionDialog>
  </div>
</template>
