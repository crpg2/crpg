import type { LMap } from '@vue-leaflet/vue-leaflet'
import type { Map } from 'leaflet'

import type { TerrainType } from '~/models/strategus/terrain'

import {
  addTerrain,
  deleteTerrain,
  getTerrains,
  mapTerrainsToFeatureCollection,
  terrainColorByType,
  updateTerrain,
} from '~/services/strategus/terrain-service'

export type Tool = 'draw' | 'edit' | 'drag' | 'remove'

export const useTerrains = (map: Ref<typeof LMap | null>) => {
  const mapInstance = computed(() => map.value!.leafletObject as Map)

  const {
    state: terrains,
    executeImmediate: loadTerrains,
  } = useAsyncState(getTerrains, [], {
    resetOnExecute: false,
  })

  const terrainsFeatureCollection = computed(() => mapTerrainsToFeatureCollection(terrains.value))

  const [terrainLayerVisibility, toggleTerrainLayerVisibility] = useToggle(true)

  const editMode = ref<boolean>(false)
  const isEditorInit = ref<boolean>(false)
  // const editedLayer = ref<L.Layer | null>(null)
  const terrainDrawType = ref<TerrainType | null>(null)

  const [onTerrainUpdate] = useAsyncCallback(async (event: any) => {
    if (event.type === 'pm:create' && terrainDrawType.value) {
      await addTerrain({
        type: terrainDrawType.value,
        boundary: event.layer.toGeoJSON().geometry,
      })
      event.layer.removeFrom(mapInstance.value)
      await loadTerrains()
    }

    if (event.type === 'pm:update' || event.type === 'pm:dragend' || event.type === 'pm:edit') {
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
  }, {
    pageLoading: true,
  })

  const initEditor = () => {
    mapInstance.value.pm.removeControls()

    mapInstance.value.on('pm:create', onTerrainUpdate)
    mapInstance.value.on('pm:remove', onTerrainUpdate)

    // mapInstance.on('pm:keyevent', (e) => {
    //   // e.event.
    //   console.log(e)
    // })

    mapInstance.value.pm.setGlobalOptions({
      exitModeOnEscape: true,
    })

    // mapInstance.on('pm:editstart', (e) => {
    //   editedLayer.value = e.propagatedFrom
    // })
    isEditorInit.value = true
  }

  const activeTool = ref<Tool | null>(null)

  const toggleEditMode = () => {
    editMode.value = !editMode.value

    if (!isEditorInit.value) {
      initEditor()
      return
    }

    if (!editMode.value) {
      activeTool.value = null
      mapInstance.value.pm.globalEditModeEnabled() && mapInstance.value.pm.disableGlobalEditMode()
      mapInstance.value.pm.globalRemovalModeEnabled() && mapInstance.value.pm.disableGlobalRemovalMode()
      mapInstance.value.pm.globalDragModeEnabled() && mapInstance.value.pm.disableGlobalDragMode()
    }
  }

  const toggleDrawTool = (type: TerrainType) => {
    if (mapInstance.value.pm.globalDrawModeEnabled()) {
      mapInstance.value.pm.disableDraw('Polygon')
      activeTool.value = null
      return
    }

    mapInstance.value.pm.enableDraw('Polygon')

    terrainDrawType.value = type
    const color = terrainColorByType[type]
    mapInstance.value.pm.setPathOptions({ color, fillColor: color })

    activeTool.value = 'draw'
  }

  const toggleEditTool = () => {
    if (mapInstance.value.pm.globalEditModeEnabled()) {
      activeTool.value = null
      mapInstance.value.pm.disableGlobalEditMode()
      return
    }

    activeTool.value = 'edit'
    mapInstance.value.pm.enableGlobalEditMode()
  }

  const toggleDragTool = () => {
    if (mapInstance.value.pm.globalDragModeEnabled()) {
      activeTool.value = null
      mapInstance.value.pm.disableGlobalDragMode()
      return
    }

    activeTool.value = 'drag'
    mapInstance.value.pm.enableGlobalDragMode()
  }

  const toggleRemovalTool = () => {
    if (mapInstance.value.pm.globalRemovalModeEnabled()) {
      activeTool.value = null
      mapInstance.value.pm.disableGlobalRemovalMode()
      return
    }

    activeTool.value = 'remove'
    mapInstance.value.pm.enableGlobalRemovalMode()
  }

  return {
    terrains,
    loadTerrains,
    terrainsFeatureCollection,

    terrainLayerVisibility,
    toggleTerrainLayerVisibility,

    editMode,
    toggleEditMode,
    onTerrainUpdate,

    activeTool,

    toggleDrawTool,
    terrainDrawType,

    toggleEditTool,
    toggleRemovalTool,
    toggleDragTool,
  }
}
