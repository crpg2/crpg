<script setup lang="ts"></script>

<template>
  <div class="prose">
    <h3 class="text-center">
      {{ $t('help.joinClan.title') }}
    </h3>

    <div v-html="$t('help.joinClan.content')" />
  </div>
</template>
