<script setup lang="ts">
const router = useRouter()
</script>

<template>
  <div class="prose">
    <h3 class="text-center">
      {{ $t('help.overview.title') }}
    </h3>

    <div
      v-html="
        $t(
          'help.overview.intro',
          {
            buildSupportLink: `<a href='${router.resolve({ name: 'help-build-support' }).path}'> ${$t('help.buildSupport.navTitle')}</a>`,
          },
        )"
    />
  </div>
</template>
