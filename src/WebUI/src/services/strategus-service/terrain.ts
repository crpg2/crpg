import type { Terrain, TerrainCreation, TerrainFeatureCollection, TerrainUpdate } from '~/models/strategus/terrain'

import { TerrainType } from '~/models/strategus/terrain'
import { del, get, post, put } from '~/services/crpg-client'

// TODO: colors
export const terrainColorByType: Record<TerrainType, string> = {
  [TerrainType.Barrier]: '#d03c3c ',
  [TerrainType.SparseForest]: '#22c55e',
  [TerrainType.ThickForest]: '#166534',
  [TerrainType.ShallowWater]: '#60a5fa',
  [TerrainType.DeepWater]: '#1e40af',
}

export const terrainIconByType: Record<TerrainType, string> = {
  [TerrainType.Barrier]: 'terrain-barrier ',
  [TerrainType.SparseForest]: 'terrain-sparse-forest',
  [TerrainType.ThickForest]: 'terrain-thick-forest',
  [TerrainType.ShallowWater]: 'terrain-shallow-water',
  [TerrainType.DeepWater]: 'terrain-deep-water',
}

export const terrainToFeatureCollection = (terrains: Terrain[]): TerrainFeatureCollection => ({
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

export const getTerrains = () => get<Terrain[]>('/terrains')

export const addTerrain = (payload: TerrainCreation) => post<Terrain>('/terrains', payload)

export const updateTerrain = (id: number, payload: TerrainUpdate) =>
  put<Terrain>(`/terrains/${id}`, payload)

export const deleteTerrain = (id: number) => del(`/terrains/${id}`)
