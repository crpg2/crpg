<script setup lang="ts">
import { Role } from '~/models/role'

definePageMeta({
  roles: [Role.Admin, Role.Moderator, Role.User],
})

const userStore = useUserStore()
const { characters, user } = toRefs(userStore)

const route = useRoute('characters-id')
const router = useRouter()
</script>

<template>
  <div class="container relative py-6">
    <NuxtPage />
  </div>
</template>
