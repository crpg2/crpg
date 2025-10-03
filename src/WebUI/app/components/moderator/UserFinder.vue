<script setup lang="ts">
import type { SelectItem, TabsItem } from '@nuxt/ui'
import type { ValueOf } from 'type-fest'

import type { Platform } from '~/models/platform'

import { AVAILABLE_PLATFORM, PLATFORM } from '~/models/platform'
import { platformToIcon } from '~/services/platform-service'
import { getUserById, searchUser } from '~/services/restriction-service'

const SEARCH_MODE = {
  Name: 'Name',
  Platform: 'Platform',
  Id: 'Id',
} as const
type SearchMode = ValueOf<typeof SEARCH_MODE>

const { t } = useI18n()

const activeSearchMode = ref<SearchMode>(SEARCH_MODE.Name)

const searchByNameModel = ref<string>('')
const searchByIdModel = ref<number | null>(null)
const searchByPlatformModel = ref<{ platform: Platform, platformUserId: string }>({
  platform: PLATFORM.Steam,
  platformUserId: '',
})

const {
  state: users,
  execute: search,
  isLoading: searching,
  isReady: loadedUsers,
} = useAsyncState(
  async () => {
    if (activeSearchMode.value === SEARCH_MODE.Id && searchByIdModel.value) {
      return [await getUserById(searchByIdModel.value)]
    }

    const payload = activeSearchMode.value === SEARCH_MODE.Name
      ? { name: searchByNameModel.value }
      : {
          platform: searchByPlatformModel.value.platform,
          platformUserId: searchByPlatformModel.value.platformUserId,
        }

    return await searchUser(payload)
  },
  [],
  { immediate: false },
)

const clearUsers = () => {
  users.value = []
  loadedUsers.value = false
}

const searchModeItems = Object.keys(SEARCH_MODE).map<TabsItem>(mode => ({
  label: t(`findUser.mode.${mode}.label`),
  value: mode,
  slot: `${mode}` as const,
}))

const platformItems = computed(() => AVAILABLE_PLATFORM.map<SelectItem>(p => ({
  value: p,
  label: t(`platform.${p}`),
  icon: `crpg:${platformToIcon[p]}`,
})))
</script>

<template>
  <div class="space-y-6">
    <UCard>
      <template #header>
        <UTabs
          v-model="activeSearchMode"
          :items="searchModeItems"
          color="neutral"
          :content="false"
        />
      </template>

      <UForm
        v-if="activeSearchMode === SEARCH_MODE.Name"
        :state="searchByNameModel"
        @submit="() => { search() }"
      >
        <UFieldGroup class="w-full" size="xl">
          <UInput
            v-model="searchByNameModel"
            :placeholder="$t('findUser.mode.Name.field.name.placeholder')"
            color="neutral"
            class="w-full"
            icon="crpg:search"
            :loading="searching"
            variant="outline"
            @input="clearUsers"
          />
          <UButton
            color="neutral"
            variant="subtle"
            :label="$t('action.find')"
            type="submit"
          />
        </UFieldGroup>
      </UForm>

      <UForm
        v-else-if="activeSearchMode === SEARCH_MODE.Platform"
        :state="searchByPlatformModel"
        @submit="() => { search() }"
      >
        <UFieldGroup class="w-full" size="xl">
          <USelect
            v-model="searchByPlatformModel.platform"
            color="neutral"
            :items="platformItems"
            :icon="`crpg:${platformToIcon[searchByPlatformModel.platform]}`"
            :ui="{
              content: 'w-auto',
            }"
          />
          <UInput
            v-model="searchByPlatformModel.platformUserId"
            :placeholder="$t('findUser.mode.Platform.field.platformId.placeholder')"
            color="neutral"
            icon="crpg:search"
            :loading="searching"
            variant="outline"
            class="w-full"
            @input="clearUsers"
          />
          <UButton
            color="neutral"
            variant="subtle"
            :label="$t('action.find')"
            @click="() => { search() }"
          />
        </UFieldGroup>
      </UForm>

      <UForm
        v-else-if="activeSearchMode === SEARCH_MODE.Id"
        :state="searchByIdModel"
        @submit="() => { search() }"
      >
        <UFieldGroup class="w-full" size="xl">
          <UInput
            v-model="searchByIdModel"
            :placeholder="$t('findUser.mode.Id.field.id.placeholder')"
            color="neutral"
            icon="crpg:search"
            :loading="searching"
            variant="outline"
            class="w-full"
            type="number"
            @input="clearUsers"
          />
          <UButton
            color="neutral"
            variant="subtle"
            :label="$t('action.find')"
            @click="() => { search() }"
          />
        </UFieldGroup>
      </UForm>
    </UCard>

    <div v-if="users.length">
      <UiTextView variant="h4" margin-bottom>
        {{ $t('findUser.result.title') }}
      </UiTextView>

      <div class="grid max-h-[480px] grid-cols-2 gap-x-8 gap-y-6 overflow-y-auto">
        <div
          v-for="user in users"
          :key="user.id"
          class="flex items-center justify-between gap-2"
        >
          <NuxtLink
            :to="{ name: 'moderator-user-id-restrictions', params: { id: user.id } }"
            class="inline-block"
          >
            <UserMedia :user="user" />
          </NuxtLink>

          <slot
            name="user-prepend"
            v-bind="user"
          />
        </div>
      </div>
    </div>
  </div>
</template>
