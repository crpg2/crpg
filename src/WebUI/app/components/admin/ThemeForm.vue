<script setup lang="ts"> 
    import type { ThemeViewModel } from '~/models/theme' 
    
    const props = defineProps<{ 
       theme?: ThemeViewModel | null 
       loading?: boolean 
    }>() 
 
    const emit = defineEmits<{ 
       submit: [data: { name: string }] 
    }>() 
 
    const model = reactive({ name: props.theme?.name ?? '' }) 
 
    watch(() => props.theme, (theme) => { 
       model.name = theme?.name ?? '' 
    }) 
 
    const onSubmit = () => emit('submit', { name: model.name.trim() }) 
 </script> 

 <template> 
    <UForm 
        :state="model" 
        class="space-y-8" 
        @submit="onSubmit" 
    > 
        <UFormField required :label="$t('theme.form.field.name.label')"> 
            <UInput 
                v-model="model.name" 
                size="xl" 
                class="w-full" 
                :maxlength="100" 
                required 
            /> 
        </UFormField> 

        <div class="flex justify-end"> 
            <UButton 
                type="submit" 
                size="xl" 
                variant="subtle" 
                :loading 
                :label="$t('action.save')" 
            /> 
        </div> 
    </UForm> 
 </template>