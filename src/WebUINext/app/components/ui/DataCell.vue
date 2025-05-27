<script setup lang="ts">
</script>

<template>
  <div class="TODO: flex items-center gap-1.5">
    <slot name="leftContent" />
    <slot />
    <slot name="rightContent" />
  </div>
</template>
