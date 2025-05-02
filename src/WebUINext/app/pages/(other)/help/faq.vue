<script setup lang="ts">
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
  <div class="prose prose-invert mx-auto">
    <h2 class="text-center">
      {{ $t('help.FAQ.title') }}
    </h2>

    <div class="space-y-10">
      <div
        class="text-center"
        v-html="$t('help.FAQ.intro')"
      />

      <UiDivider />

      <UiFormGroup
        v-for="({ q, a }, idx) in qaList"
        :key="idx"
        :label="`${idx + 1}. ${q}`"
        :collapsable="false"
      >
        <div v-html="a" />
      </UiFormGroup>
    </div>
  </div>
</template>
