export default defineAppConfig({
  // icon: {
  // size: '1.5rem', // TODO:
  // },
  ui: {
    colors: {
      primary: 'crpg',
      secondary: 'zinc', // TODO:
      neutral: 'neutral',
    },
    icons: {
      // arrowLeft: 'i-lucide-arrow-left',
      // arrowRight: 'i-lucide-arrow-right',
      check: 'crpg:check',
      // chevronDoubleLeft: 'i-lucide-chevrons-left',
      // chevronDoubleRight: 'i-lucide-chevrons-right',
      chevronDown: 'crpg:chevron-down',
      chevronLeft: 'crpg:chevron-left',
      chevronRight: 'crpg:chevron-right',
      chevronUp: 'crpg:chevron-up',
      close: 'crpg:close',
      // ellipsis: 'i-lucide-ellipsis',
      // external: 'i-lucide-arrow-up-right',
      // folder: 'i-lucide-folder',
      // folderOpen: 'i-lucide-folder-open',
      loading: 'crpg:loading',
      // minus: 'i-lucide-minus',
      // plus: 'i-lucide-plus',
      search: 'crpg:search',
    },
    table: {
      slots: {
        th: 'text-2xs text-muted',
        td: 'text-xs text-inherit',
        tbody: '[&>tr]:data-[selectable=true]:cursor-pointer',
      },
    },
    modal: {
      slots: {
        wrapper: 'w-full',
        title: 'text-center text-lg',
        close: '-right-4 -top-4',
      },
    },
    input: {
      variants: {
        size: {
          xl: {
            base: 'py-[0.938rem] px-[1.125rem] h-[3.375rem]',
          },
          lg: {
            base: 'py-[0.938rem] px-4 h-[3rem]',
          },
          md: {
            base: 'p-3 h-[2.625rem]',
          },
          sm: {
            base: 'py-[0.563rem] px-2.5h-[2.25rem]',
          },
          xs: {
            base: 'p-1.5 h-[1.875rem]',
          },
        },
      },
      // compoundVariants: {

      // },
    },
    dropdownMenu: {
      variants: {
        size: {
          xl: {
            item: 'px-2 py-2 gap-2.5 !text-title-md',
            itemTrailingIcon: 'size-6',
            itemLeadingIcon: 'size-6',
          },
          lg: {
            item: 'px-2 py-2 gap-2.5 !text-title-md',
            itemTrailingIcon: 'size-[1.125rem]',
            itemLeadingIcon: 'size-[1.125rem]',
          },
          md: {
            item: 'px-2 py-1.5 gap-2 !text-title-sm ',
            itemTrailingIcon: 'size-[1.125rem]',
            itemLeadingIcon: 'size-[1.125rem]',
          },
          sm: {
            item: 'px-2 py-1.5 gap-2 !text-title-sm',
            itemTrailingIcon: 'size-[1.125rem]',
            itemLeadingIcon: 'size-[1.125rem]',
          },
          xs: {
            item: 'px-1.5 py-1 gap-2 !text-title-sm',
            itemTrailingIcon: 'size-4',
            itemLeadingIcon: 'size-4',
          },
        },
      },
    },
    popover: {
      slots: {
        content: 'rounded-lg px-4 py-3.5',
      },
    },
    tooltip: {
      slots: {
        content: 'rounded-lg px-4 py-3.5 h-auto',
      },
    },
    button: {
      slots: {
        base: '!leading-none rounded-full group font-bold',
      },
      variants: {
        variant: {
          outline: '',
        },
        size: {
          xl: {
            base: 'py-[0.938rem] px-[1.125rem] !text-title-md h-[3.375rem]',
            trailingIcon: 'size-6',
            leadingIcon: 'size-6',
          },
          lg: {
            base: 'py-[0.938rem] px-4 !text-title-md h-[3rem]',
            trailingIcon: 'size-[1.125rem]',
            leadingIcon: 'size-[1.125rem]',
          },
          md: {
            base: 'p-3 !text-title-sm h-[2.625rem]',
            trailingIcon: 'size-[1.125rem]',
            leadingIcon: 'size-[1.125rem]',
          },
          sm: {
            base: 'py-[0.563rem] px-2.5 !text-title-sm h-[2.25rem]',
            trailingIcon: 'size-[1.125rem]',
            leadingIcon: 'size-[1.125rem]',
          },
          xs: {
            base: 'p-1.5 !text-title-sm h-[1.875rem]',
            trailingIcon: 'size-[1.125rem]',
            leadingIcon: 'size-[1.125rem]',
          },
        },
      },
      compoundVariants: [
        {
          color: 'primary',
          variant: 'outline',
          class: 'ring-2 ring-default',
        },
        {
          color: 'secondary',
          variant: 'solid',
          class: 'bg-base-200 text-content-200 hover:text-content-100 hover:bg-base-300',
        },
        {
          color: 'secondary',
          variant: 'outline',
          class: 'ring-2 text-content-200 hover:text-content-100 group-hover:ring-secondary/100',
        },
        {
          size: 'xl',
          square: true,
          class: 'p-[0.938rem]',
        },
        {
          size: 'lg',
          square: true,
          class: 'p-[0.938rem]',
        },
        {
          size: 'md',
          square: true,
          class: 'p-3',
        },
        {
          size: 'sm',
          square: true,
          class: 'p-[0.563rem]',
        },
        {
          size: 'xs',
          square: true,
          class: 'p-1.5',
        },
      ],
    },
    toast: {
      slots: {
        root: 'rounded-full',
      },
      variants: {
        color: {
          success: {
            root: 'bg-status-success',
            progress: 'opacity-0',
          },
        },
      },
    },
  },
})
