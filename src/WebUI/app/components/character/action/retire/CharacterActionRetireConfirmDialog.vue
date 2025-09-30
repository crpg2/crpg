<script setup lang="ts">
import { minimumLevel } from '~root/data/constants.json'

import type { Character } from '~/models/character'

import { getExperienceMultiplierBonus, getHeirloomPointByLevel } from '~/services/character-service'

const { character, userExperienceMultiplier } = defineProps<{
  character: Character
  userExperienceMultiplier: number
}>()

defineEmits<{
  close: [boolean]
}>()

const heirloomPointByLevel = computed(() => getHeirloomPointByLevel(character.level))

const experienceMultiplierBonus = computed(() => getExperienceMultiplierBonus(userExperienceMultiplier))
</script>

<template>
  <AppConfirmActionDialog
    :title="$t('character.settings.retire.dialog.title')"
    :confirm="character.name"
    :confirm-label="$t('action.confirm')"
    @close="(res) => $emit('close', res)"
  >
    <template #title>
      <div class="space-y-2">
        <i18n-t
          scope="global"
          keypath="character.settings.retire.dialog.desc"
          tag="div"
        >
          <template #character>
            <CharacterMedia :character class="font-bold text-primary" />
          </template>
        </i18n-t>

        <i18n-t
          scope="global"
          keypath="character.settings.retire.dialog.reward"
          tag="div"
        >
          <template #heirloom>
            <AppLoom :point="heirloomPointByLevel" />
          </template>
          <template #multiplierBonus>
            <span class="font-bold text-success">
              +{{ $n(experienceMultiplierBonus, 'percent', { minimumFractionDigits: 0 }) }}
            </span>
          </template>
          <template #resetLevel>
            <span class="font-bold text-error">{{ minimumLevel }}</span>
          </template>
        </i18n-t>
      </div>
    </template>
  </AppConfirmActionDialog>
</template>
