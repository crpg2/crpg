<script setup lang="ts">
import type { SelectItem } from '@nuxt/ui'

import { useModerationUser } from '~/composables/moderator/use-moderation-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ROLE } from '~/models/role'
import {
  getAutoRetireCount,
  getCharactersByUserId,
  getExperienceMultiplierBonusByRetireCount,
  getLevelByExperience,
  sumExperienceMultiplierBonus,
} from '~/services/character-service'
import { getItemImage } from '~/services/item-service'
import { rewardCharacter, rewardUser } from '~/services/moderation-service'
import { updateUserNote } from '~/services/restriction-service'
import { useUserStore } from '~/stores/user'

const emit = defineEmits<{
  update: []
}>()

const toast = useToast()

const userStore = useUserStore()
const { moderationUser } = useModerationUser()

const {
  state: characters,
  execute: loadCharacters,
} = await useAsyncState(
  () => getCharactersByUserId(moderationUser.value!.id),
  [],
  {
    resetOnExecute: false,
  },
)

const note = ref<string>(moderationUser.value.note || '')

const { execute: onUpdateNote, isLoading: updatingNote } = useAsyncCallback(async () => {
  await updateUserNote(moderationUser.value.id, note.value)
  toast.add({
    title: 'The user note has been updated',
    close: false,
    color: 'success',
  })
  emit('update')
})

//

// TODO: Reward - refactoring, spec
const canReward = computed(() => userStore.user!.role === ROLE.Admin) // TODO: to service

interface RewardForm {
  gold: number
  itemId: string
  experience: number
  autoRetire: boolean
  characterId?: number
  heirloomPoints: number
}

const getDefaultRewardForm = (): RewardForm => ({
  // TODO: FIXME:
  autoRetire: true,
  characterId: characters.value[0]?.id,
  experience: 111_111_111,
  gold: 111_111_111,
  heirloomPoints: 111,
  itemId: 'crpg_basic_imperial_leather_armor_v2',
  // autoRetire: false,
  // experience: 0,
  // gold: 0,
  // heirloomPoints: 0,
  // itemId: '',
})

const rewardFormModel = ref<RewardForm>(getDefaultRewardForm())

const selectedCharacter = computed(() =>
  characters.value.find(c => c.id === rewardFormModel.value.characterId),
)

const {
  execute: onSubmitRewardForm,
  isLoading: rewarding,
} = useAsyncCallback(async () => {
  if (
    rewardFormModel.value.gold !== 0
    || rewardFormModel.value.heirloomPoints !== 0
    || rewardFormModel.value.itemId !== ''
  ) {
    await rewardUser(moderationUser.value.id, {
      gold: rewardFormModel.value.gold,
      heirloomPoints: rewardFormModel.value.heirloomPoints,
      itemId: rewardFormModel.value.itemId,
    })

    toast.add({
      title: 'The user has been rewarded',
      close: false,
      color: 'success',
    })
  }

  if (rewardFormModel.value.characterId && rewardFormModel.value.experience !== 0) {
    await rewardCharacter(moderationUser.value.id, rewardFormModel.value.characterId!, {
      autoRetire: rewardFormModel.value.autoRetire,
      experience: rewardFormModel.value.experience,
    })

    toast.add({
      title: 'The character has been rewarded',
      close: false,
      color: 'success',
    })
  }

  rewardFormModel.value = {
    ...getDefaultRewardForm(),
    characterId: rewardFormModel.value.characterId,
  }

  await loadCharacters()

  emit('update')
})

const totalRewardValues = computed(() => {
  const gold = moderationUser.value.gold + rewardFormModel.value.gold
  const heirloomPoints = moderationUser.value.heirloomPoints + rewardFormModel.value.heirloomPoints

  if (rewardFormModel.value.autoRetire) {
    const { remainExperience, retireCount } = getAutoRetireCount(
      rewardFormModel.value.experience,
      selectedCharacter.value!.experience,
    )

    return {
      experience: remainExperience,
      experienceMultiplier: sumExperienceMultiplierBonus(
        moderationUser.value.experienceMultiplier,
        getExperienceMultiplierBonusByRetireCount(retireCount),
      ),
      gold,
      heirloomPoints,
      level: getLevelByExperience(remainExperience),
    }
  }

  return {
    experience: selectedCharacter.value!.experience + rewardFormModel.value.experience,
    experienceMultiplier: moderationUser.value.experienceMultiplier,
    gold,
    heirloomPoints,
    level: getLevelByExperience(selectedCharacter.value!.experience + rewardFormModel.value.experience),
  }
})
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-8 pb-8">
    <UCard>
      <template #header>
        User
      </template>
      <ModeratorModerationUserOverview :user="moderationUser" />
    </UCard>

    <UCard>
      <template #header>
        Characters
      </template>
      <div class="flex flex-wrap gap-3">
        <CharacterMedia
          v-for="character in characters"
          :key="character.id"
          class="rounded-full border border-border-200 px-3 py-2"
          :character="character"
          :is-active="character.id === moderationUser?.activeCharacterId"
        />
      </div>
    </UCard>

    <UCard v-if="canReward">
      <template #header>
        Rewards
      </template>

      <UForm
        class="space-y-8"
        :state="rewardFormModel"
        @submit.prevent
      >
        <div class="grid grid-cols-3 gap-4">
          <UFormField>
            <template #label>
              <UiDataCell>
                <template #leftContent>
                  <UiSpriteSymbol
                    name="coin"
                    viewBox="0 0 18 18"
                    class="size-5"
                  />
                </template>
                Gold
              </UiDataCell>
            </template>
            <UInputNumber
              v-model="rewardFormModel.gold"
              placeholder="Gold"
              size="lg"
              class="w-full"
            />
          </UFormField>

          <UFormField>
            <template #label>
              <UiDataCell>
                <template #leftContent>
                  <UIcon
                    name="crpg:blacksmith"
                    class="size-5 text-primary"
                  />
                </template>
                Heirloom points
              </UiDataCell>
            </template>
            <UInputNumber
              v-model="rewardFormModel.heirloomPoints"
              placeholder="Heirloom points"
              size="lg"
              class="w-full"
            />
          </UFormField>

          <div class="col-span-2 space-y-4">
            <UFormField label="Personal item">
              <UInput
                v-model="rewardFormModel.itemId"
                placeholder="crpg_"
                size="lg"
                class="w-full"
              />
            </UFormField>

            <div v-if="rewardFormModel.itemId" class="h-24 w-52">
              <ItemThumb
                :key="rewardFormModel.itemId"
                :thumb="getItemImage(rewardFormModel.itemId)"
                :name="rewardFormModel.itemId"
              />
            </div>
          </div>
        </div>

        <USeparator />

        <div v-if="selectedCharacter" class="grid grid-cols-3 gap-4">
          <UFormField
            class="col-span-1"
            label="Character"
          >
            <USelect
              v-model="rewardFormModel.characterId"
              size="lg"
              :items="characters.map<SelectItem>((character) => ({
                label: character.name,
                value: character.id,
              }))"
              class="w-full"
            />
          </UFormField>

          <UFormField>
            <template #label>
              <UiDataCell>
                <template #leftContent>
                  <UIcon
                    name="crpg:experience"
                    class="size-5 text-primary"
                  />
                </template>
                Experience
              </UiDataCell>
            </template>

            <UInputNumber
              v-model="rewardFormModel.experience"
              placeholder="Experience"
              size="lg"
              class="w-full"
            />
          </UFormField>

          <UFormField label="Auto retire">
            <USwitch v-model="rewardFormModel.autoRetire" />
          </UFormField>

          <USeparator class="col-span-3" />

          <div class="col-span-3 space-y-4">
            <!-- TODO: to cmp -->
            <div
              v-if="rewardFormModel.heirloomPoints"
              class="flex items-center gap-2 font-bold"
            >
              <AppLoom :point="moderationUser.heirloomPoints" />
              ->
              <AppLoom
                :point="totalRewardValues.heirloomPoints" :class="[rewardFormModel.heirloomPoints < 0 ? `
                  text-status-danger
                ` : `text-status-success`]"
              />
            </div>

            <div
              v-if="rewardFormModel.gold"
              class="flex items-center gap-2 font-bold"
            >
              <AppCoin :value="moderationUser.gold" />
              ->
              <AppCoin
                :value="totalRewardValues.gold" :class="[rewardFormModel.gold < 0 ? `
                  text-status-danger
                ` : `text-status-success`]"
              />
            </div>

            <template v-if="rewardFormModel.experience">
              <div class="flex items-center gap-2 font-bold">
                <AppExperience :value="selectedCharacter.experience" />
                ->
                <AppExperience
                  :value="totalRewardValues.experience" :class="[rewardFormModel.experience < 0 ? `
                    text-status-danger
                  ` : `text-status-success`]"
                />
              </div>

              <div
                v-if="rewardFormModel.autoRetire && totalRewardValues.experienceMultiplier - moderationUser.experienceMultiplier !== 0"
                class="flex items-center gap-2 font-bold"
              >
                <span>exp. multi</span>
                {{ $n(moderationUser.experienceMultiplier) }}
                ->
                <span class="text-status-success">
                  {{ $n(totalRewardValues.experienceMultiplier) }}
                </span>
              </div>

              <div
                v-if="totalRewardValues.level - selectedCharacter.level !== 0"
                class="flex items-center gap-2 font-bold"
              >
                <span>lvl</span>
                <span>{{ selectedCharacter.level }}</span>
                <span>-></span>
                <span
                  class="text-status-success"
                  :class="[rewardFormModel.experience < 0 ? 'text-status-danger' : `
                    text-status-success
                  `]"
                >
                  {{ totalRewardValues.level }}
                </span>
              </div>
            </template>
          </div>
        </div>

        <AppConfirmActionPopover
          :confirm-label="$t('action.ok')"
          title="Are you sure you want to reward this user?"
          placement="bottom"
          @confirm="onSubmitRewardForm"
        >
          <UButton size="lg" label="Submit" :loading="rewarding" />
        </AppConfirmActionPopover>
      </UForm>
    </UCard>

    <UCard>
      <template #header>
        Note
      </template>

      <UForm
        class="space-y-8"
        :state="note"
        @submit="onUpdateNote"
      >
        <UFormField help="For internal use">
          <UTextarea
            v-model="note"
            placeholder="User note"
            class="w-full"
            autoresize
          />
        </UFormField>

        <UButton
          :disabled="moderationUser.note === note"
          size="lg"
          :loading="updatingNote"
          label="Update"
          type="submit"
        />
      </UForm>
    </UCard>
  </div>
</template>
