import type { TableColumn } from '@nuxt/ui'
import type { Row, SortingState } from '@tanstack/vue-table'
import type { TreeItemSelectEvent, TreeItemToggleEvent } from 'reka-ui'
import type { Ref } from 'vue'

import { getCoreRowModel, getFacetedRowModel, getFacetedUniqueValues, getFilteredRowModel, getSortedRowModel, useVueTable } from '@tanstack/vue-table'
import { refDebounced } from '@vueuse/core'

import type { ItemFlat, ItemType, WeaponClass } from '~/models/item'

import { itemTypeOrder, weaponClassOrder } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemUpgrades, getWeaponClassesByItemType, itemTypeToIcon, weaponClassToIcon } from '~/services/item-service'

export interface TreeNode<TItem extends ItemFlat = ItemFlat> {
  key: string
  level: 'type' | 'weaponClass' | 'item' | 'upgrade'
  label?: string
  icon?: string
  count?: number
  item?: TItem
  disabled?: boolean
  children?: TreeNode<TItem>[]
}

export interface UseItemSelectTreeOptions<TItem extends ItemFlat, TSelected> {
  selectedItem: Ref<TSelected | null>
  fetchItems: () => Promise<TItem[]>
  getItemKey: (item: TItem) => string
  mapToSelected: (item: TItem) => TSelected
  isSelectedEqual: (selected: TSelected, item: TItem) => boolean
  supportsUpgrades?: boolean
  isItemDisabled?: (item: TItem) => boolean
}

export function useItemSelectTree<TItem extends ItemFlat, TSelected>(
  options: UseItemSelectTreeOptions<TItem, TSelected>,
) {
  const {
    selectedItem,
    fetchItems,
    getItemKey,
    mapToSelected,
    isSelectedEqual,
    supportsUpgrades = false,
    isItemDisabled,
  } = options

  const selectedNode = shallowRef<TreeNode<TItem> | undefined>(undefined)
  const expandedNodes = ref<string[]>([])

  const searchModel = ref<string>('')
  const debouncedSearchModel = refDebounced(searchModel, 250)

  const { t } = useI18n()
  const treeRef = useTemplateRef('tree')

  const {
    state: items,
    isLoading: loadingItems,
  } = useAsyncState(fetchItems, [] as TItem[], { onSuccess: expandSelectedItem })

  const columns: TableColumn<TItem>[] = [
    { accessorKey: 'id', enableGlobalFilter: false },
    { accessorKey: 'name' },
    { accessorKey: 'price', enableGlobalFilter: false },
    { accessorKey: 'type', enableGlobalFilter: false },
    { accessorKey: 'weaponClass', enableGlobalFilter: false },
  ]

  const sorting = ref<SortingState>([
    { id: 'price', desc: true },
  ])

  const grid = useVueTable({
    get data() {
      return items.value
    },
    columns,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getFacetedRowModel: getFacetedRowModel(),
    getFacetedUniqueValues: getFacetedUniqueValues(),
    filterFns: {
      includesSome,
    },
    getRowId: row => row.id,
    state: {
      get globalFilter() {
        return debouncedSearchModel.value
      },
      get sorting() {
        return sorting.value
      },
    },
  })

  // Upgrades (only used when supportsUpgrades is true)
  const upgradesByItemNodeKey = ref<Record<string, ItemFlat[]>>({})
  const loadingUpgradesNodeKeys = ref<Set<string>>(new Set())

  const setLoadingNodeUpgrades = (itemBaseId: string, isLoading: boolean) => {
    if (isLoading) {
      loadingUpgradesNodeKeys.value.add(itemBaseId)
    }
    else {
      loadingUpgradesNodeKeys.value.delete(itemBaseId)
    }
    loadingUpgradesNodeKeys.value = new Set(loadingUpgradesNodeKeys.value)
  }

  async function loadItemUpgrades(itemBaseId: string) {
    if (itemBaseId in upgradesByItemNodeKey.value) {
      return
    }

    setLoadingNodeUpgrades(itemBaseId, true)
    try {
      upgradesByItemNodeKey.value = {
        ...upgradesByItemNodeKey.value,
        [itemBaseId]: createItemIndex(await getItemUpgrades(itemBaseId)).toSpliced(0, 1),
      }
    }
    finally {
      setLoadingNodeUpgrades(itemBaseId, false)
    }
  }

  function getItemNode(item: TItem): TreeNode<TItem> {
    return {
      level: 'item',
      key: getItemKey(item),
      label: item.name,
      item,
      disabled: isItemDisabled ? isItemDisabled(item) : false,
      children: [],
    }
  }

  function getChildrenItems(sortedRows: Row<TItem>[], type: ItemType, weaponClass?: WeaponClass): TreeNode<TItem>[] {
    return sortedRows
      .filter(item => item.original.type === type && (!weaponClass || item.original.weaponClass === weaponClass))
      .map((row) => {
        const node = getItemNode(row.original)

        if (supportsUpgrades) {
          node.children = (upgradesByItemNodeKey.value[row.original.baseId] || []).map(upgrade => ({
            ...getItemNode(upgrade as TItem),
            level: 'upgrade' as const,
          }))
        }

        return node
      })
  }

  const treeItems = computed<TreeNode<TItem>[]>(() => {
    const facetsByType = [...grid.getColumn('type')?.getFacetedUniqueValues() || []]
    const facetsByWeaponClass = [...grid.getColumn('weaponClass')?.getFacetedUniqueValues() || []]
    const sortedRows = grid.getSortedRowModel().rows

    return facetsByType
      .sort(([a], [b]) => itemTypeOrder.get(a)! - itemTypeOrder.get(b)!)
      .map(([type, count]) => {
        const weaponClasses = getWeaponClassesByItemType(type)

        const children = weaponClasses.length
          ? facetsByWeaponClass
              .filter(([weaponClass]) => weaponClasses.includes(weaponClass as WeaponClass))
              .sort(([a], [b]) => weaponClassOrder.get(a)! - weaponClassOrder.get(b)!)
              .map<TreeNode<TItem>>(([weaponClass, count]) => {
                return {
                  level: 'weaponClass',
                  key: weaponClass,
                  label: t(`item.weaponClass.${weaponClass}`),
                  icon: weaponClassToIcon[weaponClass as WeaponClass],
                  count,
                  children: getChildrenItems(sortedRows, type, weaponClass),
                }
              })
          : getChildrenItems(sortedRows, type)

        return {
          level: 'type' as const,
          key: type,
          label: t(`item.type.${type}`),
          icon: itemTypeToIcon[type as ItemType],
          count,
          children,
        }
      })
  })

  const visibleNodes = computed<TreeNode<TItem>[]>(() => {
    const result: TreeNode<TItem>[] = []
    const expandedKeys = new Set(expandedNodes.value)

    function walk(nodes: TreeNode<TItem>[]) {
      for (const node of nodes) {
        result.push(node)

        if (node.children?.length && expandedKeys.has(node.key)) {
          walk(node.children)
        }
      }
    }

    walk(treeItems.value)

    return result
  })

  function estimateTreeItemSize(index: number) {
    const node = visibleNodes.value[index]
    if (!node || node.level === 'type' || node.level === 'weaponClass') {
      return 52
    }

    return 82
  }

  function getTreeItemOffset(index: number) {
    let offset = 0
    for (let i = 0; i < index; i++) {
      offset += estimateTreeItemSize(i)
    }

    return offset
  }

  // TODO: need to use scrollToIndex from tanstack, for this it needs to be exposed from reka-ui/nuxt-ui
  async function scrollToNode(key: string) {
    await nextTick()

    const index = visibleNodes.value.findIndex(node => node.key === key)
    if (index === -1) {
      return
    }

    const scrollElement = (treeRef.value as any)?.$el
    if (!scrollElement) {
      return
    }

    const itemSize = estimateTreeItemSize(index)
    const itemOffset = getTreeItemOffset(index)
    const targetOffset = Math.max(0, itemOffset - ((scrollElement.clientHeight - itemSize) / 2))

    scrollElement.scrollTo({
      top: targetOffset,
      behavior: 'smooth',
    })
  }

  async function expandSelectedItem() {
    if (!selectedItem.value) {
      return
    }

    if (supportsUpgrades && (selectedItem.value as any).rank > 0) {
      await loadItemUpgrades((selectedItem.value as any).baseId)
    }

    const expandedKeys = findExpandedKeysByItemId(treeItems.value, (selectedItem.value as any).id)

    if (expandedKeys.length) {
      expandedNodes.value = expandedKeys

      let item: TItem | undefined
      if (supportsUpgrades && (selectedItem.value as any).rank > 0 && upgradesByItemNodeKey.value[(selectedItem.value as any).baseId]) {
        item = upgradesByItemNodeKey.value[(selectedItem.value as any).baseId]!.find(
          i => i.id === (selectedItem.value as any).id,
        ) as TItem | undefined
      }

      if (!item) {
        item = items.value.find(i => i.id === (selectedItem.value as any).id)
      }

      if (item) {
        selectedNode.value = getItemNode(item)
        scrollToNode(selectedNode.value!.key)
      }
    }
  }

  function findExpandedKeysByItemId(nodes: TreeNode<TItem>[], itemId: string, parentKeys: string[] = []): string[] {
    for (const node of nodes) {
      if (node.item?.id === itemId) {
        return parentKeys
      }

      if (node.children?.length) {
        const expandedKeys = findExpandedKeysByItemId(node.children, itemId, [...parentKeys, node.key])
        if (expandedKeys.length) {
          return expandedKeys
        }
      }
    }

    return []
  }

  function expandNodesRecursive() {
    const parentKeys: string[] = []
    for (const typeNode of treeItems.value) {
      parentKeys.push(typeNode.key)

      if (typeNode.children) {
        for (const child of typeNode.children) {
          if (child.level === 'weaponClass') {
            parentKeys.push(child.key)
          }
        }
      }
    }
    expandedNodes.value = parentKeys
  }

  watch(debouncedSearchModel, () => {
    if (!debouncedSearchModel.value) {
      expandedNodes.value = []
    }

    if (debouncedSearchModel.value.length < 3) {
      return
    }

    expandNodesRecursive()
  })

  function onToggle(e: TreeItemToggleEvent<TreeNode<TItem>>) {
    if (!supportsUpgrades) {
      return
    }

    const node = e.detail.value
    if (node?.level === 'item' && node.item) {
      loadItemUpgrades(node.item.baseId)
    }
  }

  function onSelect(e: TreeItemSelectEvent<TreeNode<TItem>>) {
    if (e.detail.originalEvent.type === 'click') {
      e.preventDefault()
    }
  }

  function onChange(node: TreeNode<TItem>) {
    if (node?.disabled) {
      return
    }

    selectedNode.value = node

    if (!node || !node.item || (selectedItem.value && isSelectedEqual(selectedItem.value, node.item))) {
      selectedItem.value = null
      return
    }

    selectedItem.value = mapToSelected(node.item)
  }

  return {
    searchModel,
    loadingItems,
    treeItems,
    expandedNodes,
    selectedNode,
    estimateTreeItemSize,
    loadingUpgradesNodeKeys,
    onToggle,
    onChange,
    onSelect,
  }
}
