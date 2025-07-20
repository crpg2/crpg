export default defineAppConfig({
  ui: {
    colors: {
      primary: 'crpg-gold',
      // secondary: 'crpg-stone',
      neutral: 'crpg-neutral',
      success: 'crpg-green',
      error: 'crpg-red',
      warning: 'crpg-amber',
    },
    icons: {
      arrowLeft: 'crpg:arrow-left',
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
      minus: 'crpg:minus',
      plus: 'crpg:plus',
      search: 'crpg:search',
    },
    table: {
      slots: {
        th: 'text-sm text-default',
        td: 'text-sm text-inherit',
        tbody: '[&>tr]:data-[selectable=true]:cursor-pointer',

      },
    },
    tabs: {
      variants: {
        size: {
          md: {
            trigger: 'h-[2.125rem]', // same height (42px) with MD input and button
          },
        },
      },
    },
    card: {
      variants: {
        variant: {
          outline: {
            root: 'bg-transparent ring ring-default divide-y divide-default',
          },
          subtle: {
            root: 'bg-elevated/50',
          },
        },
      },
    },
    modal: {
      slots: {
        overlay: 'bg-elevated/10 backdrop-blur',
        content: '!overflow-visible',
        wrapper: 'w-full',
        header: 'p-8 sm:px-6',
        body: 'p-6 sm:p-8',
        footer: 'p-8 sm:px-6',
        title: 'text-center text-lg',
        // close: '-right-4 -top-4',
      },
    },
    formField: {
      slots: {
        label: 'text-muted',
      },
      variants: {
        size: {
          md: {
            root: 'text-sm',
            label: 'text-sm',
            hint: 'text-xs',
            help: 'text-xs',
            description: 'text-xs',
            error: 'text-xs',
          },
        },
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
    },
    // select: {
    //   variants: {
    //     size: {
    //       md: {
    //         // TODO: FIXME:
    //         // base: 'h-[2.125rem] px-2.5 py-1.5 text-xs gap-1.5',
    //         // base: 'text-xs',

    //         // item: 'text-xs',
    //       },
    //     },
    //   },
    // },
    toast: {
      slots: {
        description: 'text-xs',
      },
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
            item: 'px-2 py-1.5 gap-2 !text-title-md',
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
        content: 'text-sm text-default rounded-md px-4 py-3.5 h-auto',
      },
    },
    tooltip: {
      slots: {
        content: 'text-sm text-default rounded-md px-4 py-3.5 h-auto',
      },
    },
    slider: {
      slots: {
        thumb: 'rounded-[.188rem] origin-left rotate-45 transform bg-primary !ring-default ring-2 focus-visible:outline-2 focus-visible:outline-offset-2',
      },
    },
    button: {
      slots: {
        // base: '!leading-none rounded-full group font-bold',
      },
      variants: {
        variant: {
          outline: '',
        },
        size: {
          // xl: {
          //   base: 'py-[0.938rem] px-[1.125rem] !text-title-md h-[3.375rem]',
          //   trailingIcon: 'size-6',
          //   leadingIcon: 'size-6',
          // },
          // lg: {
          //   base: 'py-[0.938rem] px-4 !text-title-md h-[3rem]',
          //   trailingIcon: 'size-[1.125rem]',
          //   leadingIcon: 'size-[1.125rem]',
          // },
          // md: {
          //   base: 'p-3 !text-title-sm h-[2.625rem]',
          //   trailingIcon: 'size-[1.125rem]',
          //   leadingIcon: 'size-[1.125rem]',
          // },
          // sm: {
          //   base: 'py-[0.563rem] px-2.5 !text-title-sm h-[2.25rem]',
          //   trailingIcon: 'size-[1.125rem]',
          //   leadingIcon: 'size-[1.125rem]',
          // },
          // xs: {
          //   base: 'p-1.5 !text-title-sm h-[1.875rem]',
          //   trailingIcon: 'size-[1.125rem]',
          //   leadingIcon: 'size-[1.125rem]',
          // },
        },
      },
      compoundVariants: [
        {
          color: 'primary',
          variant: 'outline',
          class: 'ring-default',
        },
        // {
        //   size: 'xl',
        //   square: true,
        //   class: 'p-[0.938rem]',
        // },
        // {
        //   size: 'lg',
        //   square: true,
        //   class: 'p-[0.938rem]',
        // },
        // {
        //   size: 'md',
        //   square: true,
        //   class: 'p-3',
        // },
        // {
        //   size: 'sm',
        //   square: true,
        //   class: 'p-[0.563rem]',
        // },
        // {
        //   size: 'xs',
        //   square: true,
        //   class: 'p-1.5',
        // },
      ],
    },
    badge: {
      compoundVariants: [
        {
          color: 'primary',
          variant: 'outline',
          class: 'ring-default',
        },
      ],
    },
  },
})
