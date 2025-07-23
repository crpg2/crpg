<script setup lang="ts">
interface FaqItem { q: string, a: string }
const faqList = $tm('help.FAQ.list') as FaqItem[]
</script>

<template>
  <div class="prose">
    <h3 class="text-center">
      {{ $t('help.FAQ.title') }}
    </h3>

    <div class="space-y-10">
      <div
        class="text-center"
        v-html="$t('help.FAQ.intro')"
      />

      <UCard v-for="(item, idx) in faqList" :key="idx">
        <template #header>
          <h4 class="!m-0">
            {{ `${idx + 1}. ${$rt(item.q)}` }}
          </h4>
        </template>
        <div class="prose">
          <p class="prose" v-html="$rt(item.a)" />
        </div>
      </UCard>
    </div>
  </div>
</template>
