<script setup lang="ts">
// ({
//   meta: {
//     // bg: 'background-2.webp',
//     // middleware: 'activeCharacterRedirect',
//     // roles: ['User', 'Moderator', 'Admin'],
//   },
// })
</script>

<template>
  <div class="flex size-full min-h-[500px] items-center justify-center">
    <div class="space-y-8">
      <Divider />

      <div class="prose prose-invert text-center">
        <h3 class="text-xl text-content-100">
          {{ $t('character.empty.title') }}
        </h3>
        <div>{{ $t('character.empty.desc') }}</div>
      </div>

      <Divider />
    </div>
  </div>
</template>
