<script setup lang="ts">
import type { UserPublic } from '~/models/user'

import { useUserStore } from '~/stores/user'

const { borrower, lender } = defineProps<{
  lender: UserPublic
  borrower?: UserPublic | null
}>()

const { user } = toRefs(useUserStore())
</script>

<template>
  <div class="group flex items-center">
    <UTooltip>
      <UserMedia
        :user="lender"
        hidden-platform
        hidden-title
        hidden-clan
        size="sm"
        :is-self="user!.id === lender.id"
        :class="{ 'transition duration-200 ease-in group-hover:-translate-x-3': borrower }"
      />
      <template #content>
        <i18n-t
          scope="global"
          keypath="clan.armory.item.lender.tooltip.title"
          tag="div"
          class="flex items-center gap-2"
        >
          <template #user>
            <UserMedia
              :user="lender"
              :is-self="user!.id === lender.id"
              hidden-platform
              hidden-clan
            />
          </template>
        </i18n-t>
      </template>
    </UTooltip>

    <UTooltip v-if="borrower">
      <UserMedia
        :user="borrower"
        hidden-platform
        hidden-title
        hidden-clan
        size="sm"
        class="relative z-10 -ml-2.5"
        :is-self="user!.id === borrower.id"
      />
      <template #content>
        <div class="flex items-center gap-2">
          <i18n-t
            scope="global"
            keypath="clan.armory.item.borrower.tooltip.title"
            tag="div"
            class="flex items-center gap-2"
          >
            <template #user>
              <UserMedia
                class="max-w-40"
                :user="borrower"
                :is-self="user!.id === borrower.id"
                hidden-platform
                hidden-clan
              />
            </template>
          </i18n-t>
        </div>
      </template>
    </UTooltip>
  </div>
</template>
