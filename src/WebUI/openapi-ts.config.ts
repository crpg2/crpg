import type { OpenApiSchemaObject } from '@hey-api/openapi-ts'

import { defineConfig } from '@hey-api/openapi-ts'
import { fileURLToPath } from 'node:url'

export default defineConfig({
  input: `https://localhost:8000/swagger/v1/swagger.json`, // dev only
  output: 'generated/api',
  plugins: [
    {
      name: '@hey-api/typescript',
      enums: false,
    },
    {
      name: '@hey-api/sdk',
      transformer: '@hey-api/transformers',
      client: '@hey-api/client-nuxt',
    },
    {
      name: '@hey-api/transformers',
      dates: true,
    },
    {
      name: '@hey-api/client-nuxt',
      runtimeConfigPath: fileURLToPath(new URL('app/api.config.ts', import.meta.url)),
    },
  ],
  parser: {
    patch: {
      schemas: {
        CharacterStatisticsViewModel: schema => convertDateTimeToTimestamp(schema, 'playTime'),
        ClanViewModel: schema => convertDateTimeToTimestamp(schema, 'armoryTimeout'),
        UpdateClanCommand: schema => convertDateTimeToTimestamp(schema, 'armoryTimeout'),
        CreateClanCommand: schema => convertDateTimeToTimestamp(schema, 'armoryTimeout'),
        RestrictCommand: schema => convertDateTimeToTimestamp(schema, 'duration'),
        RestrictionPublicViewModel: schema => convertDateTimeToTimestamp(schema, 'duration'),
        RestrictionViewModel: schema => convertDateTimeToTimestamp(schema, 'duration'),
        ItemWeaponComponentViewModel: (schema) => {
          // @ts-expect-error ////
          schema.properties.itemUsage.enum = [
            'long_bow',
            'bow',
            'crossbow',
            'crossbow_light',
            'polearm_couch',
            'polearm_bracing',
            'polearm_pike',
            'polearm',
          ]
        },
        ItemType: (schema) => {
          schema.enum = [...(schema.enum || []), 'Ranged', 'Ammo']
        },
        WeaponClass: (schema) => {
          schema.enum = [...(schema.enum || []), 'Bullets']
        },
        WeaponFlags: (schema) => {
          schema.enum = [...(schema.enum || []), 'CanReloadOnHorseback', 'CantUseOnHorseback']
        },
        ItemMountComponentViewModel: (schema) => {
          // @ts-expect-error ///
          schema.properties.familyType.enum = [0, 1, 2, 3] // Undefined: 0, Horse: 1, Camel: 2, EBA: 3
          // @ts-expect-error ///
          schema.properties.familyType.type = 'integer'
        },
        ItemArmorComponentViewModel: (schema) => {
          // @ts-expect-error ///
          schema.properties.familyType.enum = [0, 1, 2, 3] // Undefined: 0, Horse: 1, Camel: 2, EBA: 3
          // @ts-expect-error ///
          schema.properties.familyType.type = 'integer'
        },
      },
      // TODO:
      // parameters: {
      //   // from: convertDateTimeToString,
      //   region: (parameter) => {
      //     // parameter.schema.type = 'integer'
      //   },
      // },
    },
  },
})

function convertDateTimeToTimestamp(schema: OpenApiSchemaObject.V2_0_X | OpenApiSchemaObject.V3_0_X | OpenApiSchemaObject.V3_1_X, key: string) {
  // @ts-expect-error ///
  delete schema.properties[key].format
  // @ts-expect-error ///
  schema.properties[key].type = 'number'
}
