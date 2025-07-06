<script setup lang="ts"></script>

<template>
  <div class="mx-auto prose prose-invert">
    <h2 class="text-center">
      {{ $t('help.buildSupport.title') }}
    </h2>
    <div v-html="$t('help.buildSupport.content')" />
  </div>
</template>
