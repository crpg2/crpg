<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { SortingState, VisibilityState } from '@tanstack/vue-table'

import { AppConfirmActionPopover, UButton, UiGridColumnHeader, UiTextView, UserMedia } from '#components'

import type { BattleFighter } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'

const { fighters, canManage, canManageByFighter } = defineProps<{
  fighters: BattleFighter[]
  loading: boolean
  canManage: boolean
  canManageByFighter: (fighter: BattleFighter) => boolean
}>()

const emit = defineEmits<{
  kick: [participantId: number]
  // showMercinaryApplication: [mercinaryApplicationId: number]
}>()

const { user } = useUser()

const table = useTemplateRef('table')
const columns = computed<TableColumn<BattleFighter>[]>(() => [

  {
    accessorKey: 'user',
    header: 'Fighter',
    // header: () => h('div', { class: 'inline-flex gap-1.5' }, [
    //   h('span', null, 'Participants'),
    //   h('span', null, '·'),
    //   h(UiTextView, { variant: 'caption' }, {
    //     default: () => `${participants.length}/${totalParticipantSlots}`,
    //   }),
    // ]),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.party!.user,
      isSelf: row.original.party?.user.id === user.value!.id,
    }),
  },
  {
    accessorFn: row => row.party!.troops,
    id: 'party_troops',
    header: ({ column }) => h(UiGridColumnHeader, {
      label: 'Troops',
      withSort: true,
      sorted: column.getIsSorted(),
      onSort: () => column.toggleSorting(column.getIsSorted() === 'asc'),
    }),
  },
  {
    header: '',
    accessorFn: row => row.party!.user.region,
    id: 'region',
  },
  {
    id: 'actions',
    header: '',
    cell: ({ row }) => {
      if (!canManageByFighter(row.original)) {
        return null
      }

      return h('div', { class: 'flex gap-1.5 justify-end' }, [
        // ...(row.original.type === BATTLE_PARTICIPANT_TYPE.Mercenary && row.original.mercenaryApplicationId
        //   ? [
        //       h(UButton, {
        //         variant: 'subtle',
        //         icon: 'crpg:mercenary',
        //         onClick: () => emit('showMercinaryApplication', row.original.mercenaryApplicationId!),
        //       }),
        //     ]
        //   : []),
        h(AppConfirmActionPopover, {
          onConfirm: () => emit('kick', row.original.id),
        }, {
          default: () => h(UButton, {
            variant: 'subtle',
            color: 'error',
            icon: 'crpg:close',
          }),
        }),
      ])
    },
  },
])

const columnVisibility = computed<VisibilityState>(() => {
  return {
    // type: false,
    ...(!canManage && {
      actions: false,
    }),
  }
})

const sorting = ref<SortingState>([
  // { id: 'type', desc: true },
])
</script>

<template>
  <div>
    <UTable
      ref="table"
      v-model:column-visibility="columnVisibility"
      v-model:sorting="sorting"
      class="relative rounded-md border border-muted"
      :loading
      :data="fighters"
      :columns
    >
      <template #empty>
        <UiResultNotFound message="TODO:" />
      </template>
    </UTable>
  </div>
</template>
