<script setup lang="ts"> 
    import type { TableColumn } from '@nuxt/ui' 

    import type { ThemeEventViewModel } from '~/models/theme' 

    defineProps<{ 
       events: ThemeEventViewModel[] 
       loading?: boolean 
    }>() 

    defineEmits<{ 
       edit: [event: ThemeEventViewModel] 
       delete: [event: ThemeEventViewModel] 
    }>() 

    const { t, d } = useI18n() 

    const columns: TableColumn<ThemeEventViewModel>[] = [ 
       { accessorKey: 'id', header: () => t('theme.event.table.column.id') }, 
       { accessorKey: 'name', header: () => t('theme.event.table.column.name') }, 
       { 
           id: 'theme', 
           header: () => t('theme.event.table.column.theme'), 
           cell: ({ row }) => row.original.eventTheme?.name ?? '—', 
       }, 
       { 
           accessorKey: 'goldMultiplier', 
           header: () => t('theme.event.table.column.goldMultiplier'), 
           cell: ({ row }) => `×${row.original.goldMultiplier}`, 
       }, 
       { 
           accessorKey: 'expMultiplier', 
           header: () => t('theme.event.table.column.expMultiplier'), 
           cell: ({ row }) => `×${row.original.expMultiplier}`, 
       }, 
       { 
           accessorKey: 'activeFromUtc', 
           header: () => t('theme.event.table.column.activeFrom'), 
           cell: ({ row }) => d(new Date(row.original.activeFromUtc), 'short'), 
       }, 
       { 
           accessorKey: 'activeUntilUtc', 
           header: () => t('theme.event.table.column.activeUntil'), 
           cell: ({ row }) => row.original.activeUntilUtc ? d(new Date(row.original.activeUntilUtc), 'short') : '∞', 
       }, 
       { id: 'actions' }, 
    ] 
 </script> 

 <template> 
    <UTable 
        :data="events" 
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
                    :title="$t('theme.event.delete.title')" 
                    @confirm="$emit('delete', row.original)" 
                > 
                    <UButton 
                        size="sm" 
                        color="error" 
                        variant="subtle" 
                        :label="$t('action.delete')" 
                    /> 

                    <template #description-content> 
                        {{ $t('theme.event.delete.confirm') }} 
                    </template> 
                </AppConfirmActionPopover> 
            </div> 
        </template> 
    
        <template #empty> 
            <UiResultNotFound /> 
        </template> 
    </UTable> 
 </template>