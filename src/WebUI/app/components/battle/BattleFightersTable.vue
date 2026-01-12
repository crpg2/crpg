<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { AppConfirmActionPopover, UButton, UIcon, UiDataCell, UiDataContent, UserMedia, UTooltip } from '#components'

import type { BattleFighter } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'

const { fighters, canKickByFighter, canLeaveByFighter } = defineProps<{
  fighters: BattleFighter[]
  loading?: boolean
  canKickByFighter: (fighter: BattleFighter) => boolean
  canLeaveByFighter: (fighter: BattleFighter) => boolean
}>()

const emit = defineEmits<{
  kick: [fighterId: number]
  leave: [fighterId: number]
}>()

const { n } = useI18n()
const { user } = useUser()

const columns = computed<TableColumn<BattleFighter>[]>(() => [
  {
    accessorKey: 'user',
    cell: ({ row }) => h(UiDataCell, {}, {
      leftContent: () => h(UserMedia, {
        user: row.original.party!.user,
        isSelf: row.original.party?.user.id === user.value!.id,
      }),
      default: () =>
        canKickByFighter(row.original)
          ? h(AppConfirmActionPopover, { onConfirm: () => emit('kick', row.original.id) }, () =>
              h(UTooltip, { text: 'TODO: kick' }, () =>
                h(UButton, {
                  variant: 'ghost',
                  color: 'error',
                  icon: 'crpg:kick',
                })))
          : canLeaveByFighter(row.original)
            ? h(AppConfirmActionPopover, { onConfirm: () => emit('leave', row.original.id) }, () =>
                h(UTooltip, { text: 'TODO: retreat' }, () =>
                  h(UButton, {
                    variant: 'ghost',
                    color: 'error',
                    icon: 'crpg:retreat',
                  })))
            : null,
    }),
  },
  {
    accessorFn: row => row.party!.troops,
    id: 'party_troops',
    cell: ({ row }) => h(UTooltip, { text: 'TODO: troops/participantSlots' }, () =>
      h(UiDataCell, null, {
        leftContent: () => h(UIcon, { name: 'crpg:member', class: 'size-5' }),
        default: () => h(UiDataContent, {
          label: n(row.original.party!.troops),
          caption: n(row.original.participantSlots),
        }),
      })),
    meta: {
      class: {
        td: tw`w-24`,
      },
    },
  },
])
</script>

<template>
  <div>
    <UTable
      class="rounded-md border border-muted"
      :loading
      :data="fighters"
      :columns
      :ui="{
        thead: 'hidden',
      }"
    >
      <template #empty>
        <UiResultNotFound message="TODO: loading" />
      </template>
    </UTable>
  </div>
</template>
