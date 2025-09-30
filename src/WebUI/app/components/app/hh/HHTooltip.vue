<script setup lang="ts">
import type { Region } from '~/models/region'

defineProps<{ region: Region }>()
</script>

<template>
  <UTooltip>
    <slot />

    <template #content>
      <i18n-t
        scope="global"
        keypath="hh.tooltip-content.template"
        tag="div"
        class="prose prose-invert"
      >
        <template #title>
          <h3>🎉 {{ $t('hh.tooltip-content.title') }} 🎉</h3>
        </template>

        <template #desc>
          <i18n-t
            scope="global"
            keypath="hh.tooltip-content.desc"
            tag="p"
          >
            <template #descHighlight>
              <span class="text-primary">
                {{ $t('hh.tooltip-content.descHighlight') }}
              </span>
            </template>
          </i18n-t>
        </template>

        <template #region>
          <i18n-t
            scope="global"
            keypath="hh.tooltip-content.region"
            tag="p"
          >
            <template #currentRegion>
              <span class="text-primary">
                {{ $t(`region.${region}`, 0) }}
              </span>
            </template>
          </i18n-t>
        </template>

        <template #regionDetect>
          <i18n-t
            scope="global"
            keypath="hh.tooltip-content.regionDetect"
            tag="p"
          />
        </template>
      </i18n-t>
    </template>
  </UTooltip>
</template>
