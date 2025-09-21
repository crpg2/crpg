<script setup lang="ts">
const router = useRouter()
</script>

<template>
  <div class="prose">
    <h2 class="text-center">
      {{ $t('help.overview.title') }}
    </h2>

    <div
      v-html="
        $t(
          'help.overview.intro',
          {
            buildSupportLink: router.resolve({ name: 'help-build-support' }).path,
          },
        )"
    />
  </div>
</template>
