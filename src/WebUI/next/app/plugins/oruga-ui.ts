import { type IconDefinition, library } from '@fortawesome/fontawesome-svg-core'
import {
  FontAwesomeIcon,
  FontAwesomeLayers,
  FontAwesomeLayersText,
} from '@fortawesome/vue-fontawesome'
import {
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
import VueSlider from 'vue-slider-component'

export default defineNuxtPlugin((nuxtApp) => {
  Object.values(
    import.meta.glob<IconDefinition>('../assets/themes/oruga-tailwind/icons/**/*.ts', {
      eager: true,
      import: 'default',
    }),
  ).forEach((ic) => {
    library.add(ic)
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
    .component('VueSlider', VueSlider)
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
})
