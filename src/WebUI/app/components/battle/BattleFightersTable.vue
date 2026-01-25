<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { VisibilityState } from '@tanstack/vue-table'

import { AppConfirmActionPopover, UButton, UIcon, UiDataCell, UiDataContent, UserMedia, UTooltip } from '#components'

import type { BattleFighter } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'

const { fighters, showActions, canKickByFighter, canLeaveByFighter } = defineProps<{
  fighters: BattleFighter[]
  loading: boolean
  showActions: boolean
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
    header: '',
    cell: ({ row }) => h(UserMedia, {
      user: row.original.party!.user,
      isSelf: row.original.party?.user.id === user.value!.id,
    }),
  },
  {
    accessorFn: row => row.party!.troops,
    id: 'party_troops',
    header: '',
    cell: ({ row }) => h(UTooltip, { text: 'TODO: troops/participantSlots' }, () =>
      h(UiDataCell, null, {
        leftContent: () => h(UIcon, { name: 'crpg:member', class: 'size-5' }),
        default: () => h(UiDataContent, {
          label: n(row.original.party!.troops * 100),
          caption: n(row.original.participantSlots),
        }),
      })),
  },
  {
    id: 'actions',
    header: '',
    cell: ({ row }) => {
      return h('div', { class: 'flex gap-1.5 justify-end' }, [
        ...(canKickByFighter(row.original)
          ? [
              h(AppConfirmActionPopover, { onConfirm: () => emit('kick', row.original.id) }, () =>
                h(UTooltip, { text: 'TODO: kick' }, () =>
                  h(UButton, {
                    variant: 'ghost',
                    color: 'error',
                    icon: 'crpg:kick',
                  }))),
            ]
          : []),
        ...(canLeaveByFighter(row.original)
          ? [
              h(AppConfirmActionPopover, { onConfirm: () => emit('leave', row.original.id) }, () =>
                h(UTooltip, { text: 'TODO: retreat' }, () =>
                  h(UButton, {
                    variant: 'ghost',
                    color: 'error',
                    icon: 'crpg:retreat',
                  }))),
            ]
          : []),
      ])
    },
  },
])

const columnVisibility = computed<VisibilityState>(() => {
  return {
    ...(!showActions && {
      actions: false,
    }),
  }
})
</script>

<template>
  <div>
    <UTable
      v-model:column-visibility="columnVisibility"
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
