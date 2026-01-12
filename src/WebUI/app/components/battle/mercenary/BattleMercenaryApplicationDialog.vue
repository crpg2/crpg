<script setup lang="ts">
import {
  strategusMercenaryMaxWage,
  strategusMercenaryNoteMaxLength,
} from '~root/data/constants.json'

import type { BattleSide, BattleSideDetailed } from '~/models/strategus/battle'

import { useCharacters, useCharactersProvider } from '~/composables/character/use-character'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_MERCENARY_APPLICATION_STATUS } from '~/models/strategus/battle'

const { sideInfo, onApply } = defineProps<{
  side: BattleSide
  sideInfo: BattleSideDetailed
  onApply: (value: {
    characterId: number
    note: string
    wage: number
  }) => void
  onDeleteApplication: () => void
  onLeaveFromBattle: () => void
}>()

const emit = defineEmits<{
  close: [boolean]
}>()

interface ApplicationModel {
  characterId: number | null
  note: string
  wage: number
}

const getEmptyApplicationModel = (): ApplicationModel => {
  return {
    characterId: null,
    note: '',
    wage: 0,
  }
}

const applicationModel = ref<ApplicationModel>(sideInfo.mercenaryApplication
  ? {
      characterId: sideInfo.mercenaryApplication.character.id,
      note: sideInfo.mercenaryApplication.note,
      wage: sideInfo.mercenaryApplication.wage,
    }
  : getEmptyApplicationModel())

const isNewApplication = ref(false)
const readonly = computed(() => Boolean(sideInfo.mercenaryApplication) && !isNewApplication.value)

const onNewApplication = () => {
  isNewApplication.value = true
  applicationModel.value = getEmptyApplicationModel()
}

const onCancel = () => {
  emit('close', false)
}

const apply = () => {
  if (applicationModel.value.characterId === null) {
    return
  }
  // @ts-expect-error TODO:
  onApply(applicationModel.value)
}
const { execute } = useCharactersProvider()
execute()
const { user } = useUser()
const { characters } = useCharacters()
</script>

<template>
  <UModal
    :ui="{
      content: 'max-w-xl',
      title: 'gap-4 flex justify-center items-center',
      body: 'space-y-6',
      footer: 'block space-y-6',
    }"
  >
    <template #title>
      {{ $t('strategus.battle.mercenaryApplication.title') }}
    </template>

    <template #body>
      <div class="grid grid-cols-[auto_auto_auto] items-start gap-4">
        <UiDataContent v-if="sideInfo.commander.party" :caption="$t('strategus.battle.commander')" layout="reverse" size="lg">
          <UserMedia :user="sideInfo.commander.party.user" />
        </UiDataContent>

        <UiDataContent :caption="$t('strategus.battle.sideTitle')" :label="side" layout="reverse" size="lg" />

        <UiDataContent v-if="sideInfo.mercenaryApplication" :caption="$t('strategus.battle.mercenaryApplication.statusTitleShort')" layout="reverse" size="lg">
          <div>
            <BattleMercenaryApplicationStatusBadge :application-status="sideInfo.mercenaryApplication.status" />
          </div>
        </UiDataContent>
      </div>

      <UFormField
        :label="$t('strategus.battle.manage.briefing.title')"
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

      <div class="space-y-6">
        <UFormField
          :label="$t('strategus.battle.mercenaryApplication.form.character.label')"
          size="xl"
        >
          <CharacterSelect
            :readonly
            :characters
            :current-character-id="applicationModel.characterId"
            :active-character-id="user!.activeCharacterId"
            @select="(id) => applicationModel.characterId = id"
          />
        </UFormField>

        <UFormField
          :label="$t('strategus.battle.mercenaryApplication.form.note.label')"
          :help="$t('strategus.battle.mercenaryApplication.form.note.help')"
          size="xl"
        >
          <UTextarea
            v-model="applicationModel.note"
            :readonly
            autoresize
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
              {{ $t('strategus.battle.mercenaryApplication.form.wage.label') }}
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
            :readonly
            :max="strategusMercenaryMaxWage"
            :min="0"
            class="w-full"
          />
        </UFormField>
      </div>
    </template>

    <template #footer>
      <div v-if="!readonly" class="flex items-center justify-center gap-4">
        <UButton
          variant="outline"
          size="xl"
          :label="$t('action.cancel')"
          @click="onCancel"
        />

        <AppConfirmActionPopover @confirm="apply">
          <UButton
            size="xl"
            :label="$t('action.send')"
          />
        </AppConfirmActionPopover>
      </div>

      <i18n-t
        v-if="!isNewApplication && sideInfo.mercenaryApplication?.status === BATTLE_MERCENARY_APPLICATION_STATUS.Declined"
        scope="global"
        class="text-center"
        keypath="strategus.battle.mercenaryApplication.create.title"
        tag="div"
      >
        <template #link>
          <ULink
            class="cursor-pointer text-primary"
            @click="onNewApplication"
          >
            {{ $t('strategus.battle.mercenaryApplication.create.link') }}
          </ULink>
        </template>
      </i18n-t>

      <i18n-t
        v-if="sideInfo.mercenaryApplication?.status === BATTLE_MERCENARY_APPLICATION_STATUS.Pending"
        scope="global"
        class="text-center"
        keypath="strategus.battle.mercenaryApplication.delete.title"
        tag="div"
      >
        <template #link>
          <AppConfirmActionPopover @confirm="onDeleteApplication">
            <ULink
              class="
                cursor-pointer text-error
                hover:text-error/80
              "
            >
              {{ $t('strategus.battle.mercenaryApplication.delete.link') }}
            </ULink>
          </AppConfirmActionPopover>
        </template>
      </i18n-t>

      <!-- TODO: -->
      <div
        v-if="sideInfo.mercenaryApplication?.status === BATTLE_MERCENARY_APPLICATION_STATUS.Accepted"
        class="text-center"
      >
        You can
        <AppConfirmActionPopover @confirm="onLeaveFromBattle">
          <ULink
            class="
              cursor-pointer text-error
              hover:text-error/80
            "
          >
            leave the Battle
          </ULink>
          on the {{ side }} side
        </AppConfirmActionPopover>
      </div>
    </template>
  </UModal>
</template>
