import { createTestingPinia } from '@pinia/testing'
import { mount } from '@vue/test-utils'

import Login from '~/components/app/Login.vue'
import { Platform } from '~/models/platform'
import { login } from '~/services/auth-service'
import { useUserStore } from '~/stores/user'

vi.mock('~/services/auth-service', () => ({
  login: vi.fn(),
}))

const userStore = useUserStore(createTestingPinia())

describe('not logged in', () => {
  it('should be exist login btn', () => {
    const wrapper = mount(Login)

    expect(wrapper.find('[data-aq-login-btn]').exists()).toBeTruthy()
    expect(wrapper.find('[data-aq-character-link]').exists()).toBeFalsy()
  })

  it('should called login method when login btn clicked', async () => {
    const wrapper = mount(Login)

    const loginBtn = wrapper.find('[data-aq-login-btn]')
    expect(wrapper.find('[data-aq-character-link]').exists()).toBeFalsy()

    await loginBtn.trigger('click')
    expect(login).toBeCalledWith(Platform.Steam) // default platform
  })

  it('should render a few platform items', () => {
    const wrapper = mount(Login)

    const platformItems = wrapper.findAll('[data-aq-platform-item]')

    expect(platformItems.at(0)!.attributes('checked')).toEqual('true')
    expect(platformItems.at(1)!.attributes('checked')).toEqual('false')
  })

  it('change platform, then login', async () => {
    const wrapper = mount(Login)

    const platformItems = wrapper.findAll('[data-aq-platform-item]')

    await platformItems.at(1)!.trigger('click')

    const loginBtn = wrapper.find('[data-aq-login-btn]')
    await loginBtn.trigger('click')
    expect(login).toBeCalledWith(Platform.EpicGames)
  })
})

describe('logged in', () => {
  it('should be exist link to character\'s page', () => {
    userStore.$patch({ user: { id: 1 } })

    const wrapper = mount(Login)

    expect(wrapper.find('[data-aq-login-btn]').exists()).toBeFalsy()
    expect(wrapper.find('[data-aq-character-link]').exists()).toBeTruthy()
  })
})
