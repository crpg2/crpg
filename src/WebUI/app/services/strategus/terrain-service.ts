import type { Polygon } from 'geojson'

import {
  getTerrains as _getTerrains,
  deleteTerrainsById,
  postTerrains,
  putTerrainsById,
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
  [TERRAIN_TYPE.Plain]: 'crpg:terrain-plain',
  [TERRAIN_TYPE.Barrier]: 'crpg:terrain-barrier',
  [TERRAIN_TYPE.SparseForest]: 'crpg:terrain-sparse-forest',
  [TERRAIN_TYPE.ThickForest]: 'crpg:terrain-thick-forest',
  [TERRAIN_TYPE.ShallowWater]: 'crpg:terrain-shallow-water',
  [TERRAIN_TYPE.DeepWater]: 'crpg:terrain-deep-water',
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

export interface TerrainCreation {
  type: TerrainType
  boundary: Polygon
}

export interface TerrainUpdate {
  boundary: Polygon
}

export const getTerrains = async (): Promise<Terrain[]> => (await _getTerrains({})).data!

export const addTerrain = (payload: TerrainCreation) => postTerrains({ body: payload })

export const updateTerrain = (id: number, payload: TerrainUpdate) => putTerrainsById({ path: { id }, body: payload })

export const deleteTerrain = (id: number) => deleteTerrainsById({ path: { id } })
