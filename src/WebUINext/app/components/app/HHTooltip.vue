<script setup lang="ts">
import type { Region } from '~/models/region'

defineProps<{ region: Region }>()
</script>

<template>
  <VTooltip>
    <template #default="slotProps">
      <slot
        name="default"
        v-bind="slotProps"
      />
    </template>

    <template #popper>
      <i18n-t
        scope="global"
        keypath="hh.tooltip-content.template"
        tag="div"
        class="prose prose-invert max-w-xs pr-6"
      >
        <template #title>
          <h5>🎉 {{ $t('hh.tooltip-content.title') }} 🎉</h5>
        </template>

        <template #desc>
          <i18n-t
            scope="global"
            keypath="hh.tooltip-content.desc"
            tag="p"
          >
            <template #descHighlight>
              <span class="text-content-100">
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
              <span class="text-content-100">
                {{ $t(`region.${region}`, 0) }}
              </span>
            </template>
          </i18n-t>
        </template>
      </i18n-t>
    </template>
  </VTooltip>
</template>
