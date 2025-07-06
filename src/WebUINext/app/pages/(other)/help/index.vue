<script setup lang="ts"></script>

<template>
  <div class="mx-auto prose prose-invert">
    <h2 class="text-center">
      {{ $t('help.overview.title') }}
    </h2>
    <div v-html="$t('help.overview.intro')" />
  </div>
</template>
