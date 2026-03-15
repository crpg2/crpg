<script setup lang="ts">
defineProps<{
  label: string
}>()

const emit = defineEmits<{
  submit: []
  cancel: []
}>()

const open = ref(false)

const onCancel = () => {
  emit('cancel')
  open.value = false
}

const onSubmit = () => {
  emit('submit')
  open.value = false
}
</script>

<template>
  <UDrawer
    v-model:open="open"
    direction="top"
    :handle="false"
    :dismissible="false"
    :handle-only="false"
    :ui="{
      header: 'flex items-center justify-center gap-4',
      content: 'mx-auto max-w-xl',
      body: 'flex justify-center',
      footer: 'flex flex-row justify-end',
    }"
  >
    <slot />

    <template #header>
      <div class="flex flex-1 flex-wrap items-center justify-center gap-4">
        <UiTextView variant="h2">
          {{ label }}
        </UiTextView>
      </div>

      <div class="mr-0 ml-auto">
        <UButton
          color="neutral"
          variant="ghost"
          icon="i-lucide-x"
          @click="onCancel"
        />
      </div>
    </template>

    <template #body>
      <slot name="body" />
    </template>

    <template #footer>
      <UButton
        :label="$t('action.cancel')"
        block
        color="neutral"
        variant="soft"
        @click="onCancel"
      />

      <UButton
        :label="$t('action.submit')"
        block
        color="primary"
        variant="soft"
        @click="onSubmit"
      />
    </template>
  </UDrawer>
</template>
