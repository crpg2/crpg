<script setup lang="ts"> 
    import type { NavigationMenuItem } from '@nuxt/ui' 

    import { ROLE } from '~/models/role' 

    definePageMeta({ 
        roles: [ROLE.Admin], 
    }) 

    const route = useRoute() 
    const { t } = useI18n() 

    const navigationItems = computed<NavigationMenuItem[]>(() => [ 
        { 
            label: t('nav.main.Admin'), 
            to: { name: 'admin' }, 
            active: route.name === 'admin', 
        }, 
        { 
            label: t('theme.title'), 
            to: { name: 'admin-themes' }, 
        }, 
    ]) 
 </script> 

 <template> 
    <div class="space-y-6 py-6"> 
        <UNavigationMenu 
            color="primary" 
            class="w-full justify-center" 
            :items="navigationItems" 
        /> 
            <NuxtPage /> 
    </div> 
 </template>