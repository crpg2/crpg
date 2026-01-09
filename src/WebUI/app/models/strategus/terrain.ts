import type { TerrainType as _TerrainType, GeoJsonPolygon } from '#api'
import type { ValueOf } from 'type-fest'

export const TERRAIN_TYPE = {
  Plain: 'Plain',
  Barrier: 'Barrier',
  ThickForest: 'ThickForest',
  SparseForest: 'SparseForest',
  ShallowWater: 'ShallowWater',
  DeepWater: 'DeepWater',
} as const satisfies Record<_TerrainType, _TerrainType>

export type TerrainType = ValueOf<typeof TERRAIN_TYPE>

export interface Terrain {
  id: number
  type: TerrainType
  boundary: GeoJsonPolygon
}
