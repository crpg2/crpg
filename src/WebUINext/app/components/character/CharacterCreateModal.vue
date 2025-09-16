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
      content: 'max-w-xl',
    }"
  >
    <template #body>
      <UiTextView variant="p" tag="p" margin-bottom>
        {{ $t('character.create.guide.intro') }}
      </UiTextView>
      <UiListView variant="number">
        <UiListViewItem :label="$t('character.create.guide.step.install')" />
        <UiListViewItem :label="$t('character.create.guide.step.join')" />
        <UiListViewItem :label="$t('character.create.guide.step.joined')" />
        <UiListViewItem>
          <i18n-t
            scope="global"
            keypath="character.create.guide.step.refresh"
          >
            <template #refresh>
              <ULink @click="refreshPage">
                {{ $t('action.refresh', 1) }}
              </ULink>
            </template>
          </i18n-t>
        </UiListViewItem>
      </UiListView>
    </template>

    <template #footer>
      <UiTextView variant="p" tag="p" class="text-warning">
        {{ $t('character.create.guide.outro') }}
      </UiTextView>
    </template>
  </UModal>
</template>
