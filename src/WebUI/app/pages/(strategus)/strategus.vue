<script setup lang="ts">
import type { Map } from 'leaflet'

import { LMap, LTileLayer } from '@vue-leaflet/vue-leaflet'
import L from 'leaflet'
import 'leaflet-textpath'
import { LMarkerClusterGroup } from 'vue-leaflet-markercluster'
import '@geoman-io/leaflet-geoman-free'

import { useMainHeader } from '~/composables/app/use-main-header'
import { useMap, useMapContextProvider } from '~/composables/strategus/use-map'
import { useParty, usePartyState } from '~/composables/strategus/use-party'
import { usePartyMove } from '~/composables/strategus/use-party-move'
import { usePartyOrder } from '~/composables/strategus/use-party-order'
import { useSettlements } from '~/composables/strategus/use-settlements'
import { useTerrains } from '~/composables/strategus/use-terrains'
import { SomeRole } from '~/models/role'
import { TERRAIN_TYPE } from '~/models/strategus/terrain'
import { getSelfUpdate, shouldPartyBeInBattle, shouldPartyBeInSettlement } from '~/services/strategus/party-service'
import { terrainIconByType } from '~/services/strategus/terrain-service'

definePageMeta({
  roles: SomeRole,
  layoutOptions: {
    noFooter: true,
    noStickyHeader: true,
  },
  middleware: [
    async (to) => {
      try {
        const { partyState, setPartyState } = usePartyState(false)

        if (!partyState.value) {
          const partyRes = await getSelfUpdate()

          if (partyRes?.errors !== null || partyRes.data === null) {
            return navigateTo({ name: 'join-to-strategus' })
          }

          setPartyState(partyRes.data)
        }

        const { party } = partyState.value

        if (shouldPartyBeInSettlement(party) && !to.meta.groups?.includes('strategussettlement')) {
          return navigateTo({
            name: 'strategus-settlement-id',
            params: { id: party.currentSettlement!.id },
          })
        }

        if (shouldPartyBeInBattle(party) && !to.meta.groups?.includes('strategusbattle')) {
          return navigateTo({
            name: 'strategus-battle-id',
            params: { id: party.currentBattle!.id },
          })
        }
      }
      // eslint-disable-next-line unused-imports/no-unused-vars
      catch (_error) {
        return navigateTo({ name: 'join-to-strategus' })
      }
    },
  ],
})

const { mainHeaderHeight } = useMainHeader()

const {
  center,
  map,
  mapBounds,
  mapOptions,
  maxBounds,
  onMapMoveEnd,
  tileLayerOptions,
  zoom,
  zoomIn,
  zoomOut,
} = useMap()

useMapContextProvider({ zoom })

const { partyState, partySpawn } = useParty()

const flyToSelfParty = () => {
  (map.value!.leafletObject as Map).flyTo(positionToLatLng(partyState.value.party.position.coordinates), 5, {
    animate: false,
  })
}

const {
  isMoveMode,
  applyMoveEvents,
  onStartMove,
} = usePartyMove(map)

const {
  orderTargetCoordinates,
  availableOrders,
  confirmOrderDialog,
  closeOrderDialog,
  openPartyOrderDialog,
  openSettlementOrderDialog,
  openBattleOrderDialog,
} = usePartyOrder()

const {
  // flyToSettlement,
  // loadSettlements,
  // settlements,
  // shownSearch,
  // toggleSearch,
  visibleSettlements,
} = useSettlements(map, mapBounds, zoom)

const {
  terrainsFeatureCollection,
  terrainLayerVisibility,
  toggleTerrainLayerVisibility,
  onTerrainUpdate,
  editMode,
  toggleEditMode,
  activeTool,
  terrainDrawType,
  toggleDrawTool,
  toggleEditTool,
  toggleRemovalTool,
  toggleDragTool,
} = useTerrains(map)

const mapControlsRef = useTemplateRef('mapControls')
const mapPartyProfileRef = useTemplateRef('mapPartyProfile')

const isMapRdy = ref(false)
const [onMapReady] = useAsyncCallback(async (map: Map) => {
  const partyPane = map.createPane('partyPane')
  partyPane.style.zIndex = '650' // TODO: to const
  mapBounds.value = map.getBounds()
  flyToSelfParty()
  applyMoveEvents()
  partySpawn()
  isMapRdy.value = true

  if (mapControlsRef.value) {
    L.DomEvent.disableClickPropagation(mapControlsRef.value)
    L.DomEvent.disableScrollPropagation(mapControlsRef.value)
  }

  if (mapPartyProfileRef.value) {
    L.DomEvent.disableClickPropagation(mapPartyProfileRef.value.$el)
    L.DomEvent.disableScrollPropagation(mapPartyProfileRef.value.$el)
  }
})
</script>

<template>
  <div :style="{ height: `calc(100vh - ${mainHeaderHeight}px)` }">
    <LMap
      ref="map"
      v-model:zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @ready="onMapReady"
      @move-end="onMapMoveEnd"
    >
      <LTileLayer v-bind="tileLayerOptions" :z-index="1" />

      <!-- <ControlSearchToggle
        position="topleft"
        @click="toggleSearch"
      />
     -->

      <MapDialogOrder
        v-if="orderTargetCoordinates !== null"
        :lat-lng="orderTargetCoordinates"
        :orders="availableOrders"
        @confirm="confirmOrderDialog"
        @cancel="closeOrderDialog"
      />

      <MapMarkerBattle
        v-for="battle in partyState.visibleBattles"
        :key="`battle-${battle.id}`"
        :battle
        @click="openBattleOrderDialog(battle)"
      />

      <MapMarkerSettlement
        v-for="settlement in visibleSettlements"
        :key="`settlement-${settlement.id}`"
        :settlement
        @click="openSettlementOrderDialog(settlement)"
      />

      <LMarkerClusterGroup
        v-if="isMapRdy"
        chunked-loading
        cluster-pane="partyPane"
        spiderfy-on-max-zoom
        :show-coverage-on-hover="false"
        zoom-to-bounds-on-click
      >
        <MapMarkerParty
          v-for="party in partyState.visibleParties"
          :key="`party-${party.id}`"
          :party
          @click="openPartyOrderDialog(party)"
        />
      </LMarkerClusterGroup>

      <MapMarkerParty
        :party="partyState.party"
        is-self
        @click="onStartMove"
      />

      <MapLayerTerrain
        v-if="terrainLayerVisibility"
        :data="terrainsFeatureCollection"
        @update="onTerrainUpdate"
      />

      <MapPartyMovementPolyline
        v-if="!isMoveMode"
        :party="partyState.party"
      />

      <MapPartyProfile
        ref="mapPartyProfile"
        class="absolute top-6 left-6 z-1000 cursor-auto"
        :party="partyState.party"
        @locate="flyToSelfParty"
      />

      <!-- <MapControlMousePosition position="bottomright" />
       -->
      <div
        ref="mapControls"
        class="absolute bottom-12 left-1/2 z-1000 -translate-x-1/2 cursor-auto"
      >
        <UFieldGroup size="xl">
          <UButton
            icon="i-lucide-minus" variant="subtle" color="neutral"
            @click="() => {
              zoomOut()
            }"
          />

          <UButton
            icon="i-lucide-plus" variant="subtle" color="neutral"
            @click="() => {
              zoomIn()
            }"
          />

          <UTooltip :text="$t('strategus.control.terrainLayerToggle')">
            <UButton
              variant="soft"
              color="neutral"
              icon="i-lucide-layers-2"
              :class="[terrainLayerVisibility && 'bg-accented']"
              @click="() => {
                toggleTerrainLayerVisibility()
              }"
            />
          </UTooltip>

          <UTooltip
            :open="editMode"
            :content="{
              side: 'top',
            }"
            :ui="{
              content: 'p-0 bg-transparent ring-0',
            }"
          >
            <UButton
              variant="subtle"
              color="neutral"
              icon="i-lucide-edit"
              :class="[editMode && 'bg-accented']"
              @click="() => {
                toggleEditMode()
              }"
            />
            <template #content>
              <div class="flex items-center gap-2">
                <UFieldGroup size="xl">
                  <UButton
                    v-for="terrain in Object.values(TERRAIN_TYPE).filter(t => t !== 'Plain')"
                    :key="terrain"
                    variant="subtle" color="neutral"
                    :icon="terrainIconByType[terrain]"
                    :class="[activeTool === 'draw' && terrainDrawType === terrain && 'bg-accented']"
                    @click="() => toggleDrawTool(terrain)"
                  />
                </UFieldGroup>

                <UFieldGroup size="xl">
                  <UButton
                    variant="subtle" color="neutral"
                    icon="i-lucide-spline-pointer"
                    :class="[activeTool === 'edit' && 'bg-accented']"
                    @click="toggleEditTool"
                  />

                  <UButton
                    variant="subtle" color="neutral"
                    icon="i-lucide-move"
                    :class="[activeTool === 'drag' && 'bg-accented']"
                    @click="() => toggleDragTool()"
                  />

                  <UButton
                    variant="subtle" color="neutral"
                    icon="i-lucide-eraser"
                    :class="[activeTool === 'remove' && 'bg-accented']"
                    @click="() => toggleRemovalTool()"
                  />
                </UFieldGroup>
              </div>
            </template>
          </UTooltip>

          <UTooltip :text="$t('strategus.control.searchBySettlements')">
            <UButton
              variant="soft"
              color="neutral"
              icon="i-lucide-search"
              @click="() => {}"
            />
          </UTooltip>
        </UFieldGroup>
        <!--  -->
      </div>
    </LMap>

    <NuxtPage class="absolute top-6 left-6 z-1000" :style="{ height: `calc(100% - 3rem)` }" />
  </div>
</template>

<style>
@reference "~/assets/css/main.css";

@import 'leaflet/dist/leaflet.css';
@import 'vue-leaflet-markercluster/dist/style.css';
@import '@geoman-io/leaflet-geoman-free/dist/leaflet-geoman.css';

.leaflet-interactive {
  @apply select-none outline-0;
}

.leaflet-container a {
  color: inherit;
}

.leaflet-tooltip {
  @apply rounded-sm border-transparent bg-elevated/80 text-highlighted shadow-none! p-2;
}

/* tooltip arrow */
.leaflet-tooltip-top:before {
  @apply border-t-default/80;
}

.leaflet-tooltip-bottom:before {
  @apply border-b-default/80;
}

.leaflet-tooltip-left:before {
  @apply border-l-default/80;
}

.leaflet-tooltip-right:before {
  @apply border-r-default/80;
}

.leaflet-popup-content-wrapper,
.leaflet-popup-tip {
  @apply border-transparent bg-elevated/80 text-highlighted shadow-none!;
}

.leaflet-container a.leaflet-popup-close-button,
.leaflet-container a.leaflet-popup-close-button:hover,
.leaflet-container a.leaflet-popup-close-button:focus {
  @apply text-highlighted;
}

.marker-cluster {
  @apply bg-secondary/40;
}

.marker-cluster > div {
  @apply bg-secondary/60 text-highlighted;
}
</style>
