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
        <div class="prose">
          <i18n-t
            v-if="!canSetCharacterForTournament"
            scope="global"
            keypath="character.settings.tournament.tooltip.required"
            class="text-warning"
            tag="h4"
          >
            <template #requiredLevel>
              <span>{{ `<${tournamentLevelThreshold}` }}</span>
            </template>
          </i18n-t>

          <h3>
            {{ $t('character.settings.tournament.tooltip.title') }}
          </h3>

          <i18n-t
            scope="global"
            keypath="character.settings.tournament.tooltip.desc"
            tag="p"
          >
            <template #tournamentLevel>
              <span class="text-sm font-bold text-highlighted">
                {{ tournamentLevelThreshold }}
              </span>
            </template>
          </i18n-t>
        </div>
      </template>
    </UTooltip>

    <AppConfirmActionDialog
      :open="shownConfirmDialog"
      :title="$t('character.settings.tournament.dialog.title')"
      :confirm="character.name"
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
