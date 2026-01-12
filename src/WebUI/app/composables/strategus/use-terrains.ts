import type { LMap } from '@vue-leaflet/vue-leaflet'

import type { TerrainFeatureCollection } from '~/models/strategus/terrain'

// import type { TerrainFeatureCollection } from '~/models/strategus/terrain'
// import L, { type Map } from 'leaflet'
import { TERRAIN_TYPE } from '~/models/strategus/terrain'
import {
  // addTerrain,
  // deleteTerrain,
  getTerrains,
  mapTerrainsToFeatureCollection,
  terrainColorByType,
  terrainIconByType,
  // terrainToFeatureCollection,
  // updateTerrain,
} from '~/services/strategus/terrain-service'

const terrainDrawControls = [
  {
    type: TERRAIN_TYPE.Barrier,
    title: 'Barrier',
    className: `icon-${terrainIconByType[TERRAIN_TYPE.Barrier]}`,
  },
  {
    type: TERRAIN_TYPE.ShallowWater,
    title: 'Shallow water',
    className: `icon-${terrainIconByType[TERRAIN_TYPE.ShallowWater]}`,
  },
  {
    type: TERRAIN_TYPE.DeepWater,
    title: 'Deep water',
    className: `icon-${terrainIconByType[TERRAIN_TYPE.DeepWater]}`,
  },
  {
    type: TERRAIN_TYPE.SparseForest,
    title: 'Sparse forest',
    className: `icon-${terrainIconByType[TERRAIN_TYPE.SparseForest]}`,
  },
  {
    type: TERRAIN_TYPE.ThickForest,
    title: 'Thick forest',
    className: `icon-${terrainIconByType[TERRAIN_TYPE.ThickForest]}`,
  },
]

export const useTerrains = (map: Ref<typeof LMap | null>) => {
  const { state: terrains, execute: loadTerrains } = useAsyncState(() => getTerrains(), [], {
    immediate: false,
    resetOnExecute: false,
  })

  const terrainsFeatureCollection = computed(() => mapTerrainsToFeatureCollection(terrains.value))

  const terrainVisibility = ref<boolean>(true) // TODO:
  const toggleTerrainVisibilityLayer = () => {
    terrainVisibility.value = !terrainVisibility.value
  }

  // const editMode = ref<boolean>(false)
  // const isEditorInit = ref<boolean>(false)
  // const toggleEditMode = () => {
  //   editMode.value = !editMode.value

  //   if (!isEditorInit.value) {
  //     return createEditControls()
  //   }

  //   (map.value!.leafletObject as Map).pm.toggleControls()
  // }

  // const editType = ref<TerrainType | null>(null)

  // const setEditType = (type: TerrainType) => {
  //   editType.value = type

  //   if (map.value === null) { return }

  //   const color = terrainColorByType[editType.value];

  //   (map.value.leafletObject as Map).pm.setPathOptions({
  //     color,
  //     fillColor: color,
  //   })
  // }

  // TODO: event - ts
  // const onTerrainUpdated = async (event: any) => {
  //   if (event.type === 'pm:create') {
  //     await addTerrain({
  //       type: event.shape,
  //       boundary: event.layer.toGeoJSON().geometry,
  //     })
  //     event.layer.removeFrom(map.value!.leafletObject as Map)
  //     await loadTerrains()
  //   }

  //   if (event.type === 'pm:update') {
  //     await updateTerrain(event.layer.feature.id as number, {
  //       boundary: event.layer.toGeoJSON().geometry,
  //     })
  //     await loadTerrains()
  //   }

  //   if (event.type === 'pm:remove') {
  //     event.layer.off()
  //     await deleteTerrain(event.layer.feature.id as number)
  //     await loadTerrains()
  //   }
  // }

  // const createEditControls = () => {
  //   (map.value!.leafletObject as Map).pm.addControls({
  //     position: 'topleft',
  //     drawCircle: false,
  //     drawMarker: false,
  //     drawCircleMarker: false,
  //     drawPolyline: false,
  //     drawRectangle: false,
  //     drawText: false,
  //     drawPolygon: false,
  //     rotateMode: false,
  //     cutPolygon: false,
  //   })

  //   terrainDrawControls.forEach((dc) => {
  //     (map.value!.leafletObject as Map).pm.Toolbar.copyDrawControl('Polygon', {
  //       name: dc.type,
  //       block: 'draw',
  //       title: dc.title,
  //       className: dc.className,
  //       onClick: () => setEditType(dc.type),
  //     })
  //   });

  //   (map.value!.leafletObject as Map).on('pm:create', onTerrainUpdated);
  //   (map.value!.leafletObject as Map).on('pm:remove', onTerrainUpdated)

  //   L.PM.reInitLayer(map.value!.leafletObject)

  //   isEditorInit.value = true
  // }

  return {
    terrains,
    terrainsFeatureCollection,
    loadTerrains,
    // terrainVisibility,
    // toggleTerrainVisibilityLayer,

    // editMode,
    // toggleEditMode,

    // onTerrainUpdated,
    // createEditControls,
  }
}
