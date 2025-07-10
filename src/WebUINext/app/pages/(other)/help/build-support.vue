<script setup lang="ts">
const router = useRouter()
</script>

<template>
  <div class="prose">
    <h3 class="text-center">
      {{ $t('help.buildSupport.title') }}
    </h3>

    <div
      v-html="
        $t(
          'help.buildSupport.content',
          {
            bulderLink: `<a href='${router.resolve({ name: 'builder' }).path}'> ${$t('nav.main.Builder', 1)}</a>`,
          },
        )"
    />
  </div>
</template>
