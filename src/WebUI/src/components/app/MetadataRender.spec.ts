import { mount } from '@vue/test-utils'

import { type ActivityLog, ActivityLogType } from '~/models/activity-logs'

import MetadataRender from './MetadataRender.vue'

const getWrapper = (activityLog: ActivityLog) =>
  mount(MetadataRender, {
    shallow: true,
    global: {
      renderStubDefaultSlot: true,
      stubs: {
        'i18n-t': {
          template: `<div data-aq-i18n-t-stub>
                      <slot />
                      <slot name="item" />
                      <slot name="price" />
                      <slot name="gold" />
                      <slot name="heirloomPoints" />
                      <slot name="itemId" />
                      <slot name="experience" />
                      <slot name="damage" />
                      <slot name="targetUserId" />
                      <slot name="instance" />
                      <slot name="oldName" />
                      <slot name="newName" />
                      <slot name="characterId" />
                      <slot name="generation" />
                      <slot name="level" />
                      <slot name="message" />
                    </div>`,
        },
        'VTooltip': {
          template: `<div>
                      <slot />
                      <slot name="popper" />
                    </div>`,
        },
      },
    },
    props: {
      activityLog,
      keypath: `some.tpl.${activityLog.type}`,
      dict: {
        characters: [],
        clans: [],
        users: [],
      },
    },
  })

it('gold, heirloomPoints, itemId, experience', () => {
  // TODO: FIXME: fix spec
  // eslint-disable-next-line unused-imports/no-unused-vars
  const wrapper = getWrapper({
    id: 1,
    createdAt: new Date('2023-03-30T18:00:00.0000000Z'),
    metadata: {
      experience: '1000',
      gold: '10',
      heirloomPoints: '100',
      itemId: 'mlp_armor',
    },
    type: ActivityLogType.ItemUpgraded,
    userId: 2,
  })

  //   expect(wrapper.find('[data-aq-i18n-t-stub]').attributes('keypath')).toEqual(
  //     'activityLog.tpl.ItemUpgraded',
  //   )

//   expect(wrapper.find('[data-aq-addLogItem-tpl-goldPrice]').exists()).toBeTruthy()
//   expect(wrapper.findComponent({ name: 'Coin' }).props('value')).toEqual(10)
//   expect(wrapper.find('[data-aq-addLogItem-tpl-heirloomPoints]').exists()).toBeTruthy()
//   expect(wrapper.find('[data-aq-addLogItem-tpl-itemId]').exists()).toBeTruthy()
//   expect(wrapper.find('[data-aq-addLogItem-tpl-experience]').exists()).toBeTruthy()
})

// it('team hit', async () => {
//   const wrapper = getWrapper({
//     createdAt: new Date('2023-03-30T18:00:00.0000000Z'),
//     id: 1,
//     metadata: {
//       damage: '10',
//       targetUserId: '1',
//     },
//     type: ActivityLogType.TeamHit,
//     userId: 2,
//   })

//   expect(wrapper.classes('self-start')).toBeDefined()

//   expect(wrapper.find('[data-aq-i18n-t-stub]').attributes('keypath')).toEqual(
//     'activityLog.tpl.TeamHit',
//   )

//   const userMediaComponents = wrapper.findAllComponents({ name: 'UserMedia' })
//   expect(userMediaComponents.length).toEqual(2)
//   expect(userMediaComponents.at(0)!.props('user')).toEqual({ id: 2 })
//   expect(userMediaComponents.at(1)!.props('user')).toEqual({ id: 1 })

//   expect(wrapper.find('[data-aq-addLogItem-tpl-damage]')).toBeDefined()

//   const addUserBtn = wrapper.find('[data-aq-addLogItem-addUser-btn]')
//   await addUserBtn.trigger('click')
//   expect(wrapper.emitted().addUser[0]).toEqual([1])
// })

// it('add type', async () => {
//   const wrapper = getWrapper({
//     createdAt: new Date('2023-03-30T18:00:00.0000000Z'),
//     id: 1,
//     metadata: {},
//     type: ActivityLogType.ChatMessageSent,
//     userId: 2,
//   })

//   const addTagTrigger = wrapper.find('[data-aq-addLogItem-type]')
//   await addTagTrigger.trigger('click')

//   expect(wrapper.emitted().addType[0]).toEqual([ActivityLogType.ChatMessageSent])
// })
