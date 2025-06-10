<script setup lang="ts">
import type { SelectItem } from '@nuxt/ui'

import type { HumanDuration } from '~/models/datetime'
import type { UserRestrictionCreation } from '~/models/user'

import { UserRestrictionType } from '~/models/user'
import { convertHumanDurationToMs } from '~/utils/date'

defineProps<{ loading: boolean }>()

const emit = defineEmits<{
  submit: [data: Omit<UserRestrictionCreation, 'restrictedUserId'>]
}>()

const durationModel = ref<HumanDuration>({
  days: 0,
  hours: 0,
  minutes: 0,
})

const newRestrictionModel = ref<Omit<UserRestrictionCreation, 'restrictedUserId' | 'duration'>>({
  publicReason: '',
  reason: '',
  type: UserRestrictionType.Join,
})

const onSubmit = () => {
  emit('submit', {
    ...newRestrictionModel.value,
    duration: convertHumanDurationToMs(durationModel.value),
  })
}
</script>

<template>
  <UForm
    :state="newRestrictionModel"
    class="space-y-8"
    @submit="onSubmit"
  >
    <UFormField :label="$t('restriction.create.form.field.type.label')">
      <USelect
        v-model="newRestrictionModel.type"
        :items="Object.values(UserRestrictionType).map<SelectItem>((rt) => ({
          label: $t(`restriction.type.${rt}`),
          value: rt,
        }))"
        class="w-full"
      />
    </UFormField>

    <UFormField help="Use a duration of 0 to un-restrict">
      <div class="grid grid-cols-3 gap-2">
        <UFormField :label="$t('restriction.create.form.field.days.label')" la>
          <UInputNumber
            v-model="durationModel.days"
            :min="0"
          />
        </UFormField>

        <UFormField :label="$t('restriction.create.form.field.hours.label')">
          <UInputNumber
            v-model="durationModel.hours"
            :min="0"
          />
        </UFormField>

        <UFormField :label="$t('restriction.create.form.field.minutes.label')">
          <UInputNumber
            v-model="durationModel.minutes"
            :min="0"
          />
        </UFormField>
      </div>
    </UFormField>

    <UFormField required :label="$t('restriction.create.form.field.reason.label')">
      <UTextarea
        v-model="newRestrictionModel.reason"
        class="w-full"
        required
        autoresize
      />
    </UFormField>

    <UFormField :label="$t('restriction.create.form.field.publicReason.label')">
      <UTextarea
        v-model="newRestrictionModel.publicReason"
        class="w-full"
        autoresize
      />
    </UFormField>

    <UButton
      :loading
      type="submit"
      variant="subtle"
      icon="crpg:plus"
      :label="$t('restriction.create.form.action.submit')"
    />
  </UForm>
</template>
