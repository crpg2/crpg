<script setup lang="ts">
import type { Map } from 'leaflet'

import { LControlZoom, LMap, LTileLayer } from '@vue-leaflet/vue-leaflet'
import { LMarkerClusterGroup } from 'vue-leaflet-markercluster'
import '@geoman-io/leaflet-geoman-free'

import type { PartyCommon } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useMap } from '~/composables/strategus/use-map'
import { useMove } from '~/composables/strategus/use-move'
import { useParty } from '~/composables/strategus/use-party'
import { useSettlements } from '~/composables/strategus/use-settlements'
import { useTerrains } from '~/composables/strategus/use-terrains'
import useMainHeaderHeight from '~/composables/use-main-header-height'
import { MovementTargetType, MovementType } from '~/models/strategus'
import { PartyStatus } from '~/models/strategus/party'
import { inSettlementStatuses } from '~/services/strategus-service'
import { positionToLatLng } from '~/utils/geometry'

definePage({
  meta: {
    layout: 'default',
    noFooter: true,
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const mainHeaderHeight = useMainHeaderHeight()

const {
  center,
  map,
  mapBounds,
  mapOptions,
  maxBounds,
  onMapMoveEnd,
  tileLayerOptions,
  zoom,
} = useMap()

const {
  terrainsFeatureCollection,
  loadTerrains,
  terrainVisibility,
  toggleTerrainVisibilityLayer,
  toggleEditMode,
  onTerrainUpdated,
} = useTerrains(map)

const {
  isRegistered,
  moveParty,
  onRegistered,
  party,
  partySpawn,
  visibleParties,
} = useParty()

const {
  flyToSettlement,
  loadSettlements,
  settlements,
  shownSearch,
  toggleSearch,
  visibleSettlements,
} = useSettlements(map, mapBounds, zoom)

const {
  applyEvents: applyMoveEvents,
  closeMoveDialog,
  isMoveMode,

  moveDialogCoordinates,
  moveDialogMovementTypes,

  moveTarget,
  moveTargetType,

  onStartMove,
  showMoveDialog,
} = useMove(map)

const onPartyClick = (targetParty: PartyCommon) => {
  if (party.value === null) { return }

  showMoveDialog({
    movementTypes: [MovementType.Follow, MovementType.Attack],
    target: targetParty,
    targetType: MovementTargetType.Party,
  })
}

const onSettlementClick = (settlement: SettlementPublic) => {
  if (party.value === null) { return }

  showMoveDialog({
    movementTypes: [MovementType.Move, MovementType.Attack],
    target: settlement,
    targetType: MovementTargetType.Settlement,
  })
}

const onMoveDialogConfirm = (mt: MovementType) => {
  if (moveTarget.value !== null) {
    switch (moveTargetType.value) {
      case MovementTargetType.Party:
        moveParty({
          status:
            mt === MovementType.Follow
              ? PartyStatus.FollowingParty
              : PartyStatus.MovingToAttackParty,
          targetedPartyId: moveTarget.value.id,
        })
        break
      case MovementTargetType.Settlement:
        moveParty({
          status:
            mt === MovementType.Move
              ? PartyStatus.MovingToSettlement
              : PartyStatus.MovingToAttackSettlement,
          targetedSettlementId: moveTarget.value.id,
        })
        break
    }
  }

  closeMoveDialog()
}

const mapIsLoading = ref<boolean>(true)
const onMapReady = async (map: Map) => {
  mapBounds.value = map.getBounds()
  await Promise.all([loadSettlements(), loadTerrains(), partySpawn()])

  if (party.value !== null) {
    map.flyTo(positionToLatLng(party.value.position.coordinates), 5, {
      animate: false,
    })
  }

  applyMoveEvents()
  mapIsLoading.value = false
}
</script>

<template>
  <div :style="{ height: `calc(100vh - ${mainHeaderHeight}px)` }">
    <OLoading
      v-if="mapIsLoading"
      full-page
      active
      icon-size="xl"
    />

    <LMap
      ref="map"
      v-model:zoom="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @ready="onMapReady"
      @move-end="onMapMoveEnd"
    >
      <!-- TODO: FIXME: low res map image -->
      <!-- TODO: FIXME: zIndex -->
      <!-- <LImageOverlay
        url="https://www.printablee.com/postpic/2011/06/blank-100-square-grid-paper_405041.jpg"
        :bounds="maxBounds"
      /> -->
      <LTileLayer v-bind="tileLayerOptions" />
      <LControlZoom position="bottomleft" />

      <ControlSearchToggle
        position="topleft"
        @click="toggleSearch"
      />
      <ControlTerrainVisibilityToggle
        position="topleft"
        @click="toggleTerrainVisibilityLayer"
      />

      <!-- TODO: policy -->
      <ControlTerrainEditToggle
        position="topleft"
        @click="toggleEditMode"
      />

      <ControlMousePosition />
      <ControlLocateParty
        v-if="party !== null"
        :party="party"
        position="bottomleft"
      />

      <LayerTerrain
        v-if="terrainVisibility"
        :data="terrainsFeatureCollection"
        @update="onTerrainUpdated"
      />

      <MarkerParty
        v-if="party !== null"
        :party="party"
        is-self
        @click="onStartMove"
      />
      <PartyMovementLine
        v-if="party !== null && !isMoveMode"
        :party="party"
      />

      <LMarkerClusterGroup
        :show-coverage-on-hover="false"
        chunked-loading
      >
        <MarkerParty
          v-for="visibleParty in visibleParties"
          :key="`party-${visibleParty.id}`"
          :party="visibleParty"
          @click="onPartyClick(visibleParty)"
        />
      </LMarkerClusterGroup>

      <MarkerSettlement
        v-for="settlement in visibleSettlements"
        :key="`settlement-${settlement.id}`"
        :settlement="settlement"
        @click="onSettlementClick(settlement)"
      />

      <DialogMove
        v-if="moveDialogCoordinates !== null"
        :lat-lng="moveDialogCoordinates"
        :movement-types="moveDialogMovementTypes"
        @confirm="onMoveDialogConfirm"
        @cancel="closeMoveDialog"
      />
    </LMap>

    <!-- Dialogs -->
    <div class="absolute left-16 top-6 z-[1000]">
      <!-- TODO: placement, design -->
      <SettlementSearch
        v-if="shownSearch"
        :settlements="settlements"
        @select="flyToSettlement"
      />

      <DialogRegistration
        v-if="!isRegistered"
        @registered="onRegistered"
      />

      <DialogSettlement
        v-if="
          party !== null
            && party.targetedSettlement !== null
            && inSettlementStatuses.has(party.status)
        "
      />
    </div>
  </div>
</template>

<style>
@import 'leaflet/dist/leaflet.css';
@import 'vue-leaflet-markercluster/dist/style.css';
@import '@geoman-io/leaflet-geoman-free/dist/leaflet-geoman.css';

/* TODO: to terrain edit cmp */
.leaflet-pm-toolbar .icon-terrain-barrier {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMiIgaGVpZ2h0PSIzMiIgZmlsbD0ibm9uZSI+PHBhdGggZmlsbD0iIzAwMCIgZD0ibTIxLjc2IDItMTEuNTgyLjAyNEwyIDEwLjIzN2wuMDI0IDExLjU4M0wxMC4yMzcgMzBsMTEuNTgzLS4wMjRMMzAgMjEuNzZsLS4wMjQtMTEuNTgyTDIxLjc2IDJabS0uNDc3IDEuMTcxIDcuNTIgNy40OS4wMjMgMTAuNjIzLTcuMzM0IDcuMzY0LS4xNTUuMTU0LTEwLjYyMy4wMjQtNy41Mi03LjQ4OS0uMDIzLTEwLjYyMyA3LjQ5LTcuNTIgMTAuNjIzLS4wMjNabS0uNDQ2IDEuMDczLTkuNzMuMDItNi44NjMgNi44OTcuMDIgOS43MyA2Ljg5NyA2Ljg2MyA5LjczLS4wMTggNi44NjMtNi45LS4wMTgtOS43MjktNi45LTYuODYzWk03LjIwNSAxMy4yODJjLjMxNiAwIC42NDMuMDMuOTc3LjA3Ny4zNC4wNDguNjg1LjEyIDEuMDQzLjIxNXYxLjIyMWE1LjA3MyA1LjA3MyAwIDAgMC0uOTMtLjMyMSAzLjc1NCAzLjc1NCAwIDAgMC0uODU4LS4xMDhjLS4zNTcgMC0uNjIuMDQ4LS43OS4xNWEuNDg4LjQ4OCAwIDAgMC0uMjU1LjQ1OGMwIC4xNTUuMDU3LjI3NC4xNy4zNjQuMTE3LjA4My4zMjcuMTYuNjMuMjJsLjYzMi4xMjVjLjY0NC4xMzEgMS4xMDMuMzI4IDEuMzc3LjU5LjI2OC4yNjguNDA1LjY0My40MDUgMS4xMjYgMCAuNjM3LS4xOSAxLjExNC0uNTY2IDEuNDMtLjM4MS4zMS0uOTYuNDY1LTEuNzQuNDY1LS4zNjMgMC0uNzMzLS4wMzYtMS4xMDMtLjEwOGE2LjQyOCA2LjQyOCAwIDAgMS0xLjEwOC0uMzFWMTcuNjJjLjM2OS4xOTcuNzI2LjM0NiAxLjA3LjQ0Ny4zNDYuMDk2LjY4Mi4xNDMuOTk4LjE0My4zMjggMCAuNTc4LS4wNTMuNzUtLjE2YS41MTguNTE4IDAgMCAwIC4yNjMtLjQ2NS41Mi41MiAwIDAgMC0uMTc5LS40MjNjLS4xMTktLjA5Ni0uMzUxLS4xODUtLjcwMy0uMjYzbC0uNTc4LS4xMjVjLS41OC0uMTI1LTEuMDA0LS4zMjEtMS4yNzMtLjU5Ni0uMjY2LS4yNjgtLjM5OS0uNjM3LS4zOTktMS4wOTYgMC0uNTc4LjE4Ni0xLjAxOC41NTgtMS4zMjguMzczLS4zMTYuOTA4LS40NzEgMS42MDktLjQ3MVptMTEuMTgzIDBjLjkzIDAgMS42NTYuMjY4IDIuMTg2LjgwNC41My41My43OTMgMS4yNjMuNzkzIDIuMjA1IDAgLjkzNS0uMjYyIDEuNjY4LS43OTMgMi4yMDQtLjUzLjUzLTEuMjU3Ljc5OS0yLjE4Ni43OTktLjkzIDAtMS42NTYtLjI2OS0yLjE4Ny0uNzk5LS41My0uNTM2LS43OTItMS4yNjktLjc5Mi0yLjIwNCAwLS45NDIuMjYyLTEuNjc0Ljc5Mi0yLjIwNS41My0uNTM2IDEuMjU4LS44MDQgMi4xODctLjgwNFptLTguNDU0LjEwN2g1LjMzOHYxLjEyNmgtMS45MTh2NC42NjVoLTEuNDk2di00LjY2NUg5LjkzNFYxMy4zOVptMTIuMjYxIDBoMi40NzljLjczMiAwIDEuMjk4LjE2NyAxLjY5Mi40OTUuMzk5LjMyMi41OTUuNzg2LjU5NSAxLjM5NCAwIC42MDgtLjE5NiAxLjA3OC0uNTk1IDEuNDA2LS4zOTQuMzIyLS45Ni40ODgtMS42OTIuNDg4aC0uOTl2Mi4wMDhoLTEuNDg5di01Ljc5Wm0tMy44MDcuOTc3Yy0uNDU5IDAtLjgxLjE2Ny0xLjA2LjUwNy0uMjUuMzMzLS4zNzYuODEtLjM3NiAxLjQxOCAwIC42MDcuMTI1IDEuMDc4LjM3NSAxLjQxOC4yNS4zMzMuNjAyLjUgMS4wNi41LjQ2IDAgLjgxMS0uMTY3IDEuMDYxLS41LjI1LS4zNC4zNzYtLjgxLjM3Ni0xLjQxOCAwLS42MDgtLjEyNS0xLjA4NS0uMzc2LTEuNDE4LS4yNS0uMzQtLjYwMS0uNTA3LTEuMDYtLjUwN1ptNS4yOTcuMTA4djEuNjE0aC44MjhjLjI5MiAwIC41MTItLjA3MS42NzMtLjIwOC4xNTUtLjE0My4yMzItLjM0Ni4yMzItLjYwMnMtLjA3Ny0uNDU5LS4yMzItLjU5NmMtLjE2LS4xNDMtLjM4MS0uMjA4LS42NzMtLjIwOGgtLjgyOFoiLz48L3N2Zz4=');
}

.leaflet-pm-toolbar .icon-terrain-shallow-water {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMiIgaGVpZ2h0PSIzMiIgZmlsbD0ibm9uZSI+PHBhdGggZmlsbD0iIzAwMCIgZD0iTTE2LjQ5IDIuNTIxQzE0LjYzMSAxMy4zMjUgOS4xIDE1LjUwMSA5LjEgMjMuMDc2YzAgMy44IDMuNzU2IDYuNTk0IDcuMzkgNi41OTQgMy43MTYgMCA3LjI0Ni0yLjYxMyA3LjI0Ni02LjIyIDAtNy4yNzItNS4zMjUtMTAuMTQ1LTcuMjQ2LTIwLjkyOVpNMTIuODQ4IDE2LjA5Yy0xLjc0OCA0LjcyLS4zMiA5LjY2IDMuMzkgMTEuMjEuOTUuMzk4IDEuOTgzLjQ4MyAyLjk5NC4zOTMtNy4yNTggMy4xMTItMTEuNjE0LTQuOTg4LTYuMzg0LTExLjYwM1oiLz48L3N2Zz4=');
}

.leaflet-pm-toolbar .icon-terrain-deep-water {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMiIgaGVpZ2h0PSIzMiIgZmlsbD0ibm9uZSI+PHBhdGggZmlsbD0iIzAwMCIgZD0iTTE0Ljg4OCAyYy43ODcgNC40MiAyLjE2NSA3LjQ2OCAzLjQ5MyAxMC4wNDdsLTcuNzE1IDcuNzE1Yy0uMTA0LTEuNDcuMTE1LTMuMDU1LjY5My00LjYxNS0uMTE3LjE0Ny0uMjI4LjI5NS0uMzM1LjQ0NC0xLjM2OCAxLjkwNS0xLjk0MyAzLjkxMS0xLjg5IDUuNzAzTDcuNzc2IDIyLjY1YTUuNTEgNS41MSAwIDAgMS0uMDQ5LS43MzVjMC0yLjQ2My42MDQtNC4zMzcgMS40NzQtNi4xMzYuNDI4LS44ODQuOTItMS43NSAxLjQzOC0yLjY2IDEuNTYzLTIuNzUgMy4zNS01Ljg5MyA0LjI1LTExLjEyWk0xNy41NDUgMjYuMzljLTIuODYgMS4yMjYtNS4yNTYuNjU1LTYuNzUtLjg4NWwtLjkwNC45MDRjMS4zNiAxLjE4NiAzLjE5NCAxLjg5NiA0Ljk5NyAxLjg5NiAzLjYgMCA3LjAyMS0yLjUzMiA3LjAyMS02LjAyOCAwLTIuNDY4LS42MzMtNC40MTQtMS41MzctNi4zNUwxMi4yIDI0LjFhNS45MzIgNS45MzIgMCAwIDAgMi40NDQgMS45MWMuOTIuMzg0IDEuOTIxLjQ2NiAyLjkwMS4zOFpNMjkgNi4xNThsLTEuNzk0LTEuNzk0TDMgMjguNTY4bDEuNzk0IDEuNzk0TDI5IDYuMTU4WiIvPjwvc3ZnPg==');
}

.leaflet-pm-toolbar .icon-terrain-sparse-forest {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA1MTIgNTEyIiBzdHlsZT0iaGVpZ2h0OiA1MTJweDsgd2lkdGg6IDUxMnB4OyI+PGcgY2xhc3M9IiIgdHJhbnNmb3JtPSJ0cmFuc2xhdGUoMCwwKSIgc3R5bGU9IiI+PHBhdGggZD0iTTI0OS4yOCAxOS4xODh2LjI1Yy0xOC4xMTQgMzguNjM0LTQ1LjA2NSA3Mi4zNi03Ny42ODYgMTAyLjkzN2wzNy43Mi0zLjkzOC01MS4zNDUgNjUuMDMyIDI0LjgxLTcuOTA3LTMzLjYyNCA1NC44NzUgMTYuNTMgOS44NDMtNjUuMjUgOTIuMTU3IDM2LjA5NS4xODgtNTEuNjg2IDgzLjU5NCA2My41NjItOC4xMjYgMTIgMzIuMDk0IDY2LjQzOC0yNS4yODJMMjE1LjUgNDkzLjI4aDUyLjkzOGwtNi41MzItNjguMjE3IDM4LjE4OCAxNi40MDYgMTAuMTg3LTI0Ljc4MyA0NC4yODMgMjAuOTcgNTYuNDA2LTIwLjc1LTM3LjA2NC02NC4wOTQtMTIuNDM3LTIuMjgyIDYuNzggMTcuMTkgNy44NDQgMTkuOTA1LTE5LjkzOC03Ljc4LTUwLjkwNi0xOS45MDhWMzk1LjY4OGwtMTQuMTU2LTguNTk0LTY5LjM3NS00Mi0yMS41OTUgMjEuMjUtMTguMDMgMTcuNzUgMi4xNTUtMjUuMjIgMi4xMjUtMjQuNjU1IDE4LjE4OCAxLjU2IDkuMjE4LTkuMDkyIDUuMTktNS4wOTQgNi4yMTggMy43NSA2MS4zNzUgMzcuMTU2di0yOS45MDZsMTIuNzUgNC45NyA0My43MTggMTcuMDkyLTUuMDkyLTEyLjkwNi02LjE1Ny0xNS42NTYgMTYuNTMzIDMuMDMgNDUuNDY4IDguMzQ1LTM0LjUzLTM4Ljk0LTIzLjYyNSAxNC4wMzMtNi42ODggMy45NjgtNS4xMjUtNS44NzQtMTQuMjgtMTYuNDM3LjIxOCAxLjIxNy0xOC40MDYgMy4yMi01Ljk3LTM0LjMxMy01Ljc1LTMzLjA2MyAyMiAyNS4zNDUgMzEuMTg4IDM1Ljg3NSA0My45MDctMjYuMDNjLTI0LjY3LTE5LjU0My0zOS41MDctMzMuODctNDkuNjU4LTQ4LjgxNGwuODEzIDEyLjY1NiAxLjk3IDMxLTE4Ljc1LTI0Ljc1LTM0LjQ3LTQ1LjQzNy0yMi4yNSA0Ni44MTMtMTMuODQ0IDI5LjEyNS0zLjg0My0zMi4wMzItMy41LTI4Ljg0MyAxNi41MzItMS45NjggMTYuNjI0LTM0Ljk3IDYuNTk0LTEzLjg3NSA5LjI4IDEyLjIyIDI1IDMyLjkzNi0uNzUtMTEuNTMtLjkwNi0xNC4yOCAxMy40NyA0LjkzNkwzNDEuODEgMTg4bC0yNi4xMjUtMzUuMTU2LTU1Ljg0My0yOC44NzUtOC45MzggMjAuMjE4LTkuNjU2IDIxLjkzNy03LjcyLTIyLjY4OC03LjQ2OC0yMS44NzUgMTYuOTctNS43OCAzLjcxOC04LjQzOCA0LTkuMTI1IDguODQ0IDQuNTkzIDQ5LjM3NSAyNS41MyAxNi40NjctNS41NjJjLTQzLjQyLTM0LjMxLTY0LjYzLTY4Ljg4Ni03Ni4xNTYtMTAzLjU5M3oiIGZpbGw9IiMwMDAiIGZpbGwtb3BhY2l0eT0iMSI+PC9wYXRoPjwvZz48L3N2Zz4=');
}

.leaflet-pm-toolbar .icon-terrain-thick-forest {
  background-image: url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMiIgaGVpZ2h0PSIzMiIgZmlsbD0ibm9uZSI+PHBhdGggZmlsbD0iIzAwMCIgZD0iTTEwLjk4MyAyYy0uNjA4Ljk2Ni0xLjI5NiAxLjk3LTIuNDEyIDIuOTJsLjA4MS4wNzggMS4zNjMtLjAyNC0uMjA0LjYyYy0uMDMuMDg4LS4wNjIuMTc2LS4wOTcuMjYzLjE2Mi4xMTIuMzMyLjIyNC41MTMuMzM1bDEuNDUuODg3LTIuNjYxLS4wNDZhNi4yODIgNi4yODIgMCAwIDEtLjM2NS40MmMuNTA5LjY2NyAxLjI3NSAxLjMxNSAyLjIzOCAxLjg5N2wtLjcxOC0uMDIxLS4wMDEuMzI4Yy40OTQuMzY4IDEuMDMuNzIgMS41NzIgMS4wNyAxLjA0Ny0uNjcgMS44MzktMS40MTggMi4yODYtMi4xNzdsLTMuMzE5LjA1NyAxLjQ1LS44ODdhOS42MjcgOS42MjcgMCAwIDAgMS4zMzUtLjk3NmMtLjI5NS0uMzE3LS41Mi0uNjM0LS42NTEtLjk2NmwtLjIzNC0uNTkyIDEuNzIxLS4xMDZjLTEuMzA5LS44MzgtMi42MjMtMS43Ny0zLjM0Ny0zLjA4Wm0xMy41NzUuNzY1Yy0uMzc4Ljc1LS44MDMgMS42NzktMS4zNjggMi41ODIuNjMyIDEuMDYzIDEuNDEzIDIuMDgzIDIuNzA2IDMuMDMxbDEuMTgyLjg2Ny0zLjAzOC0uMDYzYy40MDIuNzAyIDEuMTUzIDEuNTU0IDIgMi4zMTcgMS4xMDkuOTk4IDIuMzY5IDEuODg1IDMuMTIgMi4zMWwuNjU2LjM3di0uODA4Yy0uOTkzLS42OS0xLjcyMi0xLjQ5Mi0yLjEwMS0yLjQ0MWwtLjIzOC0uNTk2IDIuNTIzLS4xNWMtLjU3My0uMjUyLTEuMTQzLS40NzgtMS42NjItLjY5Ny0uNDkyLS4yMDgtLjk0LS40MTItMS4zMTUtLjY1LS4zNzYtLjIzOS0uNy0uNTE4LS44Ni0uOTIzbC0uMjI3LS41NzYgMi4wODgtLjE4NGMtMS40MDMtMS4xNy0yLjczMy0yLjgyMS0zLjQ2Ni00LjM4OVptLTE4LjU0NS4zMzJDNS4yOSA0LjQwOCAzLjk3NSA1LjM0IDIuNjY2IDYuMTc3bDEuNzIuMTA2LS4yMzMuNTkzYy0uMjc2LjcwMS0uOTczIDEuMzk2LTEuOTIgMi4xMzNhNzguNTkzIDc4LjU5MyAwIDAgMC0uMDIyIDEuMDI2YzAgLjAwMy4wMTEuMDY0LjAxNy4xODEuMDAyLjA0My4wMDIuMTg0LjAwNC4yNTNsLjguMDQ2di4wMzFsLS4yMjYuNTY2Yy0uMDY5LjE3MS0uMTQ1LjM1MS0uMjQxLjUzNGwuNjY3LS40MDhjMi4xNjItMS4zMjMgMi45MTctMi43ODYgMy45LTQuMzNsLS4wMTQuMzY5LjI1OS0uMTA5YTQuMTg0IDQuMTg0IDAgMCAxLS4xOTItLjQ3N2wtLjIwNC0uNjIgMS41MzguMDI3Yy0xLjE3My0uOTczLTEuODgtMi4wMDctMi41MDYtM1ptMTAuMzYgMS41MjljLS42MjYuOTkzLTEuMzM0IDIuMDI3LTIuNTA3IDNsMS41MzgtLjAyNi0uMjA0LjYyYy0uMzg1IDEuMTY3LTEuMzE3IDIuMTkyLTIuNTk2IDMuMDZsMS4wMzYuNjcgMS4wNTguMDQtLjI0LjYwOGMtLjEzMy4zMzktLjMxLjY2LS41Mi45NjNsLjUwNS0uMzU4YzEuMDM3LS43MzQgMi4xMjgtMS41OTEgMi45OTctMi40MDMuNTgtLjU0MiAxLjA0LTEuMDcyIDEuMzM2LTEuNWwtMi45MjQtLjIxNS45NTQtLjczOWMuNi0uNDY0IDEuMjE0LS45MjIgMS43OS0xLjM5OS0uODk2LS42NS0xLjcwOS0xLjM5LTIuMjI0LTIuMzIxWm01LjM4NS4wNGMtLjczOSAxLjU1LTIuMDk3IDIuNjYzLTMuNDIgMy42NzRsMS44MTYuMTMzLS4xODUuNTYzYy0uMjYuNzkzLS45ODMgMS41ODYtMS44OTcgMi40NGEyNy41MjEgMjcuNTIxIDAgMCAxLTIuMDc3IDEuNzMybDIuODUxLjIwMi0uMTg4LjU2NmMtLjY1MiAxLjk1Ni0yLjcxIDMuMjk0LTQuMzg3IDQuMzc0Ljc4LjQ2NiAxLjYwMS44OTMgMi4zMDQgMS4yOTEgMy45NTQgMS4wOSA4LjU0LjQwNCAxMS42MjUtLjkyNS0uNzYzLS40MzgtMS4zOC0uODQyLTEuODk0LTEuMzMyLS42ODQtLjY0OS0xLjE2Mi0xLjQ0NC0xLjUyNy0yLjU1NmwtLjE3NC0uNTMzIDMuMDA3LS4zODJhMjIuMTIgMjIuMTIgMCAwIDEtMi4xOTEtMS43MjZjLTEuMTY0LTEuMDQ4LTIuMjIyLTIuMTk5LTIuNTQxLTMuMzZsLS4xNjUtLjU5OCAxLjYxMy4wMzNjLTEuMjI2LTEuMTYxLTEuOTQxLTIuNC0yLjU3LTMuNTk3Wk03LjQ0NSA4LjE0M2MtLjYyNS45OTQtMS4zMzMgMi4wMjgtMi41MDYgM2wxLjUzOC0uMDI2LS4yMDQuNjJjLS40NzcgMS40NDYtMS43OTIgMi42NzItMy41NjYgMy42NTVsMy4wNjQuMTE2LS4yNC42MDdDNC44OTMgMTcuNzQyIDMuODEgMTkuMTggMiAyMC4yNThjNC4yNzUgMS4zMDYgOS4wNiAxLjgyNiAxMy40LS4yMDctLjc0Ny0uNDA4LTEuNTU3LS44NDgtMi4zMzQtMS4zNjMtMS4wNDUtLjY5My0xLjk5OC0xLjQ5LTIuNDEzLTIuNTNsLS4yMzgtLjU5NSAyLjgtLjE2NmEyNy4wNTMgMjcuMDUzIDAgMCAxLTEuOTQxLTEuMzUzYy0uOTQtLjczMy0xLjY4Mi0xLjM5NC0xLjk2OS0yLjEyMWwtLjIzNC0uNTkzIDEuNzIxLS4xMDZjLTEuMzA5LS44MzctMi42MjItMS43NjktMy4zNDctMy4wOFptLTIuMzQ0IDMuOTI0LTIuODM2LjA0OS0uMDI5LjAzOGMtLjAxNiAxLjI0NC0uMDI2IDEuOTMyLS4wMjkgMi40NTIgMS4zNS0uNzU1IDIuMzY1LTEuNjQgMi44OTQtMi41MzlabTYuMTE0LjcxOGMuMTkuMTcxLjM5OS4zNDkuNjI4LjUyOC4yMDQuMTU4LjQyLjMyLjY0NC40OC4zMTUtLjMuNTg0LS42MS43OTMtLjkzbC0yLjA2NS0uMDc4Wm0yLjQxIDEuMTgyYTYuNTA1IDYuNTA1IDAgMCAxLS4zNDMuMzc2Yy41MzYuMzU4IDEuMDk2LjcwOSAxLjY0MiAxLjAzbDEuMDIuNjA0Yy42NTctLjU0NSAxLjIyNC0xLjEyNCAxLjU3NS0xLjczNGwtMy44OTMtLjI3NlptMTYuMTkuNTk4LTMuOTU2LjUwM2MuMjcuNjYyLjU4MSAxLjE0MyAxIDEuNTYybC0uMDEyLS41NTZhMTYuODQ1IDE2Ljg0NSAwIDAgMCAyLjk3NC0uNDI2Yy0uMDA0LS40MTUtLjAwNS0uNzE4LS4wMDUtMS4wODNaTTE1LjcgMTYuMTc3bC0zLjg0Mi4yMjhjLjIyNi4zLjUyNi41OTcuODc0Ljg4Ni4zNy4wNzQuNzQ2LjE0MiAxLjEyNS4yMDJhMjguMzU3IDI4LjM1NyAwIDAgMCAxLjg0My0xLjMxNlptLTEyLjcxNS4xNTMtLjAxNyAyLjA2MWE2LjkxNiA2LjkxNiAwIDAgMCAxLjQzNy0yLjAwN2wtMS40Mi0uMDU0Wm0yMy45NTIgMy44NWMtLjgyNi4yNjUtMS43MDguNDg2LTIuNjI2LjY1bC4zMjEgNy42NDdoMi40ODdsLS4xODItOC4yOTdabS0xMC41MjEuNDA1Yy0uMTU0LjA4MS0uMzEuMTYtLjQ2NC4yMzRsLS4wNTQgNi42MTJoMy4wNDhsLS4xNC02LjM5M2ExNS42IDE1LjYgMCAwIDEtMi4zOS0uNDUzWm02Ljk3NC4zODdjLS44NzMuMTE1LTEuNzcuMTc3LTIuNjcyLjE3M2wtLjAyMiA4LjA4NmgzLjA0MWwtLjM0Ny04LjI1OVptLTIwLjQ0Ny41M0wyLjg5IDI4LjFoMi41NmwuMTE1LTYuMDRhMjcuMzc3IDI3LjM3NyAwIDAgMS0yLjYyMi0uNTU5Wm0xMC4xMS4zNjJjLS42Ny4xNjYtMS4zNDQuMjg2LTIuMDIuMzY0bC4yMjcgMy44M2gxLjkxM2wtLjEyLTQuMTk0Wm0tNi41NjQuMzI2LS4xNDUgNy42NyA0LjIwNi0uMTMyLS40NC03LjQxOWEyMC4wNDUgMjAuMDQ1IDAgMCAxLTMuNjIxLS4xMTlaIi8+PC9zdmc+');
}

.leaflet-interactive {
  @apply select-none outline-0;
}

.leaflet-container.mode-create {
  @apply cursor-crosshair;
}

.leaflet-right .leaflet-control {
  @apply mb-2;
}

.leaflet-container .leaflet-control-attribution {
  @apply mb-0;
}

/* TODO: colors */
.marker-cluster-small {
  @apply bg-primary;
}

.marker-cluster-small div {
  @apply bg-primary-hover text-content-100;
}

.marker-cluster-medium {
  @apply bg-primary;
}

.marker-cluster-medium div {
  @apply bg-primary-hover text-content-100;
}

.marker-cluster-large {
  @apply bg-primary;
}

.marker-cluster-large div {
  @apply bg-primary-hover text-content-100;
}
</style>
