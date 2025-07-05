<script setup lang="ts">
import { UiDecorSeparator } from '#components'

const { t } = useI18n()

const qaCount = 12

// TODO: refactoring
const qaList = computed(() =>
  Array.from({ length: qaCount })
    .fill(0)
    .map((_, idx) => ({
      a: t(`help.FAQ.list[${idx}].a`),
      q: t(`help.FAQ.list[${idx}].q`),
    }))
    .filter((qa, idx) => qa.q !== `help.FAQ.list[${idx}].q`),
)
</script>

<template>
  <div class="mx-auto prose prose-invert">
    <h2 class="text-center">
      {{ $t('help.FAQ.title') }}
    </h2>

    <div class="space-y-10">
      <div
        class="text-center"
        v-html="$t('help.FAQ.intro')"
      />

      <UiDecorSeparator />

      <UCard
        v-for="({ q, a }, idx) in qaList" :key="idx"
        variant="outline"
      >
        <template #header>
          <h3 class="!m-0">
            {{ `${idx + 1}. ${q}` }}
          </h3>
        </template>
        <div v-html="a" />
      </UCard>
    </div>
  </div>
</template>
