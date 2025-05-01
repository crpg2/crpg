<script setup lang="ts"></script>

<template>
  <div class="prose prose-invert mx-auto">
    <h2 class="text-center">
      {{ $t('help.joinClan.title') }}
    </h2>
    <div v-html="$t('help.joinClan.content')" />
  </div>
</template>
