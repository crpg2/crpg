<script setup lang="ts">
import { ModeratorRestrictionsTable } from '#components'

import { getUserRestrictions } from '~/services/restriction-service'

const props = defineProps<{ id: string }>()

definePageMeta({
  props: true,
})

const { execute: loadRestrictions, state: restrictions } = useAsyncState(
  () => getUserRestrictions(Number(props.id)),
  [],
)
</script>

<template>
  <div>
    <div class="mb-8 flex items-center gap-4">
      <h2 class="text-lg">
        {{ $t('restriction.user.history') }}
      </h2>

      <UiModal
        closable
        :auto-hide="false"
      >
        <OButton
          native-type="submit"
          variant="primary"
          size="sm"
          :label="$t('restriction.create.form.title')"
        />
        <template #popper="{ hide }">
          <div class="space-y-6 p-6">
            <div class="pb-4 text-center text-xl text-content-100">
              {{ $t('restriction.create.form.title') }}
            </div>

            <ModeratorCreateRestrictionForm
              :user-id="Number(props.id)"
              @restriction-created="() => {
                hide();
                loadRestrictions();
              }"
            />
          </div>
        </template>
      </UiModal>
    </div>

    <ModeratorRestrictionsTable
      :restrictions="restrictions"
      :hidden-cols="['restrictedUser']"
    />
  </div>
</template>
