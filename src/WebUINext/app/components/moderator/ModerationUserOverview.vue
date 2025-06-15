<script setup lang="ts">
import type { UserPrivate } from '~/models/user'

defineProps<{
  user: UserPrivate
}>()
</script>

<template>
  <div class="grid grid-cols-2 gap-2 text-2xs">
    <UiSimpleTableRow
      label="Id"
      :value="String(user.id)"
    />

    <UiSimpleTableRow
      :label="$t('character.statistics.expMultiplier.title')"
      :value="$t('character.format.expMultiplier', { multiplier: $n(user.experienceMultiplier) })"
    />

    <UiSimpleTableRow
      label="Region"
      :value="$t(`region.${user.region}`, 0)"
    />

    <UiSimpleTableRow label="Platform">
      {{ user.platform }} {{ user.platformUserId }}
      <UserPlatform
        :platform="user.platform"
        :platform-user-id="user.platformUserId"
        :user-name="user.name"
      />
    </UiSimpleTableRow>

    <UiSimpleTableRow
      v-if="user.clanMembership"
      label="Clan"
    >
      {{ user.clanMembership.clan.name }}
      <UserClan :clan="user.clanMembership.clan" />
    </UiSimpleTableRow>

    <UiSimpleTableRow
      label="Created"
      :value="$d(user.createdAt, 'long')"
    />

    <UiSimpleTableRow
      label="Last activity"
      :value="$d(user.updatedAt, 'long')"
    />

    <UiSimpleTableRow label="Gold">
      <AppCoin :value="user.gold" />
    </UiSimpleTableRow>

    <UiSimpleTableRow label="Heirloom">
      <AppLoom :point="user.heirloomPoints" />
    </UiSimpleTableRow>

    <UiSimpleTableRow label="Donor">
      {{ user.isDonor }}
    </UiSimpleTableRow>
  </div>
</template>
