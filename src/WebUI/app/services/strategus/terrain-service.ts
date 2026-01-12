import {
  getTerrains as _getTerrains,
} from '#api/sdk.gen'

import type { Terrain, TerrainFeatureCollection, TerrainType } from '~/models/strategus/terrain'

import { TERRAIN_TYPE } from '~/models/strategus/terrain'
// TODO: colors
export const terrainColorByType: Record<TerrainType, string> = {
  [TERRAIN_TYPE.Barrier]: '#d03c3c ',
  [TERRAIN_TYPE.SparseForest]: '#22c55e',
  [TERRAIN_TYPE.ThickForest]: '#166534',
  [TERRAIN_TYPE.ShallowWater]: '#60a5fa',
  [TERRAIN_TYPE.DeepWater]: '#1e40af',
  [TERRAIN_TYPE.Plain]: 'tomato',
}

export const terrainIconByType: Record<TerrainType, string> = {
  [TERRAIN_TYPE.Barrier]: 'terrain-barrier ',
  [TERRAIN_TYPE.SparseForest]: 'terrain-sparse-forest',
  [TERRAIN_TYPE.ThickForest]: 'terrain-thick-forest',
  [TERRAIN_TYPE.ShallowWater]: 'terrain-shallow-water',
  [TERRAIN_TYPE.DeepWater]: 'terrain-deep-water',
  [TERRAIN_TYPE.Plain]: 'terrain-plain',
}

export const mapTerrainsToFeatureCollection = (terrains: Terrain[]): TerrainFeatureCollection => ({
  type: 'FeatureCollection',
  features: terrains.map(t => ({
    type: 'Feature',
    id: t.id,
    geometry: t.boundary,
    properties: {
      type: t.type,
    },
  })),
})

export const getTerrains = async (): Promise<Terrain[]> => (await _getTerrains({})).data!

// export const addTerrain = (payload: TerrainCreation) => post<Terrain>('/terrains', payload)

// export const updateTerrain = (id: number, payload: TerrainUpdate) =>
//   put<Terrain>(`/terrains/${id}`, payload)

// export const deleteTerrain = (id: number) => del(`/terrains/${id}`)
