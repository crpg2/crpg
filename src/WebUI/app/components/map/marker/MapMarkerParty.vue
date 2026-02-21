<script setup lang="ts">
import type { LeafletMouseEvent } from 'leaflet'

import { LCircle, LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'
// import { strategusMaxPartyTroops, strategusMinPartyTroops } from '~root/data/constants.json'

import type { Party, PartyVisible } from '~/models/strategus/party'

import ClanTagIcon from '~/components/clan/ClanTagIcon.vue'
import { positionToLatLng } from '~/utils/geometry'

const { isSelf = false, party } = defineProps<{ party: PartyVisible | Party, isSelf?: boolean }>()

defineEmits<{ click: [LeafletMouseEvent] }>()

// const minRadius = 4
// const maxRadius = 10

// // TODO: FIXME: tweak
// const markerRadius = computed(() => {
//   const troopsRange = strategusMaxPartyTroops - strategusMinPartyTroops // strategusMaxPartyTroops = 300?
//   const sizeFactor = party.troops / troopsRange
//   return minRadius + sizeFactor * (maxRadius - minRadius)
// })

// TODO: FIXME: clan mates,
// const markerColor = computed(() => (isSelf ? '#34d399' : '#ef4444')) // TODO: colors
</script>

<template>
  <div>
    <LMarker
      :lat-lng="positionToLatLng(party.position.coordinates)"
      :options="{ bubblingMouseEvents: false }"
      @click="(e: LeafletMouseEvent) => $emit('click', e)"
    >
      <LIcon
        :icon-size="[32, 32]"
        class-name="flex! items-center justify-center"
      >
        <div class="flex items-center justify-center gap-1 rounded-sm bg-muted/20 py-0.5 pr-1">
          <UserAvatar
            :avatar="party.user.avatar || ''"
            :name="party.user.name || ''"
            size="xl"
            :is-self
          />

          <div class="flex flex-col gap-0.5">
            <ClanTagIcon
              v-if="party.user.clanMembership?.clan"
              :color="party.user.clanMembership.clan.primaryColor" class="size-5"
            />
            <span class="text-center font-medium text-highlighted">{{ $n(party.troops, 'compact') }}</span>
          </div>
        </div>

        <!-- <UBadge variant="soft" color="neutral" size="xs">
          <template #leading>
            <ClanTagIcon
              :color="party?.user.clanMembership?.clan.primaryColor || ''" class="size-3"
            />
          </template>
          {{ $n(party.troops, 'compact') }}
        </UBadge> -->
      </LIcon>

      <LTooltip :options="{ direction: 'top', offset: [0, -24] }">
        <UiDataCell>
          <template #leftContent>
            <UserMedia
              :user="party.user"
              :is-self
            />
          </template>
          <UiDataMedia icon="crpg:member" :label="$n(party.troops)" />
        </UiDataCell>
      </LTooltip>
    </LMarker>

    <LCircle
      v-if="isSelf && 'viewDistance' in party"
      :lat-lng="positionToLatLng(party.position.coordinates)"
      :radius="party.viewDistance"
      :opacity="0"
      :interactive="false"
      :fill-opacity="0.25"
      fill-color="#ccc"
      :options="{
        pmIgnore: true,
      }"
    />
  </div>
</template>
