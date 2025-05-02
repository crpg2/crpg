<script setup lang="ts">
const refreshPage = () => {
  navigateTo({
    name: 'characters', // will trigger middleware and redirect to the new active character.
  }, {
    external: true,
  })
}
</script>

<template>
  <UiModal closable>
    <template #popper>
      <div class="max-w-2xl">
        <div class="border-b border-border-200 px-12 pb-8 pt-11 text-center">
          <h3 class="text-xl text-content-100">
            {{ $t('character.create.guide.title') }}
          </h3>
        </div>

        <div class="prose prose-invert border-b border-border-200 px-12 py-8">
          <p class="text-content-400">
            {{ $t('character.create.guide.intro') }}
          </p>
          <ol>
            <li>{{ $t('character.create.guide.step.install') }}</li>
            <li>{{ $t('character.create.guide.step.join') }}</li>
            <li>{{ $t('character.create.guide.step.joined') }}</li>
            <i18n-t
              scope="global"
              keypath="character.create.guide.step.refresh"
              tag="li"
            >
              <template #refresh>
                <span
                  class="cursor-pointer text-content-link underline hover:text-content-link-hover hover:no-underline"
                  @click="refreshPage"
                >
                  {{ $t('action.refresh', 1) }}
                </span>
              </template>
            </i18n-t>
          </ol>
        </div>

        <div class="px-12 py-4">
          <p class="text-center text-status-warning">
            {{ $t('character.create.guide.outro') }}
          </p>
        </div>
      </div>
    </template>
  </UiModal>
</template>
