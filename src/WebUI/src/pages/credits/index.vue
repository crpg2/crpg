<template>
  <div class="container mx-auto p-6">
    <h1 class="text-3xl font-bold text-center text-primary-400 mb-6">
      Project Credits
    </h1>

    <p class="text-md text-gray-300 text-center mb-6">
      This channel serves to credit our <span class="font-bold text-white">developers, contributors, creators, artists, and everyone else</span> for their role in helping to build our mod. We appreciate everything given to keep this mod alive no matter how small.
    </p>
    <p class="text-lg text-gray-300 text-center mb-6">
      <span class="font-bold text-white">Without you, this mod would not have been possible!</span>
    </p>

    <div class="overflow-x-auto">
      <OTable
        :data="contributors"
        bordered
        hoverable
        :per-page="12"
        paginated
      >
        <OTableColumn field="profile" label="Profile">
          <template #default="{ row: contributor }">
  <div class="flex items-center justify-center w-12 h-12 rounded-full bg-gray-700 text-white text-sm font-semibold overflow-hidden">
    <template v-if="contributor.github">
      <a
        :href="`https://github.com/${contributor.github}`"
        target="_blank"
        rel="noopener noreferrer"
        class="block w-full h-full"
      >
      <img
          loading="lazy"
          :src="`https://github.com/${contributor.github}.png`"
          :alt="`${contributor.name}'s GitHub Profile`"
          class="object-cover w-full h-full rounded-full"
        />
      </a>
    </template>
    <template v-else>
      {{ contributor.name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase() }}
    </template>
  </div>
</template>

        </OTableColumn>

        <OTableColumn field="name" label="Name">
          <template #default="{ row: contributor }">
            {{ contributor.name }}
          </template>
        </OTableColumn>

        <OTableColumn field="role" label="Role">
          <template #default="{ row: contributor }">
            {{ contributor.role || 'Supporter' }}
          </template>
        </OTableColumn>
      </OTable>
    </div>
  </div>
</template>

<script setup lang="ts">
const contributors = [
  { name: "takeo", role: "Lead Developer (2020 - 2023)", github: "verdie-g" },
  { name: "Namidaka", role: "Lead Developer (2023 - 2024)", github: "namidaka" },
  { name: "Droob", role: "Lead Developer (Current)", github: "Muparadzi" },
  { name: "orle", role: "Lead Web Developer", github: "or2e" },
  { name: "Wasch Kadse", role: "Developer", github: "MrMendo" },
  { name: "Gotha", role: "Developer", github: "GerGotha" },
  { name: "Norr", role: "Developer, Item Crafter, QA", github: "Norr2" },
  { name: "Meowkov", role: "Lead 3D Artist, Lead Item Manager, Lead Item Crafter", github: "Meowkov" },
  { name: "Yeldur", role: "Community Manager, Lead Balancer, Lead Item Crafter", github: "Yeldur" },
  { name: "Salt", role: "Lead Balancer, Lead Item Crafter, Moderator", github: "Erdertus" },
  { name: "James of Acre", role: "Community Manager" },
  { name: "Lemon", role: "Community Manager" },
  { name: "rfl", role: "Map Design Lead, Map Designer", github: "rf-l" },
  { name: "Telford", role: "Item Crafter", github: "Telfordski" },
  { name: "Truthful", role: "Item Crafter", github: "Truthful33" },
  { name: "Diggles", role: "Item Crafter", github: "Diggles27" },
  { name: "AngryMob", role: "Item Crafter", github: "AngryMob010" },
  { name: "kaikaikai", role: "Item Crafter", github: "kaikaikaia" },
  { name: "abearirl", role: "Item Crafter", github: "abearirlcrpg" },
  { name: "AppleTruce", role: "Item Crafter, 3D Artist", github: "AppleTruce" },
  { name: "ADamnSexyName", role: "3D Artist" },
  { name: "gp", role: "3D Artist" },
  { name: "Cabs", role: "3D Artist" },
  { name: "Eba", role: "3D Artist" },
  { name: "Ikarooz", role: "3D Artist, Map Designer" },
  { name: "Judie", role: "3D Artist, Map Design Lead, Map Designer" },
  { name: "Nemerius", role: "3D Artist" },
  { name: "rhkt", role: "3D Artist" },
  { name: "ronin", role: "3D Artist" },
  { name: "tv", role: "3D Artist" },
  { name: "LizardWizard", role: "Map Design Lead, Map Designer" },
  { name: "LowLifeLarry", role: "Map Design Lead, Map Designer" },
  { name: "Lars", role: "Map Design Lead, Map Designer" },
  { name: "Bullero", role: "Map Design Lead, Map Designer" },
  { name: "Reginald", role: "Map Designer" },
  { name: "Radsvid", role: "Map Designer" },
  { name: "PloKoon", role: "Map Designer" },
  { name: "Firunien", role: "Map Designer" },
  { name: "Aragon", role: "Map Designer" },
  { name: "TruthfulLies", role: "Map Designer" },
  { name: "DannyD", role: "Map Designer" },
  { name: "Asda", role: "Map Designer" },
  { name: "MattyG", role: "Map Designer" },
  { name: "Nordwolf", role: "Map Designer" },
  { name: "Legion", role: "Map Designer" },
  { name: "mickwilliams", role: "Map Designer" },
  { name: "Wened", role: "Map Designer" },
  { name: "JayJRod", role: "Map Designer" },
  { name: "Eloquin", role: "Map Designer" },
  { name: "polluxo", role: "Map Designer" },
  { name: "Wese", role: "Map Designer" },
  { name: "Havoc", role: "Map Designer" },
  { name: "Korhum", role: "Map Designer" },
  { name: "ICEMAN1779", role: "Map Designer" },
  { name: "Giraffe", role: "Map Designer" },
  { name: "Axthenon", role: "Item Crafter, Moderator", github: "axeltt012" },
  { name: "Slyve", role: "Item Crafter", github: "S1yve" },
  { name: "Caverus", role: "Item Crafter", github: "Caver16" },
  { name: "Lubu", role: "Item Crafter", github: "MKVuong" },
  { name: "Queuexx", role: "Item Crafter", github: "Queuexx"},
  { name: "Lokiphur", role: "Item Crafter", github: "Lokiphur" },
  { name: "GEEKILLER", role: "Item Crafter", github: "GEEKILLER95" },
  { name: "MaxParks", role: "Item Crafter", github: "MaxParks" },
  { name: "Zorglubz", role: "Item Crafter", github: "Zorglubz" },
  { name: "LastKaze", role: "Item Crafter", github: "LastKaze" },
  { name: "Lexxe", role: "Item Crafter", github: "SuperLexxe" },
  { name: "Steve", role: "Item Crafter", github: "Steve1833" },
  { name: "Dupre", role: "Consultant/Server Host" },
  { name: "Redinb", role: "Moderator" },
  { name: "081233468", role: "Contributor ", github: "081233468" },
  { name: "Cajun", role: "Moderator" },
  { name: "Caver", role: "Moderator" },
  { name: "Demochi", role: "Moderator " },
  { name: "Dill Pickle", role: "Moderator " },
  { name: "Elindor", role: "Moderator " },
  { name: "evilmagic", role: "Moderator " },
  { name: "Gallonigher", role: "Moderator " },
  { name: "IZA", role: "Moderator " },
  { name: "Jimzy", role: "Moderator " },
  { name: "Kadeth", role: "Moderator " },
  { name: "ChesterOtab", role: "Moderator " },
  { name: "Unrated", role: "Contributor ", github: "Unrated" },
  { name: "kschulz90", role: "Contributor ", github: "kschulz90" },
  { name: "Falcom", role: "Contributor ", github: "Falcomfr" },
  { name: "mewtlu", role: "Contributor ", github: "mewtlu" },
  { name: "Rogerdin13", role: "Contributor ", github: "Rogerdin13" },
  { name: "thbolijn", role: "Contributor ", github: "thbolijn" },
  { name: "petro-poroshenko", role: "Contributor ", github: "petro-poroshenko" },
  { name: "Thradok", role: "Contributor ", github: "Thradok" },
  { name: "kmiestmoore", role: "Contributor ", github: "kmiestmoore" },
  { name: "winnie1069667084", role: "Contributor ", github: "winnie1069667084" },
  { name: "grughub", role: "Contributor ", github: "grughub" },
  { name: "EckoM89", role: "Contributor ", github: "EckoM89" },
  { name: "havena", role: "Contributor ", github: "havena" },
  { name: "Brainbirb", role: "Contributor ", github: "Brainbirb" },
  { name: "Peeky", role: "Contributor ", github: "Peeky" },
  { name: "Swagathor", role: "QA " },
  { name: "Summer", role: "QA " },
  { name: "Zee", role: "QA " },
  { name: "Suleyk", role: "QA " },
  { name: "HawtMiniBoss", role: "QA " },
  { name: "DelroWanep", role: "QA " },
  { name: "Phoenix", role: "Balancer " },
  { name: "Dionysus", role: "Balancer " },
  { name: "Yoshie", role: "Balancer" }
]
;
</script>