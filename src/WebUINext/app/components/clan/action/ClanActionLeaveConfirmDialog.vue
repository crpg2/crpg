<script setup lang="ts">
import type { Clan } from '~/models/clan'

defineProps<{
  clan: Clan
}>()

defineEmits<{
  close: [boolean]
}>()
</script>

<template>
  <AppConfirmActionDialog
    :title="$t('clan.member.leave.dialog.title')"
    :confirm="clan.name"
    @close="(res) => $emit('close', res)"
  >
    <template #description>
      <i18n-t
        scope="global"
        keypath="clan.member.leave.dialog.desc"
      >
        <template #clan>
          <UserClan :clan />
        </template>
      </i18n-t>
    </template>
  </AppConfirmActionDialog>
</template>
