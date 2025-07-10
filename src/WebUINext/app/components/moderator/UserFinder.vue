<script setup lang="ts">
import type { SelectItem, TabsItem } from '@nuxt/ui'

import type { Platform } from '~/models/platform'

import { PLATFORM } from '~/models/platform'
import { platformToIcon } from '~/services/platform-service'
import { getUserById, searchUser } from '~/services/restriction-service'

enum SearchMode {
  Name = 'Name',
  Platform = 'Platform',
  Id = 'Id',
}

const { t } = useI18n()

const activeSearchMode = ref<SearchMode>(SearchMode.Name)

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
    if (activeSearchMode.value === SearchMode.Id && searchByIdModel.value) {
      return [await getUserById(searchByIdModel.value)]
    }

    const payload = activeSearchMode.value === SearchMode.Name
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

const searchModeItems = Object.keys(SearchMode).map<TabsItem>(mode => ({
  label: t(`findUser.mode.${mode}.label`),
  value: mode,
  slot: `${mode}` as const,
}))

const platformItems = computed(() => Object.values(PLATFORM).map<SelectItem>(p => ({
  value: p,
  label: t(`platform.${p}`),
  icon: `crpg:${platformToIcon[p]}`,
})))
</script>

<template>
  <div class="space-y-6">
    <UTabs
      v-model="activeSearchMode"
      :items="searchModeItems"
      size="xl"
      variant="pill"
    >
      <template #Name>
        <UCard>
          <UForm :state="searchByNameModel" @submit="() => { search() }">
            <UButtonGroup>
              <UInput
                v-model="searchByNameModel"
                :placeholder="$t('findUser.mode.Name.field.name.placeholder')"
                size="lg"
                color="neutral"
                icon="crpg:search"
                variant="outline"
                @input="clearUsers"
              />
              <UButton
                size="lg"
                color="neutral"
                variant="subtle"
                :label="$t('action.find')"
                type="submit"
              />
            </UButtonGroup>
          </UForm>
        </UCard>
      </template>

      <template #Platform>
        <UCard>
          <UForm :state="searchByPlatformModel" @submit="() => { search() }">
            <UButtonGroup>
              <USelect
                v-model="searchByPlatformModel.platform"
                size="lg"
                :items="platformItems"
                :icon="`crpg:${platformToIcon[searchByPlatformModel.platform]}`"
                :ui="{
                  content: 'w-auto',
                }"
              />
              <UInput
                v-model="searchByPlatformModel.platformUserId"
                :placeholder="$t('findUser.mode.Platform.field.platformId.placeholder')"
                size="lg"
                color="neutral"
                icon="crpg:search"
                variant="outline"
                @input="clearUsers"
              />
              <UButton
                size="lg"
                color="neutral"
                variant="subtle"
                :label="$t('action.find')"
                @click="() => { search() }"
              />
            </UButtonGroup>
          </UForm>
        </UCard>
      </template>

      <template #Id>
        <UCard>
          <UForm :state="searchByIdModel" @submit="() => { search() }">
            <UButtonGroup>
              <UInput
                v-model="searchByIdModel"
                :placeholder="$t('findUser.mode.Id.field.id.placeholder')"
                size="lg"
                color="neutral"
                icon="crpg:search"
                variant="outline"
                type="number"
                @input="clearUsers"
              />
              <UButton
                size="lg"
                color="neutral"
                variant="subtle"
                :label="$t('action.find')"
                @click="() => { search() }"
              />
            </UButtonGroup>
          </UForm>
        </UCard>
      </template>
    </UTabs>

    <UiLoading :active="searching" />

    <div v-if="users.length">
      <h4 class="mb-4">
        {{ $t('findUser.result.title') }}
      </h4>

      <div class="max-h-[480px] space-y-6 overflow-y-auto">
        <div
          v-for="user in users"
          :key="user.id"
          class="flex items-center gap-2"
        >
          <NuxtLink
            :to="{ name: 'moderator-user-id-restrictions', params: { id: user.id } }"
            class="
              inline-block
              hover:text-content-100
            "
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
