<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import { maxValue } from '@vuelidate/validators'
import { strategusMercenaryMaxWage, strategusMercenaryNoteMaxLength } from '~root/data/constants.json'
import { drop } from 'es-toolkit'
import { Console } from 'node:console'

import type { Battle, BattleMercenaryApplication, BattleMercenaryApplicationCreation } from '~/models/strategus/battle'

import { BattleMercenary, BattleMercenaryApplicationStatus, BattleSide } from '~/models/strategus/battle'
import { NotificationType, notify } from '~/services/notification-service'
import { applyToBattleAsMercenary, battleSideToIcon, removeBattleMercenaryApplication } from '~/services/strategus-service/battle'
import { t } from '~/services/translate-service'
import { mapUserToUserPublic } from '~/services/users-service'
import {
  integer,
  maxLength,
  minLength,
  minValue,
  required,
} from '~/services/validators-service'
import { useUserStore } from '~/stores/user'













const props = withDefaults(
  defineProps<{
    update?: boolean
    battle: Battle
    application?: Omit<BattleMercenaryApplicationCreation, 'userId'>

  }>(),
  {
    update: false,
    application: () => ({
      characterId: 0,
      side: BattleSide.Attacker,
      wage: 0,
      note: '',
    }),
  },
)

const emit = defineEmits<{ update: [] }>()
const onRemove = async (popper: any) => {
  await removeBattleMercenaryApplication(props.battle.id)
  notify(t('battle.application.remove.notify.success'))
  popper.hide()
  emit('update')
}

const onApply = async (popper: any) => {
  if (!(await $v.value.$validate()) || selectedCharacter.value == null) {
    notify(t('form.validate.invalid'), NotificationType.Warning)
    return
  }

  const mercenaryApplication: BattleMercenaryApplicationCreation = {
    userId: userData.value.id,
    characterId: mercenaryApplicationFormModel.value.characterId,
    side: sideModel.value,
    wage: mercenaryApplicationFormModel.value.wage,
    note: mercenaryApplicationFormModel.value.note,
  }

  await applyToBattleAsMercenary(props.battle.id, mercenaryApplication)
  notify(t('battle.application.create.notify.success'))
  popper.hide()
  emit('update')
}

const userStore = useUserStore()
const { characters, user } = toRefs(userStore)

const userData = computed(() =>
  mapUserToUserPublic(user.value!, userStore.clan),
)

const mercenaryApplicationFormModel = ref<Omit<BattleMercenaryApplicationCreation, 'userId'>>(props.application)

const $v = useVuelidate(
  {
    wage: {
      integer,
      minValue: minValue(0),
      maxValue: maxValue(strategusMercenaryMaxWage),
      required,
    },
    note: {
      maxLength: maxLength(strategusMercenaryNoteMaxLength),
    },
  },
  mercenaryApplicationFormModel,
)

const sideModel = ref<BattleSide>(mercenaryApplicationFormModel.value.side)
const selectedCharacter = computed(() =>
  characters.value.find(char => char.id === mercenaryApplicationFormModel.value.characterId),
)

const battleSides = Object.values(BattleSide)

const onSelectCharacter = (id: number) => {
  mercenaryApplicationFormModel.value.characterId = id
}

if (userStore.characters.length === 0) {
  await userStore.fetchCharacters()
}
</script>

<script setup lang="ts">
</script>

<template>
  <Modal>
    <slot />

    <template #popper="popper">
      <div class="prose prose-invert space-y-6 px-12 py-10">
        <h1>{{ $t(`strategus.battle.application.mercenary.title`) }}</h1>
        <div class="space-y-8">
          <div class="flex items-center justify-center gap-4">
            <OField>
              <OTabs
                v-model="sideModel"
                content-class="hidden"
              >
                <OTabItem
                  v-for="side in battleSides"
                  :key="side"
                  :label="$t(`strategus.battle.side.${side.toLowerCase()}`, 0)"
                  :icon="battleSideToIcon[side]"
                  :value="side"
                />
              </OTabs>
            </OField>
          </div>
          <div class="flex justify-center">
            <VDropdown
              :triggers="['click']"
              placement="bottom-end"
            >
              <template #default="{ shown }">
                <OButton
                  variant="primary"
                  outlined
                  size="lg"
                >
                  <CharacterMedia
                    v-if="selectedCharacter"
                    :character="selectedCharacter"
                  />
                  <p v-else>
                    Select Character
                  </p>
                  <Divider inline />
                  <OIcon
                    icon="chevron-down"
                    size="lg"
                    :rotation="shown ? 180 : 0"
                    class="text-content-400"
                  />
                </OButton>
              </template>

              <template #popper>
                <div class="min-w-24">
                  <DropdownItem
                    v-for="char in characters"
                    :key="char.id"
                    v-close-popper
                    :checked="char.id === mercenaryApplicationFormModel.characterId"
                    class="justify-between"
                    @click="onSelectCharacter(char.id)"
                  >
                    <CharacterMedia :character="char" />
                  </DropdownItem>
                </div>
              </template>
            </VDropdown>
          </div>
          <OField>
            <template #label>
              <div class="flex items-center gap-1.5">
                <SvgSpriteImg
                  name="coin"
                  viewBox="0 0 18 18"
                  class="w-4.5"
                />
                {{ $t(`strategus.battle.application.wage`) }}
              </div>
            </template>
            <OInput
              v-model="mercenaryApplicationFormModel.wage"
              type="integer"
              size="sm"
              expanded
              :placeholder="$t('strategus.battle.application.wage')"
              :min-value="minValue"
              @blur="$v.wage.$touch"
              @focus="$v.wage.$reset"
            />
          </OField>
          <OField>
            <template #label>
              <div class="flex items-center gap-1.5">
                <OIcon
                  icon="edit"
                />
                {{ $t(`strategus.battle.application.note`) }}
              </div>
            </template>
            <OInput
              v-model="mercenaryApplicationFormModel.note"
              type="text"
              size="sm"
              expanded
              counter
              :maxlength="strategusMercenaryNoteMaxLength"
              :placeholder="$t('strategus.battle.application.note')"
              :min-length="minLength"
              :max-length="maxLength"
              @blur="$v.wage.$touch"
              @focus="$v.wage.$reset"
            />
          </OField>
        </div>

        <div class="flex items-center justify-center gap-4">
          <OButton
            variant="primary"
            outlined
            size="xl"
            :label="$t('action.cancel')"
            @click="popper.hide"
          />                <ConfirmActionTooltip
            :confirm-label="$t('action.ok')"
            :title="$t('strategus.battle.mercenary.remove.confirm')"
            placement="bottom"
            @confirm="onRemove(popper)"
          >
            <OButton
              v-if="props.update"
              variant="danger"
              outlined
              size="xl"
              :label="$t('action.remove')"
            />
          </ConfirmActionTooltip>
          <OButton
            variant="primary"
            size="xl"
            :label="props.update ? $t('action.update') : $t('action.apply')"
            @click="onApply(popper)"
          />
        </div>
      </div>
    </template>
  </Modal>
</template>
