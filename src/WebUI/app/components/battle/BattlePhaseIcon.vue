<script setup lang="ts">
const props = defineProps<{
  phase: 'Preparation' | 'Hiring' | 'Scheduled' | 'Live' | 'End'
  size?: number
}>()

const phaseConfig = {
  Preparation: {
    color: '#9ca3af', // gray-400
    animation: 'pulse-slow',
    icon: '⚙️',
  },
  Hiring: {
    color: '#facc15', // yellow-400
    animation: 'dots',
    icon: '👥',
  },
  Scheduled: {
    color: '#3b82f6', // blue-500
    animation: 'rotate-slow',
    icon: '⏰',
  },
  Live: {
    color: '#22c55e', // green-500
    animation: 'ping',
    icon: '●',
  },
  End: {
    color: '#6b7280', // gray-500
    animation: 'fade',
    icon: '⚔️',
  },
} as const

const cfg = computed(() => phaseConfig[props.phase])
</script>

<template>
  <div
    class="relative inline-flex items-center gap-1"
    :style="{ fontSize: `${size ?? 16}px`, color: cfg.color }"
  >
    <!-- Animated dot/icon -->
    <div
      class="flex items-center justify-center"
      :class="`
        phase-${cfg.animation}
      `"
    >
      <span>{{ cfg.icon }}</span>
    </div>

    <!-- Phase label -->
    <span class="text-sm font-medium select-none">
      {{ phase }}
    </span>
  </div>
</template>

<style scoped>
/* ===== Animations ===== */

/* Preparation – slow pulse */
@keyframes pulseSlow {
  0%, 100% { opacity: 0.5; transform: scale(1); }
  50% { opacity: 1; transform: scale(1.1); }
}
.phase-pulse-slow {
  animation: pulseSlow 2.5s ease-in-out infinite;
}

/* Hiring – typing dots (3 dots flicker) */
.phase-dots span::after {
  content: '';
  display: inline-block;
  width: 0.6em;
  text-align: left;
  animation: dotsAnim 1.4s infinite;
}
@keyframes dotsAnim {
  0% { content: ''; }
  25% { content: '.'; }
  50% { content: '..'; }
  75%,100% { content: '...'; }
}

/* Scheduled – slow rotate */
@keyframes rotateSlow {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
.phase-rotate-slow {
  display: inline-block;
  animation: rotateSlow 3.5s linear infinite;
}

/* Live – ping (pulsing ring) */
@keyframes ping {
  0% { transform: scale(1); opacity: 1; }
  75%, 100% { transform: scale(1.4); opacity: 0; }
}
.phase-ping {
  position: relative;
}
.phase-ping::before {
  content: '';
  position: absolute;
  inset: 0;
  border-radius: 50%;
  background-color: currentColor;
  opacity: 0.4;
  animation: ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite;
}
.phase-ping span {
  position: relative;
  display: inline-block;
  width: 0.6em;
  height: 0.6em;
  border-radius: 50%;
  background-color: currentColor;
}

/* End – fade out */
@keyframes fadeOut {
  0%, 20% { opacity: 1; }
  100% { opacity: 0.4; }
}
.phase-fade {
  animation: fadeOut 2.5s ease-in-out infinite alternate;
}
</style>
