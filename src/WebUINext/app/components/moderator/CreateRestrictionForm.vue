<script setup lang="ts">
import type { HumanDuration } from '~/models/datetime'
import type { UserRestrictionCreation } from '~/models/user'

import { UserRestrictionType } from '~/models/user'
import { restrictUser } from '~/services/restriction-service'
import { convertHumanDurationToMs } from '~/utils/date'

const props = defineProps<{ userId: number }>()

const emit = defineEmits<{
  restrictionCreated: []
}>()

const { t } = useI18n()
const { $notify } = useNuxtApp()

const newRestrictionModel = ref<Omit<UserRestrictionCreation, 'restrictedUserId'>>({
  duration: 0,
  publicReason: '',
  reason: '',
  type: UserRestrictionType.Join,
})

const durationModel = ref<HumanDuration>({
  days: 0,
  hours: 0,
  minutes: 0,
})

const durationSeconds = computed(() => convertHumanDurationToMs(durationModel.value))

const addRestriction = async () => {
  await restrictUser({
    ...newRestrictionModel.value,
    duration: durationSeconds.value,
    restrictedUserId: props.userId,
  })

  $notify(t('restriction.create.notify.success'))

  // durationModel.value = {
  //   days: 0,
  //   hours: 0,
  //   minutes: 0,
  // }

  // newRestrictionModel.value = {
  //   duration: 0,
  //   publicReason: '',
  //   reason: '',
  //   type: RestrictionType.Join,
  // }

  emit('restrictionCreated')
}
</script>

<template>
  <form
    class="space-y-8"
    @submit.prevent="addRestriction"
  >
    <OField>
      <OField :label="$t('restriction.create.form.field.type.label')">
        <VDropdown :triggers="['click']">
          <template #default="{ shown }">
            <OButton
              :label="$t(`restriction.type.${newRestrictionModel.type}`)"
              variant="secondary"
              size="lg"
              :icon-right="shown ? 'chevron-up' : 'chevron-down'"
            />
          </template>

          <template #popper="{ hide }">
            <UiDropdownItem
              v-for="rt in Object.keys(UserRestrictionType)"
              :key="rt"
              class="min-w-60 max-w-xs"
            >
              <ORadio
                v-model="newRestrictionModel.type"
                :native-value="rt"
                @change="hide"
              >
                {{ $t(`restriction.type.${rt}`) }}
              </ORadio>
            </UiDropdownItem>
          </template>
        </VDropdown>
      </OField>

      <OField message="Use a duration of 0 to un-restrict">
        <OField :label="$t('restriction.create.form.field.days.label')">
          <OInput
            v-model="durationModel.days"
            size="lg"
            class="w-20"
            required
            type="number"
          />
        </OField>

        <OField :label="$t('restriction.create.form.field.hours.label')">
          <OInput
            v-model="durationModel.hours"
            size="lg"
            class="w-20"
            required
            type="number"
          />
        </OField>

        <OField :label="$t('restriction.create.form.field.minutes.label')">
          <OInput
            v-model="durationModel.minutes"
            size="lg"
            class="w-20"
            required
            type="number"
          />
        </OField>
      </OField>
    </OField>

    <OField :label="$t('restriction.create.form.field.reason.label')">
      <OInput
        v-model="newRestrictionModel.reason"
        placeholder=""
        size="lg"
        class="w-96"
        required
        type="textarea"
        rows="3"
      />
    </OField>

    <OField :label="$t('restriction.create.form.field.publicReason.label')">
      <OInput
        v-model="newRestrictionModel.publicReason"
        placeholder=""
        size="lg"
        class="w-96"
        type="textarea"
        rows="3"
      />
    </OField>

    <OButton
      native-type="submit"
      variant="primary"
      size="lg"
      :label="$t('restriction.create.form.action.submit')"
    />
  </form>
</template>
