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
    <p class="text-md text-gray-300 text-center p-2 mb-6">
      Due to the sheer volume of people who have supported us, the table list
      <span class="font-bold text-white">only contains the main contributors on GitHub.</span>
      You can find a full list here on our
      <a href="https://discord.com/channels/279063743839862805/1187124941942947960" target="_blank" class="underline font-semibold text-yellow-200 hover:text-yellow-100"> Discord Server</a>
      Alternatively, check the below expandable list of people simply organised by their names:
    </p>

    <!-- Collapsible section for full supporter list -->
    <details class="mt-4 mb-8 text-sm text-gray-300 bg-gray-800 border border-gray-600 rounded p-4">
      <summary class="cursor-pointer font-semibold text-white hover:underline">
        Show Full List of Supporters
      </summary>
      <p class="mt-4 leading-relaxed">
        ADamnSexyName, gp, Cabs, Eba, Ikarooz, Judie, Nemerius, rhkt, ronin, tv, LizardWizard, LowLifeLarry, Lars, judie, Bullero, Reginald, Radsvid, PloKoon, Firunien, Aragon, TruthfulLies, DannyD, Asda, MattyG, Nordwolf, Legion, mickwilliams, Wened, JayJRod, Eloquin, polluxo, Wese, Havoc, Korhum, ICEMAN1779, Giraffe, FreyaUwU, axeltt012 (axthenon), S1yve, Caver16, MKVuong, Queuexx, Lokiphur, GEEKILLER95, MaxParks, Zorglubz, LastKaze, SuperLexxe, Steve, Meow, Lemon, James of Acre, Dupre, Redinb, 081233468, Cajun, Caver, Demochi, Dill Pickle, Elindor, Axthenon, evilmagic, Gallonigher, IZA, JayJrod, Jimzy, Kadeth, ChesterOtab, Unrated, kschulz90, Falcomfr, mewtlu, Rogerdin13, thbolijn, petro-poroshenko, Thradok, kmiestmoore, winnie1069667084, grughub, EckoM89, havena, Brainbirb, Peeky, Steve1833, Swagathor, Summer, Zee, Suleyk, HawtMiniBoss, DelroWanep, Phoenix, Dionysus
      </p>
    </details>

    <div class="overflow-x-auto">
      <table class="min-w-full border border-gray-600 bg-gray-800 shadow-md rounded-lg text-gray-300">
        <thead>
          <tr class="bg-gray-700 text-xl font-bold">
            <th class="px-4 py-3 text-left border-b border-gray-600">Profile</th>
            <th class="px-4 py-3 text-left border-b border-gray-600">Name</th>
            <th class="px-4 py-3 text-left border-b border-gray-600">Role</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="(contributor, index) in contributors"
            :key="index"
            class="border-t border-gray-600 hover:bg-gray-700"
          >
            <td class="px-4 py-2">
              <template v-if="contributor.profile && getGithubUsername(contributor.profile)">
                <a :href="contributor.profile" target="_blank">
                  <img
                    :src="getGithubProfileImage(contributor.profile)"
                    :alt="`${contributor.name}'s GitHub Profile`"
                    class="w-12 h-12 rounded-full border-2 border-gray-500 hover:border-blue-400 transition duration-300"
                  />
                </a>
              </template>
              <template v-else>
                <div
                  class="w-12 h-12 flex items-center justify-center rounded-full bg-gray-600 text-white font-bold border-2 border-gray-500"
                  title="No GitHub profile"
                >
                  {{ contributor.name.charAt(0).toUpperCase() }}
                </div>
              </template>
            </td>
            <td class="px-4 py-2">{{ contributor.name }}</td>
            <td class="px-4 py-2">{{ contributor.role }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
const contributors = [
  { name: "takeo", role: "Lead Developer (2020 - 2023)", profile: "https://github.com/verdie-g" },
  { name: "Namidaka", role: "Lead Developer (2023 - 2024)", profile: "https://github.com/namidaka" },
  { name: "Droob", role: "Lead Developer (Current)", profile: "https://github.com/Muparadzi" },
  { name: "orle", role: "Lead Web Developer", profile: "https://github.com/or2e" },
  { name: "Wasch Kadse", role: "Developer", profile: "https://github.com/MrMendo" },
  { name: "Gotha", role: "Developer", profile: "https://github.com/GerGotha" },
  { name: "Norr", role: "Developer, Item Crafter, QA", profile: "https://github.com/Norr2" },
  { name: "Meowkov", role: "Lead 3D Artist, Lead Item Manager,  Lead Item Crafter", profile: "https://github.com/Meowkov" },
  { name: "Yeldur", role: "Community Manager, Lead Balancer, Lead Item Crafter", profile: "https://github.com/Yeldur" },
  { name: "Salt", role: "Lead Balancer, Lead Item Crafter, Moderator", profile: "https://github.com/Erdertus" },
  { name: "rfl", role: "Map Design Lead, Map Designer", profile: "https://github.com/rf-l" },
  { name: "Telford", role: "Item Crafter", profile: "https://github.com/Telfordski" },
  { name: "Truthful", role: "Item Crafter", profile: "https://github.com/Truthful33" },
  { name: "Diggles", role: "Item Crafter", profile: "https://github.com/Diggles27" },
  { name: "AngryMob", role: "Item Crafter", profile: "https://github.com/AngryMob010" },
  { name: "kaikaikai", role: "Item Crafter", profile: "https://github.com/kaikaikaia" },
  { name: "abearirl", role: "Item Crafter", profile: "https://github.com/abearirlcrpg" },
  { name: "AppleTruce", role: "Item Crafter", profile: "https://github.com/AppleTruce" },
];

const getGithubUsername = (url: string): string | null => {
  const match = url?.match(/github\.com\/([\w-]+)/);
  return match ? match[1] : null;
};

const getGithubProfileImage = (url: string): string => {
  const username = getGithubUsername(url);
  return username ? `https://github.com/${username}.png` : '';
};
</script>
