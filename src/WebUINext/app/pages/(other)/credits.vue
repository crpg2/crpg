<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
})

interface Contributor {
  name: string
  role: string
  github?: string
}

const mainDevelopers: Contributor[] = [
  { name: 'takeo', role: 'Lead Developer (2020 - 2023)', github: 'verdie-g' },
  { name: 'Namidaka', role: 'Lead Developer (2023 - 2024)', github: 'namidaka' },
  { name: 'Droob', role: 'Lead Developer (Current)', github: 'Muparadzi' },
  { name: 'orle', role: 'Lead Web Developer', github: 'or2e' },
  { name: 'Meowkov', role: 'Lead 3D Artist, Lead Item Manager, Lead Item Crafter', github: 'Meowkov' },
  { name: 'Yeldur', role: 'Community Manager, Lead Balancer, Lead Item Crafter', github: 'Yeldur' },
  { name: 'Salt', role: 'Lead Balancer, Lead Item Crafter, Moderator', github: 'Erdertus' },
]

const contributors: Contributor[] = [
  // { name: 'takeo', role: 'Lead Developer (2020 - 2023)', github: 'verdie-g' },
  // { name: 'Namidaka', role: 'Lead Developer (2023 - 2024)', github: 'namidaka' },
  // { name: 'Droob', role: 'Lead Developer (Current)', github: 'Muparadzi' },
  // { name: 'orle', role: 'Lead Web Developer', github: 'or2e' },
  { name: 'Wasch Kadse', role: 'Developer', github: 'MrMendo' },
  { name: 'Gotha', role: 'Developer', github: 'GerGotha' },
  { name: 'Norr', role: 'Developer, Item Crafter, QA', github: 'Norr2' },
  // { name: 'Meowkov', role: 'Lead 3D Artist, Lead Item Manager, Lead Item Crafter', github: 'Meowkov' },
  // { name: 'Yeldur', role: 'Community Manager, Lead Balancer, Lead Item Crafter', github: 'Yeldur' },
  // { name: 'Salt', role: 'Lead Balancer, Lead Item Crafter, Moderator', github: 'Erdertus' },
  { name: 'James of Acre', role: 'Community Manager' },
  { name: 'Lemon', role: 'Community Manager' },
  { name: 'rfl', role: 'Map Design Lead, Map Designer', github: 'rf-l' },
  { name: 'Telford', role: 'Item Crafter', github: 'Telfordski' },
  { name: 'Truthful', role: 'Item Crafter', github: 'Truthful33' },
  { name: 'Diggles', role: 'Item Crafter', github: 'Diggles27' },
  { name: 'AngryMob', role: 'Item Crafter', github: 'AngryMob010' },
  { name: 'kaikaikai', role: 'Item Crafter', github: 'kaikaikaia' },
  { name: 'abearirl', role: 'Item Crafter', github: 'abearirlcrpg' },
  { name: 'AppleTruce', role: 'Item Crafter, 3D Artist', github: 'AppleTruce' },
  { name: 'ADamnSexyName', role: '3D Artist' },
  { name: 'gp', role: '3D Artist' },
  { name: 'Cabs', role: '3D Artist' },
  { name: 'Eba', role: '3D Artist' },
  { name: 'Ikarooz', role: '3D Artist, Map Designer' },
  { name: 'Judie', role: '3D Artist, Map Design Lead, Map Designer' },
  { name: 'Nemerius', role: '3D Artist' },
  { name: 'rhkt', role: '3D Artist' },
  { name: 'ronin', role: '3D Artist' },
  { name: 'tv', role: '3D Artist' },
  { name: 'LizardWizard', role: 'Map Design Lead, Map Designer' },
  { name: 'LowLifeLarry', role: 'Map Design Lead, Map Designer' },
  { name: 'Lars', role: 'Map Design Lead, Map Designer' },
  { name: 'Bullero', role: 'Map Design Lead, Map Designer' },
  { name: 'Reginald', role: 'Map Designer' },
  { name: 'Radsvid', role: 'Map Designer' },
  { name: 'PloKoon', role: 'Map Designer' },
  { name: 'Firunien', role: 'Map Designer' },
  { name: 'Aragon', role: 'Map Designer' },
  { name: 'TruthfulLies', role: 'Map Designer' },
  { name: 'DannyD', role: 'Map Designer' },
  { name: 'Asda', role: 'Map Designer' },
  { name: 'MattyG', role: 'Map Designer' },
  { name: 'Nordwolf', role: 'Map Designer' },
  { name: 'Legion', role: 'Map Designer' },
  { name: 'mickwilliams', role: 'Map Designer' },
  { name: 'Wened', role: 'Map Designer' },
  { name: 'JayJRod', role: 'Map Designer' },
  { name: 'Eloquin', role: 'Map Designer' },
  { name: 'polluxo', role: 'Map Designer' },
  { name: 'Wese', role: 'Map Designer' },
  { name: 'Havoc', role: 'Map Designer' },
  { name: 'Korhum', role: 'Map Designer' },
  { name: 'ICEMAN1779', role: 'Map Designer' },
  { name: 'Giraffe', role: 'Map Designer' },
  { name: 'Axthenon', role: 'Item Crafter, Moderator', github: 'axeltt012' },
  { name: 'Slyve', role: 'Item Crafter', github: 'S1yve' },
  { name: 'Caverus', role: 'Item Crafter', github: 'Caver16' },
  { name: 'Lubu', role: 'Item Crafter', github: 'MKVuong' },
  { name: 'Queuexx', role: 'Item Crafter', github: 'Queuexx' },
  { name: 'Lokiphur', role: 'Item Crafter', github: 'Lokiphur' },
  { name: 'GEEKILLER', role: 'Item Crafter', github: 'GEEKILLER95' },
  { name: 'MaxParks', role: 'Item Crafter', github: 'MaxParks' },
  { name: 'Zorglubz', role: 'Item Crafter', github: 'Zorglubz' },
  { name: 'LastKaze', role: 'Item Crafter', github: 'LastKaze' },
  { name: 'Lexxe', role: 'Item Crafter', github: 'SuperLexxe' },
  { name: 'Steve', role: 'Item Crafter', github: 'Steve1833' },
  { name: 'Dupre', role: 'Consultant/Server Host' },
  { name: 'Redinb', role: 'Moderator' },
  { name: '081233468', role: 'Contributor ', github: '081233468' },
  { name: 'Cajun', role: 'Moderator' },
  { name: 'Caver', role: 'Moderator' },
  { name: 'Demochi', role: 'Moderator ' },
  { name: 'Dill Pickle', role: 'Moderator ' },
  { name: 'Elindor', role: 'Moderator ' },
  { name: 'evilmagic', role: 'Moderator ' },
  { name: 'Gallonigher', role: 'Moderator ' },
  { name: 'IZA', role: 'Moderator ' },
  { name: 'Jimzy', role: 'Moderator ' },
  { name: 'Kadeth', role: 'Moderator ' },
  { name: 'ChesterOtab', role: 'Moderator ' },
  { name: 'Unrated', role: 'Contributor ', github: 'Unrated' },
  { name: 'kschulz90', role: 'Contributor ', github: 'kschulz90' },
  { name: 'Falcom', role: 'Contributor ', github: 'Falcomfr' },
  { name: 'mewtlu', role: 'Contributor ', github: 'mewtlu' },
  { name: 'Rogerdin13', role: 'Contributor ', github: 'Rogerdin13' },
  { name: 'thbolijn', role: 'Contributor ', github: 'thbolijn' },
  { name: 'petro-poroshenko', role: 'Contributor ', github: 'petro-poroshenko' },
  { name: 'Thradok', role: 'Contributor ', github: 'Thradok' },
  { name: 'kmiestmoore', role: 'Contributor ', github: 'kmiestmoore' },
  { name: 'winnie1069667084', role: 'Contributor ', github: 'winnie1069667084' },
  { name: 'grughub', role: 'Contributor ', github: 'grughub' },
  { name: 'EckoM89', role: 'Contributor ', github: 'EckoM89' },
  { name: 'havena', role: 'Contributor ', github: 'havena' },
  { name: 'Brainbirb', role: 'Contributor ', github: 'Brainbirb' },
  { name: 'Peeky', role: 'Contributor ', github: 'Peeky' },
  { name: 'Swagathor', role: 'QA ' },
  { name: 'Summer', role: 'QA ' },
  { name: 'Zee', role: 'QA ' },
  { name: 'Suleyk', role: 'QA ' },
  { name: 'HawtMiniBoss', role: 'QA ' },
  { name: 'DelroWanep', role: 'QA ' },
  { name: 'Phoenix', role: 'Balancer ' },
  { name: 'Dionysus', role: 'Balancer ' },
  { name: 'Yoshie', role: 'Balancer' },
]

const roles = ['All', 'Lead Developer', '3D Artist', 'Community Manager', 'Item Crafter']

const currentFilter = ref('All')

const filteredPeople = computed<Contributor[]>(() => {
  if (currentFilter.value === 'All') {
    return [...mainDevelopers, ...contributors]
  }
  return [...mainDevelopers, ...contributors].filter(p => p.role.includes(currentFilter.value))
})
</script>

<template>
  <UContainer class="py-6">
    <div class="mx-auto max-w-3xl space-y-6">
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
        <UiTextView variant="p" class="mx-auto max-w-xl">
          This channel serves to credit our <span class="font-bold text-primary">developers, contributors, creators, artists, and everyone else</span> for their role in helping to build our mod. We appreciate everything given to keep this mod alive no matter how small.
        </UiTextView>

        <UiTextView variant="h3" class="font-extrabold text-gold">
          Without you, this mod would not have been possible!
        </UiTextView>
      </div>

      <UTabs
        v-model="currentFilter"
        :items="roles.map<TabsItem>((role) => ({
          label: role,
          value: role,
        }))"
        :content="false"
        color="neutral"
        size="xl"
      />

      <div class="flex flex-wrap items-center justify-center gap-5">
        <Motion
          v-for="(user, index) in filteredPeople"
          :key="`${user.name}-${currentFilter}`"
          :initial="{
            y: 100,
            opacity: 0,
          }"
          :animate="{
            y: 0,
            opacity: 1,
          }"
          :transition="{
            type: 'spring',
            stiffness: 350,
            damping: 20,
            delay: index * 0.05,
          }"
        >
          <UUser
            :ui="{
              description: 'max-w-[160px]',
              avatar: 'rounded-none squircle',
              name: 'truncate max-w-[110px]',
            }"
            class="rounded-lg px-3 py-2.5 ring-1 ring-muted"
            :name="user.name"
            size="lg"
            :description="user.role"
            v-bind="{ ...(user.github ? {
              to: `https://github.com/${user.github}`,
              target: '_blank',
              avatar: {
                src: `https://github.com/${user.github}.png`,
              },
            } : {
              avatar: {
                text: user.name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase(),
              },
            }) }"
          />
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

.credits-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  align-items: center;
  /* display: grid; */
  /* grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); */
  gap: 1rem;
}
</style>
