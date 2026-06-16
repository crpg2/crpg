<script setup lang="ts"> 
    import type { SelectItem } from '@nuxt/ui' 
    
    import type { ThemeEventFormData, ThemeEventViewModel, ThemeViewModel } from '~/models/theme' 
    
    import { THEME_EQUIPMENT_SLOTS } from '~/models/theme' 
    
    const props = defineProps<{ 
       themes: ThemeViewModel[] 
       event?: ThemeEventViewModel | null 
       loading?: boolean 
    }>() 
 
    const emit = defineEmits<{ 
       submit: [data: ThemeEventFormData] 
    }>() 
 
    const toInputValue = (date?: Date | string | null): string => { 
        if (!date) { 
            return ''
        }

        const d = new Date(date) 
        const local = new Date(d.getTime() - d.getTimezoneOffset() * 60000) 
        return local.toISOString().slice(0, 16)
    }

    const fromInputValue = (local: string): Date | null => local ? new Date(local) : null
 
    const buildModel = () => ({ 
       name: props.event?.name ?? '', 
       themeId: props.event?.eventTheme?.id ?? props.themes[0]?.id ?? 0, 
       goldMultiplier: props.event?.goldMultiplier ?? 1, 
       expMultiplier: props.event?.expMultiplier ?? 1, 
       activeFromUtc: toInputValue(props.event?.activeFromUtc ?? new Date()), 
       activeUntilUtc: toInputValue(props.event?.activeUntilUtc), 
       requiredEquipmentSlotsMatchingTheme: [...(props.event?.requiredEquipmentSlotsMatchingTheme ?? [])], 
       minumumRequiredEquipmentSlotsMatchingTheme: props.event?.minumumRequiredEquipmentSlotsMatchingTheme ?? 0, 
    }) 
 
    const model = reactive(buildModel()) 
 
    watch(() => props.event, () => Object.assign(model, buildModel())) 
 
    const themeItems = computed<SelectItem[]>(() => props.themes.map(theme => ({ label: theme.name, value: theme.id }))) 
 
    const slotItems = computed<SelectItem[]>(() => THEME_EQUIPMENT_SLOTS.map(slot => ({ label: slot, value: slot }))) 
 
    const onSubmit = () => {
       emit('submit', { 
           name: model.name.trim(), 
           themeId: model.themeId, 
           goldMultiplier: model.goldMultiplier, 
           expMultiplier: model.expMultiplier, 
           activeFromUtc: fromInputValue(model.activeFromUtc)!, 
           activeUntilUtc: fromInputValue(model.activeUntilUtc), 
           requiredEquipmentSlotsMatchingTheme: model.requiredEquipmentSlotsMatchingTheme, 
           minumumRequiredEquipmentSlotsMatchingTheme: model.minumumRequiredEquipmentSlotsMatchingTheme, 
       }) 
    } 
 </script> 

 <template> 
    <UForm 
        :state="model" 
        class="space-y-8" 
        @submit="onSubmit" 
    > 
        <UFormField required :label="$t('theme.event.form.field.name.label')"> 
            <UInput 
                v-model="model.name" 
                size="xl" 
                class="w-full" 
                :maxlength="100" 
                required 
            /> 
        </UFormField> 

        <UFormField required :label="$t('theme.event.form.field.theme.label')"> 
            <USelect 
                v-model="model.themeId" 
                :items="themeItems" 
                size="xl" 
                class="w-full" 
            /> 
        </UFormField> 

        <div class="grid grid-cols-2 gap-4"> 
            <UFormField :label="$t('theme.event.form.field.goldMultiplier.label')"> 
                <UInputNumber 
                    v-model="model.goldMultiplier" 
                    :min="0" 
                    :step="0.1" 
                    size="xl" 
                    class="w-full" 
                /> 
            </UFormField> 

            <UFormField :label="$t('theme.event.form.field.expMultiplier.label')"> 
                <UInputNumber 
                    v-model="model.expMultiplier" 
                    :min="0" 
                    :step="0.1" 
                    size="xl" 
                    class="w-full" 
                /> 
            </UFormField> 
        </div> 

        <div class="grid grid-cols-2 gap-4"> 
            <UFormField required :label="$t('theme.event.form.field.activeFrom.label')"> 
                <UInput 
                    v-model="model.activeFromUtc" 
                    type="datetime-local" 
                    size="xl" 
                    class="w-full" 
                    required 
                /> 
            </UFormField> 

            <UFormField 
                :label="$t('theme.event.form.field.activeUntil.label')" 
                :help="$t('theme.event.form.field.activeUntil.help')" 
            > 
                <UInput 
                    v-model="model.activeUntilUtc" 
                    type="datetime-local" 
                    size="xl" 
                    class="w-full" 
                /> 
            </UFormField> 
        </div> 

        <UFormField :label="$t('theme.event.form.field.requiredSlots.label')"> 
            <USelect 
                v-model="model.requiredEquipmentSlotsMatchingTheme" 
                :items="slotItems" 
                multiple 
                size="xl" 
                class="w-full" 
            /> 
        </UFormField> 

        <UFormField 
            :label="$t('theme.event.form.field.minSlots.label')" 
            :help="$t('theme.event.form.field.minSlots.help')" 
        > 
            <UInputNumber 
                v-model="model.minumumRequiredEquipmentSlotsMatchingTheme" 
                :min="0" 
                size="xl" 
                class="w-full" 
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