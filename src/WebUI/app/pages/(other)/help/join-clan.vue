<script setup lang="ts"></script>

<template>
  <div class="prose">
    <h2 class="text-center">
      {{ $t('help.joinClan.title') }}
    </h2>

    <div v-html="$t('help.joinClan.content')" />
  </div>
</template>
