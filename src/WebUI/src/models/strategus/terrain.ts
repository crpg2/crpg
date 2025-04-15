import type { Feature, FeatureCollection, Polygon } from 'geojson'

export enum TerrainType {
  Barrier = 'Barrier',
  ThickForest = 'ThickForest',
  SparseForest = 'SparseForest',
  ShallowWater = 'ShallowWater',
  DeepWater = 'DeepWater',
}

export interface Terrain {
  id: number
  type: TerrainType
  boundary: Polygon
}

export interface TerrainProperties {
  type: TerrainType
}

export type TerrainFeatureCollection = FeatureCollection<Polygon, TerrainProperties>

export type TerrainFeature = Feature<Polygon, TerrainProperties>

export interface TerrainCreation {
  type: TerrainType
  boundary: Polygon
}

export interface TerrainUpdate {
  boundary: Polygon
}
