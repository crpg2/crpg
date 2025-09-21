<script setup lang="ts">
interface FaqItem { q: string, a: string }

const faqList = $tm('help.FAQ.list') as FaqItem[]

const { links } = useAppConfig()
</script>

<template>
  <div class="prose">
    <h2 class="text-center">
      {{ $t('help.FAQ.title') }}
    </h2>

    <div class="space-y-10">
      <div
        class="text-center"
        v-html="$t('help.FAQ.intro', {
          discordLink: links.tipsTricksHelpThread,
        })"
      />

      <UiCard
        v-for="(item, idx) in faqList"
        :key="idx"
        :label="`${idx + 1}. ${$rt(item.q)}`"
        variant="subtle"
      >
        <div class="prose">
          <p class="prose" v-html="$rt(item.a)" />
        </div>
      </UiCard>
    </div>
  </div>
</template>
