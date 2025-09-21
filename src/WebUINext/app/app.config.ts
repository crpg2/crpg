export default defineAppConfig({
  pollIntevalMs: 5000,
  // pollIntevalMs: 1000 * 60 // 1 min,
  settings: {
    discord: '',
    steam: '',
    patreon: '',
    github: '',
    reddit: '',
    modDb: '',
  },
  links: {
    tipsTricksHelpThread: 'https://discord.com/channels/279063743839862805/1044686358066778112',
    buildSupportThread: 'https://discord.com/channels/279063743839862805/1036085650849550376',
  },
  ui: {
    colors: {
      primary: 'crpg-gold',
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
    alert: {
      slots: {
        title: 'text-base font-semibold',
        description: 'text-base font-semibold',
      },
    },
    table: {
      slots: {
        th: 'text-default',
        td: 'text-inherit',
        tbody: '[&>tr]:data-[selectable=true]:cursor-pointer',
      },
    },
    tabs: {
      compoundVariants: [
        {
          color: 'neutral',
          variant: 'pill',
          class: {
            indicator: 'bg-accented',
            trigger: 'data-[state=active]:text-accented',
          },
        },
      ],
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
        header: 'p-6',
        body: 'p-6',
        footer: 'p-6',
        title: 'text-center h3',
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
    toast: {
      slots: {
        description: 'text-xs',
      },
    },
    popover: {
      slots: {
        content: 'text-sm text-default rounded-md px-3.5 py-3.5 h-auto',
      },
    },
    tooltip: {
      slots: {
        content: 'text-sm text-default rounded-md px-3.5 py-3.5 h-auto max-w-80',
        text: 'overflow-auto whitespace-normal',
      },
    },
    slider: {
      slots: {
        thumb: 'rounded-[.225rem] origin-left rotate-45 transform bg-primary !ring-default ring-2 focus-visible:outline-2 focus-visible:outline-offset-2',
      },
    },
    button: {
      compoundVariants: [
        {
          color: 'primary',
          variant: 'outline',
          class: 'ring-default',
        },
        {
          color: 'neutral',
          variant: 'outline',
          class: 'bg-transparent',
        },
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
    link: {
      variants: {
        active: {
          true: 'text-primary',
          false: 'text-primary',
        },
        disabled: {
          true: 'cursor-not-allowed opacity-75',
        },
      },
      compoundVariants: [
        {
          active: false,
          disabled: false,
          class: [
            'hover:text-primary/75',
            'transition-colors',
          ],
        },
      ],
    },
  },
})
