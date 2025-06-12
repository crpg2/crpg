<script setup lang="ts">
import { clamp } from 'es-toolkit'

import { useModerationUser } from '~/composables/moderator/use-moderation-user'
import { Role } from '~/models/role'
import {
  getAutoRetireCount,
  getCharactersByUserId,
  getExperienceMultiplierBonusByRetireCount,
  getLevelByExperience,
  sumExperienceMultiplierBonus,
} from '~/services/character-service'
import { rewardCharacter, rewardUser } from '~/services/moderation-service'
import { updateUserNote } from '~/services/restriction-service'
import { useUserStore } from '~/stores/user'

const emit = defineEmits<{
  update: []
}>()

const { moderationUser } = useModerationUser()

const { n } = useI18n()
const { $notify } = useNuxtApp()

const userStore = useUserStore()

const note = ref<string>(moderationUser.value?.note || '')

const { state: characters, execute: loadCharacters } = await useAsyncState(
  () => getCharactersByUserId(moderationUser.value!.id),
  [],
  {
    resetOnExecute: false,
  },
)

const onSubmitNoteForm = async () => {
  if (moderationUser.value!.note !== note.value) {
    await updateUserNote(moderationUser.value!.id, { note: note.value })
    $notify('The user note has been updated')
    emit('update')
  }
}

// TODO: Reward - refactoring, spec
const canReward = computed(() => userStore.user!.role === Role.Admin) // TODO: to service

interface RewardForm {
  gold: number
  itemId: string
  experience: number
  autoRetire: boolean
  characterId?: number
  heirloomPoints: number
}

const getDefaultRewardForm = (): RewardForm => ({
  autoRetire: false,
  characterId: characters.value[0]?.id,
  experience: 0,
  gold: 0,
  heirloomPoints: 0,
  itemId: '',
})

const rewardFormModel = ref<RewardForm>(getDefaultRewardForm())

const selectedCharacter = computed(() =>
  characters.value.find(c => c.id === rewardFormModel.value.characterId),
)

const tryParseNumber = (value: string): number => {
  if (value === '-') {
    value = '-0'
  }
  return Number(value.replace(/[^.0-9\\-]/g, ''))
}

// TODO: to cmp, or composable
const experienceModel = computed({
  get() {
    return n(rewardFormModel.value.experience || 0)
  },
  set(val: string) {
    rewardFormModel.value.experience = clamp(tryParseNumber(val), 0, Infinity)
  },
})

// TODO: mask to cmp, or composable
const goldModel = computed({
  get() {
    return n(rewardFormModel.value.gold || 0)
  },
  set(val: string) {
    rewardFormModel.value.gold = clamp(tryParseNumber(val), moderationUser.value.gold * -1, Infinity)
  },
})

// TODO: mask to cmp, or composable
const heirloomPointsModel = computed({
  get() {
    return n(rewardFormModel.value.heirloomPoints || 0)
  },
  set(val: string) {
    rewardFormModel.value.heirloomPoints = clamp(tryParseNumber(val), moderationUser.value.heirloomPoints * -1, Infinity)
  },
})

const onSubmitRewardForm = async () => {
  if (!canReward.value) {
    return
  }

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
    $notify('The user has been rewarded')
  }

  if (rewardFormModel.value.characterId && rewardFormModel.value.experience !== 0) {
    await rewardCharacter(moderationUser.value.id, rewardFormModel.value.characterId!, {
      autoRetire: rewardFormModel.value.autoRetire,
      experience: rewardFormModel.value.experience,
    })
    $notify('The character has been rewarded')
  }

  rewardFormModel.value = {
    ...getDefaultRewardForm(),
    characterId: rewardFormModel.value.characterId,
  }

  await loadCharacters()
  emit('update')
}

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
    level: getLevelByExperience(
      selectedCharacter.value!.experience + rewardFormModel.value.experience,
    ),
  }
})
</script>

<template>
  <div>
    <div class="mx-auto max-w-3xl space-y-8 pb-8">
      <UiFormGroup

        label="User"
        :collapsable="false"
      >
        <div class="grid grid-cols-2 gap-2 text-2xs">
          <UiSimpleTableRow
            label="Id"
            :value="String(moderationUser.id)"
          />
          <UiSimpleTableRow
            :label="$t('character.statistics.expMultiplier.title')"
            :value="$t('character.format.expMultiplier', { multiplier: $n(moderationUser.experienceMultiplier) })"
          />
          <UiSimpleTableRow
            label="Region"
            :value="$t(`region.${moderationUser.region}`, 0)"
          />
          <UiSimpleTableRow label="Platform">
            {{ moderationUser.platform }} {{ moderationUser.platformUserId }}
            <UserPlatform
              :platform="moderationUser.platform"
              :platform-user-id="moderationUser.platformUserId"
              :user-name="moderationUser.name"
            />
          </UiSimpleTableRow>
          <UiSimpleTableRow
            v-if="moderationUser.clanMembership"
            label="Clan"
          >
            {{ moderationUser.clanMembership.clan.name }}
            <UserClan :clan="moderationUser.clanMembership.clan" />
          </UiSimpleTableRow>
          <UiSimpleTableRow
            label="Created"
            :value="$d(moderationUser.createdAt, 'long')"
          />
          <UiSimpleTableRow
            label="Last activity"
            :value="$d(moderationUser.updatedAt, 'long')"
          />
          <UiSimpleTableRow label="Gold">
            <AppCoin :value="moderationUser.gold" />
          </UiSimpleTableRow>
          <UiSimpleTableRow label="Heirloom">
            <Heirloom :value="moderationUser.heirloomPoints" />
          </UiSimpleTableRow>
          <UiSimpleTableRow label="Donor">
            {{ moderationUser.isDonor }}
          </UiSimpleTableRow>
        </div>
      </UiFormGroup>

      <UiFormGroup
        label="Characters"
        :collapsable="false"
      >
        <div class="flex flex-wrap gap-3">
          <CharacterMedia
            v-for="character in characters"
            :key="character.id"
            class="rounded-full border border-border-200 px-3 py-2"
            :character="character"
            :is-active="character.id === moderationUser?.activeCharacterId"
          />
        </div>
      </UiFormGroup>

      <UiFormGroup
        v-if="canReward"
        :collapsable="false"
        label="Rewards"
        can-reward
      >
        <form
          class="space-y-8"
          @submit.prevent
        >
          <div class="grid grid-cols-2 gap-4">
            <OField>
              <template #label>
                <div class="flex items-center gap-1.5">
                  <UiSpriteSymbol
                    name="coin"
                    inline
                    viewBox="0 0 18 18"
                    class="w-4.5"
                  />
                  Gold
                </div>
              </template>
              <OInput
                v-model="goldModel"
                placeholder="Gold"
                size="lg"
                expanded
              />
            </OField>

            <OField>
              <template #label>
                <div class="flex items-center gap-1.5">
                  <OIcon
                    icon="blacksmith"
                    size="sm"
                    class="text-primary"
                  />
                  Heirloom points
                </div>
              </template>

              <OInput
                v-model="heirloomPointsModel"
                placeholder="Heirloom points"
                size="lg"
                expanded
              />
            </OField>

            <OField label="Personal item">
              <OInput
                v-model="rewardFormModel.itemId"
                placeholder="crpg_"
                size="lg"
                expanded
              />
            </OField>
          </div>

          <div v-if="selectedCharacter" class="grid grid-cols-2 gap-4">
            <OField
              class="col-span-2"
              label="Character"
            >
              <VDropdown :triggers="['click']">
                <template #default="{ shown }">
                  <OButton
                    variant="secondary"
                    outlined
                    size="lg"
                  >
                    <CharacterMedia :character="selectedCharacter!" />
                    <Divider inline />
                    <OIcon
                      icon="chevron-down"
                      size="lg"
                      :rotation="shown ? 180 : 0"
                      class="text-content-400"
                    />
                  </OButton>
                </template>

                <template #popper="{ hide }">
                  <div class="max-h-64 max-w-md overflow-y-auto">
                    <UiDropdownItem
                      v-for="character in characters"
                      :key="character.id"
                      :active="character.id === selectedCharacter!.id"
                    >
                      <CharacterMedia
                        :character="character"
                        @click="
                          () => {
                            rewardFormModel.characterId = character.id;
                            hide();
                          }
                        "
                      />
                    </UiDropdownItem>
                  </div>
                </template>
              </VDropdown>
            </OField>

            <OField class="col-span-1">
              <template #label>
                <div class="flex items-center gap-1.5">
                  <OIcon
                    icon="experience"
                    size="lg"
                    class="text-primary"
                  />
                  Experience
                </div>
              </template>
              <OInput
                v-model="experienceModel"
                placeholder="Experience"
                size="lg"
                expanded
              />
            </OField>

            <OField
              class="col-span-1"
              label="Auto retire"
            >
              <OSwitch v-model="rewardFormModel.autoRetire" />
            </OField>

            <div class="col-span-2 space-y-4">
              <!-- TODO: to cmp -->
              <div
                v-if="rewardFormModel.heirloomPoints"
                class="flex items-center gap-2 font-bold"
              >
                <OIcon
                  icon="blacksmith"
                  size="lg"
                  class="text-primary"
                />
                {{ $n(moderationUser.heirloomPoints) }}
                ->
                <span
                  :class="[
                    rewardFormModel.heirloomPoints < 0 ? 'text-status-danger' : 'text-status-success',
                  ]"
                >
                  {{ $n(totalRewardValues.heirloomPoints) }}
                </span>
              </div>

              <div
                v-if="rewardFormModel.gold"
                class="flex items-center gap-2 font-bold"
              >
                <UiSpriteSymbol
                  name="coin"
                  inline
                  viewBox="0 0 18 18"
                  class="w-4.5"
                />
                {{ $n(moderationUser.gold) }}
                ->
                <span
                  :class="[rewardFormModel.gold < 0 ? 'text-status-danger' : 'text-status-success']"
                >
                  {{ $n(totalRewardValues.gold) }}
                </span>
              </div>

              <template v-if="rewardFormModel.experience">
                <div class="flex items-center gap-2 font-bold">
                  <OIcon
                    icon="experience"
                    size="lg"
                    class="text-primary"
                  />
                  {{ $n(selectedCharacter.experience) }}
                  ->
                  <span
                    :class="[
                      rewardFormModel.experience < 0 ? 'text-status-danger' : 'text-status-success',
                    ]"
                  >
                    {{ $n(totalRewardValues.experience) }}
                  </span>
                </div>

                <div
                  v-if="
                    rewardFormModel.autoRetire
                      && totalRewardValues.experienceMultiplier - moderationUser.experienceMultiplier !== 0
                  "
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
                    :class="[
                      rewardFormModel.experience < 0 ? 'text-status-danger' : 'text-status-success',
                    ]"
                  >
                    {{ totalRewardValues.level }}
                  </span>
                </div>
              </template>
            </div>
          </div>

          <div>
            <AppConfirmActionTooltip
              :confirm-label="$t('action.ok')"
              title="Are you sure you want to reward this user?"
              placement="bottom"
              @confirm="onSubmitRewardForm"
            >
              <OButton
                native-type="submit"
                variant="primary"
                size="lg"
                label="Submit"
              />
            </AppConfirmActionTooltip>
          </div>
        </form>
      </UiFormGroup>

      <UiFormGroup
        label="Note"
        :collapsable="false"
      >
        <form
          class="space-y-8"
          @submit.prevent="onSubmitNoteForm"
        >
          <OField message="For internal use">
            <OInput
              v-model="note"
              placeholder="User note"
              size="lg"
              expanded
              type="textarea"
              rows="6"
            />
          </OField>

          <OButton
            native-type="submit"
            :disabled="moderationUser.note === note"
            variant="primary"
            size="lg"
            label="Update"
          />
        </form>
      </UiFormGroup>
    </div>
  </div>
</template>
