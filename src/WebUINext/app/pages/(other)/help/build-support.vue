<script setup lang="ts">
const router = useRouter()

const { links } = useAppConfig()
</script>

<template>
  <div class="prose">
    <h2 class="text-center">
      {{ $t('help.buildSupport.title') }}
    </h2>

    <div
      v-html="
        $t(
          'help.buildSupport.content',
          {
            bulderLink: router.resolve({ name: 'builder' }).path,
            discordLink: links.buildSupportThread,
          },
        )"
    />
  </div>
</template>
