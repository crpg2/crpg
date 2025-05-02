import type { IconDefinition } from '@fortawesome/fontawesome-svg-core'

import { library } from '@fortawesome/fontawesome-svg-core'
import {
  FontAwesomeIcon,
  FontAwesomeLayers,
  FontAwesomeLayersText,
} from '@fortawesome/vue-fontawesome'
import {
  NotificationProgrammatic,
  OButton,
  OCheckbox,
  OCollapse,
  ODatetimepicker,
  OField,
  OIcon,
  OInput,
  OLoading,
  ONotification,
  OPagination,
  ORadio,
  OrugaConfig,
  OSwitch,
  OTabItem,
  OTable,
  OTableColumn,
  OTabs,
} from '@oruga-ui/oruga-next'
import FloatingVue from 'floating-vue'
// TODO:
// import VueSlider from 'vue-slider-component'

enum NotificationType {
  Success = 'success',
  Warning = 'warning',
  Danger = 'danger',
}

const notify = (message: string, type: NotificationType = NotificationType.Success) => {
  NotificationProgrammatic.open({
    duration: 5000,
    message,
    position: 'top',
    queue: false,
    variant: type,
  })
}

export default defineNuxtPlugin((nuxtApp) => {
  Object.values(import.meta.glob<IconDefinition>('../assets/themes/oruga-tailwind/icons/**/*.ts', { eager: true, import: 'default' }))
    .forEach(ic => library.add(ic))

  nuxtApp.vueApp.use(FloatingVue, {
    disposeTimeout: 100,
    distance: 16,
  })

  nuxtApp.vueApp
    .component('OIcon', OIcon)
    .component('OButton', OButton)
    .component('OField', OField)
    .component('OCheckbox', OCheckbox)
    .component('ORadio', ORadio)
    .component('OSwitch', OSwitch)
    .component('OInput', OInput)
    .component('OTable', OTable)
    .component('OTableColumn', OTableColumn)
    .component('OTabs', OTabs)
    .component('OTabItem', OTabItem)
    .component('OLoading', OLoading)
    .component('OPagination', OPagination)
    .component('ONotification', ONotification)
    .component('OCollapse', OCollapse)
    .component('ODateTimePicker', ODatetimepicker)
    .component('FontAwesomeIcon', FontAwesomeIcon)
    .component('FontAwesomeLayers', FontAwesomeLayers)
    .component('FontAwesomeLayersText', FontAwesomeLayersText)
    // .component('VueSlider', VueSlider)
    .use(OrugaConfig, {
      // https://oruga.io/components/Icon.html
      customIconPacks: {
        crpg: {
          iconPrefix: 'fa-',
          internalIcons: {
            'close-circle': 'close',
          },
          sizes: {
            '2xl': '2xl',
            '2xs': '2xs',
            '3x': '3x',
            '4x': '4x',
            '5x': '5x',
            'default': 'sm',
            'lg': 'lg',
            'sm': 'sm',
            'xl': 'xl',
            'xs': 'xs',
          },
        },
      },
      iconComponent: 'FontAwesomeIcon',
      iconPack: 'crpg',
      useHtml5Validation: false,
    })

  return {
    provide: {
      notify,
    },
  }
})
