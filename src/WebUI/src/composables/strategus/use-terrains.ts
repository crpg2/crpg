import type { LMap } from '@vue-leaflet/vue-leaflet'

import L, { type Map } from 'leaflet'

import { type TerrainFeatureCollection, TerrainType } from '~/models/strategus/terrain'
import {
  addTerrain,
  deleteTerrain,
  getTerrains,
  terrainColorByType,
  terrainIconByType,
  terrainToFeatureCollection,
  updateTerrain,
} from '~/services/strategus-service/terrain'

const terrainDrawControls = [
  {
    type: TerrainType.Barrier,
    title: 'Barrier',
    className: `icon-${terrainIconByType[TerrainType.Barrier]}`,
  },
  {
    type: TerrainType.ShallowWater,
    title: 'Shallow water',
    className: `icon-${terrainIconByType[TerrainType.ShallowWater]}`,
  },
  {
    type: TerrainType.DeepWater,
    title: 'Deep water',
    className: `icon-${terrainIconByType[TerrainType.DeepWater]}`,
  },
  {
    type: TerrainType.SparseForest,
    title: 'Sparse forest',
    className: `icon-${terrainIconByType[TerrainType.SparseForest]}`,
  },
  {
    type: TerrainType.ThickForest,
    title: 'Thick forest',
    className: `icon-${terrainIconByType[TerrainType.ThickForest]}`,
  },
]

export const useTerrains = (map: Ref<typeof LMap | null>) => {
  const { state: terrains, execute: loadTerrains } = useAsyncState(() => getTerrains(), [], {
    immediate: false,
    resetOnExecute: false,
  })

  const terrainsFeatureCollection = computed<TerrainFeatureCollection>(() =>
    terrainToFeatureCollection(terrains.value),
  )

  const terrainVisibility = ref<boolean>(true) // TODO:
  const toggleTerrainVisibilityLayer = () => {
    terrainVisibility.value = !terrainVisibility.value
  }

  const editMode = ref<boolean>(false)
  const isEditorInit = ref<boolean>(false)
  const toggleEditMode = () => {
    editMode.value = !editMode.value

    if (!isEditorInit.value) {
      return createEditControls()
    }

    (map.value!.leafletObject as Map).pm.toggleControls()
  }

  const editType = ref<TerrainType | null>(null)

  const setEditType = (type: TerrainType) => {
    editType.value = type

    if (map.value === null) { return }

    const color = terrainColorByType[editType.value];

    (map.value.leafletObject as Map).pm.setPathOptions({
      color,
      fillColor: color,
    })
  }

  // TODO: event - ts
  const onTerrainUpdated = async (event: any) => {
    if (event.type === 'pm:create') {
      await addTerrain({
        type: event.shape,
        boundary: event.layer.toGeoJSON().geometry,
      })
      event.layer.removeFrom(map.value!.leafletObject as Map)
      await loadTerrains()
    }

    if (event.type === 'pm:update') {
      await updateTerrain(event.layer.feature.id as number, {
        boundary: event.layer.toGeoJSON().geometry,
      })
      await loadTerrains()
    }

    if (event.type === 'pm:remove') {
      event.layer.off()
      await deleteTerrain(event.layer.feature.id as number)
      await loadTerrains()
    }
  }

  const createEditControls = () => {
    (map.value!.leafletObject as Map).pm.addControls({
      position: 'topleft',
      drawCircle: false,
      drawMarker: false,
      drawCircleMarker: false,
      drawPolyline: false,
      drawRectangle: false,
      drawText: false,
      drawPolygon: false,
      rotateMode: false,
      cutPolygon: false,
    })

    terrainDrawControls.forEach((dc) => {
      (map.value!.leafletObject as Map).pm.Toolbar.copyDrawControl('Polygon', {
        name: dc.type,
        block: 'draw',
        title: dc.title,
        className: dc.className,
        onClick: () => setEditType(dc.type),
      })
    });

    (map.value!.leafletObject as Map).on('pm:create', onTerrainUpdated);
    (map.value!.leafletObject as Map).on('pm:remove', onTerrainUpdated)

    L.PM.reInitLayer(map.value!.leafletObject)

    isEditorInit.value = true
  }

  return {
    terrains,
    terrainsFeatureCollection,
    loadTerrains,
    terrainVisibility,
    toggleTerrainVisibilityLayer,

    editMode,
    toggleEditMode,

    onTerrainUpdated,
    createEditControls,
  }
}
