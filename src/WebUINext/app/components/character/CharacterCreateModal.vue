<script setup lang="ts">
const refreshPage = () => navigateTo({
  name: 'characters', // will trigger middleware and redirect to the new active character.
}, {
  external: true,
})
</script>

<template>
  <UModal
    :title="$t('character.create.guide.title')"
    :ui="{
      content: 'max-w-3xl',
    }"
    :close="{
      size: 'sm',
      color: 'secondary',
      variant: 'solid',
    }"
  >
    <template #body>
      <div class="prose prose-invert">
        <p class="">
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
                class="cursor-pointer text-link underline hover:text-link-hover hover:no-underline"
                @click="refreshPage"
              >
                {{ $t('action.refresh', 1) }}
              </span>
            </template>
          </i18n-t>
        </ol>
      </div>
    </template>

    <template #footer>
      <p class="text-warning">
        {{ $t('character.create.guide.outro') }}
      </p>
    </template>
  </UModal>
</template>
