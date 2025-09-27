<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'
import type { ValueOf } from 'type-fest'

definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
})

const ROLE = {
  'All': 'All',
  'Developer': 'Developer',
  '3DArtist': '3DArtist',
  'Balancer': 'Balancer',
  'MapDesigner': 'MapDesigner',
  'ItemCrafter': 'ItemCrafter',
  'CommunityManager': 'CommunityManager',
  'Moderator': 'Moderator',
  'QA': 'QA',
  'Contributor': 'Contributor',
} as const

type Role = ValueOf<typeof ROLE>

const IconByRole: Record<Role, string> = {
  [ROLE.All]: '',
  [ROLE.Developer]: 'lucide:code',
  [ROLE['3DArtist']]: 'lucide:box',
  [ROLE.Balancer]: 'lucide:scale',
  [ROLE.MapDesigner]: 'lucide:palette',
  [ROLE.ItemCrafter]: 'lucide:hammer',
  [ROLE.CommunityManager]: 'lucide:users',
  [ROLE.Moderator]: 'lucide:shield-check',
  [ROLE.QA]: 'lucide:check-circle',
  [ROLE.Contributor]: 'lucide:sparkles',
}

interface Contributor {
  name: string
  roles: Role[]
  description?: string
  github?: string
}

const contributors: Contributor[] = [
  { name: 'takeo', description: 'Lead Developer (2020 - 2023)', roles: [ROLE.Developer], github: 'verdie-g' },
  { name: 'Namidaka', description: 'Lead Developer (2023 - 2024)', roles: [ROLE.Developer], github: 'namidaka' },
  { name: 'Droob', description: 'Lead Developer (Current)', roles: [ROLE.Developer], github: 'Muparadzi' },
  { name: 'orle', description: 'Lead Web Developer', roles: [ROLE.Developer], github: 'or2e' },
  { name: 'Meowkov', description: 'Lead 3D Artist, Lead Item Manager, Lead Item Crafter', roles: [ROLE['3DArtist'], ROLE.ItemCrafter], github: 'Meowkov' },
  { name: 'Salt', description: 'Lead Balancer, Lead Item Crafter', roles: [ROLE.Balancer, ROLE.ItemCrafter, ROLE.Moderator], github: 'Erdertus' },
  { name: 'Yeldur', description: 'Lead Balancer, Lead Item Crafter', roles: [ROLE.CommunityManager, ROLE.ItemCrafter], github: 'Yeldur' },
  { name: 'vick', roles: [ROLE.Developer], github: 'its-vick' },
  { name: 'Wasch Kadse', roles: [ROLE.Developer], github: 'MrMendo' },
  { name: 'Gotha', roles: [ROLE.Developer], github: 'GerGotha' },
  { name: 'Norr', roles: [ROLE.Developer, ROLE.ItemCrafter, ROLE.QA], github: 'Norr2' },
  { name: 'James of Acre', roles: [ROLE.CommunityManager] },
  { name: 'Lemon', roles: [ROLE.CommunityManager] },
  { name: 'rfl', description: 'Map Design Lead', roles: [ROLE.MapDesigner], github: 'rf-l' },
  { name: 'Telford', roles: [ROLE.ItemCrafter], github: 'Telfordski' },
  { name: 'Truthful', roles: [ROLE.ItemCrafter], github: 'Truthful33' },
  { name: 'Diggles', roles: [ROLE.ItemCrafter], github: 'Diggles27' },
  { name: 'AngryMob', roles: [ROLE.ItemCrafter], github: 'AngryMob010' },
  { name: 'kaikaikai', roles: [ROLE.ItemCrafter], github: 'kaikaikaia' },
  { name: 'abearirl', roles: [ROLE.ItemCrafter], github: 'abearirlcrpg' },
  { name: 'AppleTruce', roles: [ROLE['3DArtist'], ROLE.ItemCrafter], github: 'AppleTruce' },
  { name: 'ADamnSexyName', roles: [ROLE['3DArtist']] },
  { name: 'gp', roles: [ROLE['3DArtist']] },
  { name: 'Cabs', roles: [ROLE['3DArtist']] },
  { name: 'Eba', roles: [ROLE['3DArtist']] },
  { name: 'Ikarooz', roles: [ROLE['3DArtist'], ROLE.MapDesigner] },
  { name: 'Judie', description: 'Map Design Lead', roles: [ROLE['3DArtist'], ROLE.MapDesigner] },
  { name: 'Nemerius', roles: [ROLE['3DArtist']] },
  { name: 'rhkt', roles: [ROLE['3DArtist']] },
  { name: 'ronin', roles: [ROLE['3DArtist']] },
  { name: 'tv', roles: [ROLE['3DArtist']] },
  { name: 'Phoenix', roles: [ROLE.Balancer] },
  { name: 'Dionysus', roles: [ROLE.Balancer] },
  { name: 'Yoshie', roles: [ROLE.Balancer] },
  { name: 'LizardWizard', description: 'Map Design Lead', roles: [ROLE.MapDesigner] },
  { name: 'LowLifeLarry', description: 'Map Design Lead', roles: [ROLE.MapDesigner] },
  { name: 'Lars', description: 'Map Design Lead', roles: [ROLE.MapDesigner] },
  { name: 'Bullero', description: 'Map Design Lead', roles: [ROLE.MapDesigner] },
  { name: 'Reginald', roles: [ROLE.MapDesigner] },
  { name: 'Radsvid', roles: [ROLE.MapDesigner] },
  { name: 'PloKoon', roles: [ROLE.MapDesigner] },
  { name: 'Firunien', roles: [ROLE.MapDesigner] },
  { name: 'Aragon', roles: [ROLE.MapDesigner] },
  { name: 'TruthfulLies', roles: [ROLE.MapDesigner] },
  { name: 'DannyD', roles: [ROLE.MapDesigner] },
  { name: 'Asda', roles: [ROLE.MapDesigner] },
  { name: 'MattyG', roles: [ROLE.MapDesigner] },
  { name: 'Nordwolf', roles: [ROLE.MapDesigner] },
  { name: 'Legion', roles: [ROLE.MapDesigner] },
  { name: 'mickwilliams', roles: [ROLE.MapDesigner] },
  { name: 'Wened', roles: [ROLE.MapDesigner] },
  { name: 'JayJRod', roles: [ROLE.MapDesigner] },
  { name: 'Eloquin', roles: [ROLE.MapDesigner] },
  { name: 'polluxo', roles: [ROLE.MapDesigner] },
  { name: 'Wese', roles: [ROLE.MapDesigner] },
  { name: 'Havoc', roles: [ROLE.MapDesigner] },
  { name: 'Korhum', roles: [ROLE.MapDesigner] },
  { name: 'ICEMAN1779', roles: [ROLE.MapDesigner] },
  { name: 'Giraffe', roles: [ROLE.MapDesigner] },
  { name: 'Axthenon', roles: [ROLE.ItemCrafter, ROLE.Moderator], github: 'axeltt012' },
  { name: 'Slyve', roles: [ROLE.ItemCrafter], github: 'S1yve' },
  { name: 'Caverus', roles: [ROLE.ItemCrafter], github: 'Caver16' },
  { name: 'Lubu', roles: [ROLE.ItemCrafter], github: 'MKVuong' },
  { name: 'Queuexx', roles: [ROLE.ItemCrafter], github: 'Queuexx' },
  { name: 'Lokiphur', roles: [ROLE.ItemCrafter], github: 'Lokiphur' },
  { name: 'GEEKILLER', roles: [ROLE.ItemCrafter], github: 'GEEKILLER95' },
  { name: 'MaxParks', roles: [ROLE.ItemCrafter], github: 'MaxParks' },
  { name: 'Zorglubz', roles: [ROLE.ItemCrafter], github: 'Zorglubz' },
  { name: 'LastKaze', roles: [ROLE.ItemCrafter], github: 'LastKaze' },
  { name: 'Lexxe', roles: [ROLE.ItemCrafter], github: 'SuperLexxe' },
  { name: 'Steve', roles: [ROLE.ItemCrafter], github: 'Steve1833' },
  { name: 'Dupre', description: 'Consultant/Server Host', roles: [] },
  { name: 'Redinb', roles: [ROLE.Moderator] },
  { name: 'Cajun', roles: [ROLE.Moderator] },
  { name: 'Caver', roles: [ROLE.Moderator] },
  { name: 'Demochi', roles: [ROLE.Moderator] },
  { name: 'Dill Pickle', roles: [ROLE.Moderator] },
  { name: 'Elindor', roles: [ROLE.Moderator] },
  { name: 'evilmagic', roles: [ROLE.Moderator] },
  { name: 'Gallonigher', roles: [ROLE.Moderator] },
  { name: 'IZA', roles: [ROLE.Moderator] },
  { name: 'Jimzy', roles: [ROLE.Moderator] },
  { name: 'Kadeth', roles: [ROLE.Moderator] },
  { name: 'ChesterOtab', roles: [ROLE.Moderator] },
  { name: 'Unrated', roles: [ROLE.Contributor], github: 'Unrated' },
  { name: '081233468', roles: [ROLE.Contributor], github: '081233468' },
  { name: 'kschulz90', roles: [ROLE.Contributor], github: 'kschulz90' },
  { name: 'Falcom', roles: [ROLE.Contributor], github: 'Falcomfr' },
  { name: 'mewtlu', roles: [ROLE.Contributor], github: 'mewtlu' },
  { name: 'Rogerdin13', roles: [ROLE.Contributor], github: 'Rogerdin13' },
  { name: 'thbolijn', roles: [ROLE.Contributor], github: 'thbolijn' },
  { name: 'petro-poroshenko', roles: [ROLE.Contributor], github: 'petro-poroshenko' },
  { name: 'Thradok', roles: [ROLE.Contributor], github: 'Thradok' },
  { name: 'kmiestmoore', roles: [ROLE.Contributor], github: 'kmiestmoore' },
  { name: 'winnie1069667084', roles: [ROLE.Contributor], github: 'winnie1069667084' },
  { name: 'grughub', roles: [ROLE.Contributor], github: 'grughub' },
  { name: 'EckoM89', roles: [ROLE.Contributor], github: 'EckoM89' },
  { name: 'havena', roles: [ROLE.Contributor], github: 'havena' },
  { name: 'Brainbirb', roles: [ROLE.Contributor], github: 'Brainbirb' },
  { name: 'Peeky', roles: [ROLE.Contributor], github: 'Peeky' },
  { name: 'Swagathor', roles: [ROLE.QA] },
  { name: 'Summer', roles: [ROLE.QA] },
  { name: 'Zee', roles: [ROLE.QA] },
  { name: 'Suleyk', roles: [ROLE.QA] },
  { name: 'HawtMiniBoss', roles: [ROLE.QA] },
  { name: 'DelroWanep', roles: [ROLE.QA] },
]

const currentFilter = ref<Role>(ROLE.All)

const filteredContributors = computed<Contributor[]>(() => {
  if (currentFilter.value === ROLE.All) {
    return contributors
  }
  return contributors.filter(p => p.roles.includes(currentFilter.value))
})
</script>

<template>
  <UContainer class="py-6">
    <div class="mx-auto max-w-4xl space-y-8">
      <UiHeading class="mb-14" :title="$t('credits.pageTitle')">
        <template #icon>
          <div class="flex-2">
            <UIcon
              name="crpg:sparkles"
              class="size-12 text-gold"
            />
          </div>
        </template>
      </UiHeading>

      <div class="space-y-8 text-center">
        <div
          class="
            mx-auto prose max-w-xl
            prose-strong:font-bold prose-strong:text-primary
          "
        >
          <p v-html="$t('credits.description')" />
          <h3 class="font-extrabold text-gold" v-html="$t('credits.subtitle')" />
        </div>
      </div>

      <UTabs
        v-model="currentFilter"
        :items="Object.values(ROLE).map<TabsItem>((role) => ({
          label: $t(`credits.role.${role}`),
          value: role,
          icon: IconByRole[role],
        }))"
        :content="false"
        color="neutral"
        size="xl"
        :ui="{
          list: 'flex-wrap gap-4 bg-transparent',
          indicator: 'hidden',
          trigger: 'data-[state=active]:bg-accented bg-elevated data-[state=active]:text-accented grow-0',
        }"
      />

      <div class="flex flex-wrap items-center justify-center gap-5">
        <Motion
          v-for="contributor in filteredContributors"
          :key="`${contributor.name}-${currentFilter}`"
          :initial="{
            y: 50,
            opacity: 0,
          }"
          :in-view="{
            y: 0,
            opacity: 1,
          }"
          :transition="{
            type: 'spring',
            stiffness: 350,
            damping: 30,
          }"
        >
          <UUser
            :ui="{
              description: 'max-w-[160px]',
              avatar: 'rounded-none squircle',
              name: 'truncate max-w-[140px]',
            }"
            class="rounded-lg px-3 py-2.5 ring-1 ring-muted"
            :name="contributor.name"
            size="xl"
            v-bind="{ ...(contributor.github ? {
              to: `https://github.com/${contributor.github}`,
              target: '_blank',
              // TODO: steam avatar
              avatar: {
                src: `https://github.com/${contributor.github}.png`,
              },
            } : {
              avatar: {
                text: contributor.name.at(0)?.toUpperCase(),
              },
            }) }"
          >
            <template #description>
              <UiTextView
                v-if="contributor.description"
                variant="caption-xs"
              >
                {{ contributor.description }}
              </UiTextView>
              <div class="mt-2 flex flex-wrap items-center gap-1.5">
                <div
                  v-for="role in contributor.roles"
                  :key="role"
                >
                  <UBadge
                    variant="subtle"
                    :label="$t(`credits.role.${role}`)"
                  />
                </div>
              </div>
            </template>
          </UUser>
        </Motion>
      </div>
    </div>
  </UContainer>
</template>

<style>
.squircle {
  mask-image: url("data:image/svg+xml,%3csvg width='200' height='200' xmlns='http://www.w3.org/2000/svg'%3e%3cpath d='M100 0C20 0 0 20 0 100s20 100 100 100 100-20 100-100S180 0 100 0Z'/%3e%3c/svg%3e");
  mask-size: contain;
  mask-position: center;
  mask-repeat: no-repeat;
}
</style>
