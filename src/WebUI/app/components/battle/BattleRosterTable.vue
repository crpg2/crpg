<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { SortingState, VisibilityState } from '@tanstack/vue-table'

import { AppConfirmActionPopover, UButton, UiTextView, UserMedia } from '#components'

import type { BattleParticipant } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_PARTICIPANT_TYPE } from '~/models/strategus/battle'

const { participants, canManage, canManageByParticipant, totalParticipantSlots } = defineProps<{
  participants: BattleParticipant[]
  loading: boolean
  canManage: boolean
  totalParticipantSlots: number
  canManageByParticipant: (participant: BattleParticipant) => boolean
}>()

const emit = defineEmits<{
  kickParticipant: [participantId: number]
  showMercinaryApplication: [mercinaryApplicationId: number]
}>()

const { user } = useUser()

const table = useTemplateRef('table')
const columns = computed<TableColumn<BattleParticipant>[]>(() => [

  {
    accessorKey: 'user',
    header: () => h('div', { class: 'inline-flex gap-1.5' }, [
      h('span', null, 'Participants'),
      h('span', null, '·'),
      h(UiTextView, { variant: 'caption' }, {
        default: () => `${participants.length}/${totalParticipantSlots}`,
      }),
    ]),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.user,
      isSelf: row.original.user.id === user.value!.id,
    }),
  },
  // {
  //   accessorKey: 'character',
  //   header: '',
  //   cell: ({ row }) => h(CharacterMedia, {
  //     character: row.original.character,
  //     hiddenName: true,
  //   }),
  // },
  {
    header: '',
    accessorFn: row => row.user.region,
    id: 'region',
  },
  {
    accessorKey: 'type',
    header: '',
    id: 'type',
  },
  // TODO: только для phase END
  // {
  //   header: 'K',
  //   cell: () => '12',
  // },
  // {
  //   header: 'D',
  //   cell: () => '7',
  // },
  // {
  //   header: 'A',
  //   cell: () => '11',
  // },
  {
    id: 'actions',
    header: '',
    cell: ({ row }) => {
      if (!canManageByParticipant(row.original)) {
        return null
      }

      return h('div', { class: 'flex gap-1.5 justify-end' }, [
        ...(row.original.type === BATTLE_PARTICIPANT_TYPE.Mercenary && row.original.mercenaryApplicationId
          ? [
              h(UButton, {
                variant: 'subtle',
                icon: 'crpg:mercenary',
                onClick: () => emit('showMercinaryApplication', row.original.mercenaryApplicationId!),
              }),
            ]
          : []),
        h(AppConfirmActionPopover, {
          onConfirm: () => emit('kickParticipant', row.original.id),
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
    type: false,
    ...(!canManage && {
      actions: false,
    }),
  }
})

const sorting = ref<SortingState>([
  { id: 'type', desc: true },
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
      :data="participants"
      :columns
    >
      <template #empty>
        <UiResultNotFound message="TODO:" />
      </template>
    </UTable>
  </div>
</template>
