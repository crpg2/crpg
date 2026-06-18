<script setup lang="ts"> 
    import type { TableColumn } from '@nuxt/ui' 

    import type { ThemeViewModel } from '~/models/theme' 

    defineProps<{ 
       themes: ThemeViewModel[] 
       loading?: boolean 
    }>() 

    defineEmits<{ 
       edit: [theme: ThemeViewModel] 
       delete: [theme: ThemeViewModel] 
    }>() 

    const { t } = useI18n() 

    const columns: TableColumn<ThemeViewModel>[] = [
       { accessorKey: 'name', header: () => t('theme.table.column.name') },
       { id: 'actions' },
    ]
 </script> 

 <template> 
    <UTable 
        :data="themes" 
        :columns 
        :loading 
        class="rounded-md border border-muted" 
    > 
        <template #actions-cell="{ row }"> 
            <div class="flex items-center justify-end gap-2"> 
                <UButton 
                    size="sm" 
                    color="neutral" 
                    variant="subtle" 
                    :label="$t('theme.action.edit')" 
                    @click="$emit('edit', row.original)" 
                /> 

                <AppConfirmActionPopover 
                    :confirm-label="$t('action.delete')" 
                    :title="$t('theme.delete.title')" 
                    @confirm="$emit('delete', row.original)" 
                > 
                    <UButton 
                        size="sm" 
                        color="error" 
                        variant="subtle" 
                        :label="$t('action.delete')" 
                    /> 

                    <template #description-content> 
                        {{ $t('theme.delete.confirm') }} 
                    </template> 
                </AppConfirmActionPopover> 
            </div> 
        </template> 

        <template #empty> 
            <UiResultNotFound /> 
        </template> 
    </UTable> 
 </template>