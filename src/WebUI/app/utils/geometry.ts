import type { Position } from 'geojson'
import type { LatLngExpression } from 'leaflet'

import { LatLng } from 'leaflet'

export const positionToLatLng = (p: Position) => new LatLng(p[1]!, p[0]!)

export const coordinatesToLatLngs = (coordinates: Position[][]): LatLngExpression[][] => [
  coordinates[0]!.map(positionToLatLng),
]
