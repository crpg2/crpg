<script setup lang="ts">
import { useVuelidate } from '@vuelidate/core'
import {
  clanBannerKeyMaxLength,
  clanDescriptionMaxLength,
  clanNameMaxLength,
  clanNameMinLength,
  clanTagMaxLength,
  clanTagMinLength,
} from '~root/data/constants.json'

import type { Clan } from '~/models/clan'

import { Language } from '~/models/language'
import { Region } from '~/models/region'
import { NotificationType, notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import {
  clanBannerKeyPattern,
  clanTagPattern,
  discordLinkPattern,
  integer,
  maxLength,
  minLength,
  minValue,
  required,
  url,
} from '~/services/validators-service'
import { argbIntToRgbHexColor, rgbHexColorToArgbInt } from '~/utils/color'
import { daysToMs, parseTimestamp } from '~/utils/date'

const props = withDefaults(
  defineProps<{
    clanId?: number
    clan?: Omit<Clan, 'id'>
  }>(),
  {
    clan: () => ({
      armoryTimeout: daysToMs(3),
      bannerKey: '',
      description: '',
      discord: null,
      languages: [],
      name: '',
      primaryColor: rgbHexColorToArgbInt('#000000'),
      region: Region.Eu,
      secondaryColor: rgbHexColorToArgbInt('#000000'),
      tag: '',
    }),
  },
)

const emit = defineEmits<{
  submit: [Omit<Clan, 'id'>]
}>()

const clanFormModel = ref<Omit<Clan, 'id'>>(props.clan)

const $v = useVuelidate(
  {
    armoryTimeout: {
      integer,
      minValue: minValue(1),
    },
    bannerKey: {
      clanBannerKeyPattern,
      maxLength: maxLength(clanBannerKeyMaxLength),
      required,
    },
    description: {
      maxLength: maxLength(clanDescriptionMaxLength),
    },
    discord: {
      discordLinkPattern,
      url,
    },
    name: {
      maxLength: maxLength(clanNameMaxLength),
      minLength: minLength(clanNameMinLength),
      required,
    },
    tag: {
      clanTagPattern,
      maxLength: maxLength(clanTagMaxLength),
      minLength: minLength(clanTagMinLength),
      required,
    },
  },
  clanFormModel,
)

const primaryColorModel = computed({
  get() {
    return argbIntToRgbHexColor(clanFormModel.value.primaryColor)
  },
  set(value) {
    clanFormModel.value.primaryColor = rgbHexColorToArgbInt(value)
  },
})

const secondaryColorModel = computed({
  get() {
    return argbIntToRgbHexColor(clanFormModel.value.secondaryColor)
  },
  set(value) {
    clanFormModel.value.secondaryColor = rgbHexColorToArgbInt(value)
  },
})

const onSubmit = async () => {
  if (!(await $v.value.$validate())) {
    notify(t('form.validate.invalid'), NotificationType.Warning)
    return
  }

  emit('submit', {
    ...clanFormModel.value,
    discord: clanFormModel.value.discord === '' ? null : clanFormModel.value.discord,
  })
}
</script>

<template>
  <form
    data-aq-clan-form
    @submit.prevent="onSubmit"
  >
    <div class="mb-8 space-y-4">
      <FormGroup
        icon="hash"
        :label="$t('clan.update.form.field.mainInfo')"
      >
        <div class="grid grid-cols-2 gap-4">
          <OField
            v-bind="{
              ...($v.name.$error && {
                variant: 'danger',
                message: $v.name.$errors[0].$message as string,
              }),
            }"
            data-aq-clan-form-field="name"
          >
            <OInput
              v-model="clanFormModel.name"
              type="text"
              counter
              size="sm"
              expanded
              :placeholder="$t('clan.update.form.field.name')"
              :minlength="clanNameMinLength"
              :maxlength="clanNameMaxLength"
              data-aq-clan-form-input="name"
              @blur="$v.name.$touch"
              @focus="$v.name.$reset"
            />
          </OField>

          <OField
            v-bind="{
              ...($v.tag.$error && {
                variant: 'danger',
                message: $v.tag.$errors[0].$message as string,
              }),
            }"
            data-aq-clan-form-field="tag"
          >
            <OInput
              v-model="clanFormModel.tag"
              type="text"
              counter
              size="sm"
              expanded
              :placeholder="$t('clan.update.form.field.tag')"
              :min-length="clanTagMinLength"
              :maxlength="clanTagMaxLength"
              data-aq-clan-form-input="tag"
              @blur="$v.tag.$touch"
              @focus="$v.tag.$reset"
            />
          </OField>

          <OField
            class="col-span-2"
            v-bind="{
              ...($v.description.$error && {
                variant: 'danger',
                message: $v.description.$errors[0].$message as string,
              }),
            }"
            data-aq-clan-form-field="description"
          >
            <OInput
              v-model="clanFormModel.description"
              :placeholder="`${$t('clan.update.form.field.description')} (${$t(
                'form.field.optional',
              )})`"
              type="textarea"
              rows="5"
              counter
              size="sm"
              expanded
              :maxlength="clanDescriptionMaxLength"
              data-aq-clan-form-input="description"
              @blur="$v.description.$touch"
              @focus="$v.description.$reset"
            />
          </OField>
        </div>
      </FormGroup>

      <FormGroup
        icon="region"
        :label="$t('region-title')"
      >
        <div class="space-y-8">
          <OField :addons="false">
            <div class="flex flex-col gap-4">
              <ORadio
                v-for="region in Object.keys(Region)"
                :key="region"
                v-model="clanFormModel.region"
                :native-value="region"
                data-aq-clan-form-input="region"
              >
                {{ $t(`region.${region}`, 0) }}
              </ORadio>
            </div>
          </OField>

          <OField>
            <VDropdown :triggers="['click']">
              <template #default="{ shown }">
                <OButton
                  variant="secondary"
                  outlined
                  size="lg"
                >
                  {{ $t('clan.update.form.field.languages') }}
                  <div class="flex items-center gap-1.5">
                    <Tag
                      v-for="l in clanFormModel.languages"
                      :key="l"
                      v-tooltip="$t(`language.${l}`)"
                      :label="l"
                      variant="primary"
                    />
                  </div>
                  <Divider inline />
                  <OIcon
                    icon="chevron-down"
                    size="lg"
                    :rotation="shown ? 180 : 0"
                    class="text-content-400"
                  />
                </OButton>
              </template>

              <template #popper>
                <div class="max-h-64 max-w-md overflow-y-auto">
                  <DropdownItem
                    v-for="l in Object.keys(Language)"
                    :key="l"
                  >
                    <OCheckbox
                      v-model="clanFormModel.languages"
                      :native-value="l"
                      class="items-center"
                      :label="`${$t(`language.${l}`)} - ${l}`"
                    />
                  </DropdownItem>
                </div>
              </template>
            </VDropdown>
          </OField>
        </div>
      </FormGroup>

      <FormGroup>
        <template #label>
          <ClanTagIcon
            :color="clanFormModel.primaryColor"
            size="lg"
          />
          {{ $t('clan.update.form.field.colors') }}
        </template>

        <div class="grid grid-cols-2 gap-4">
          <!-- TODO: https://github.com/oruga-ui/oruga/issues/823 -->
          <OField
            :label="`${$t('clan.update.form.field.primaryColor')}:`"
            horizontal
          >
            <div class="text-content-100">
              {{ primaryColorModel }}
            </div>
            <OInput
              v-model="primaryColorModel"
              type="color"
              data-aq-clan-form-input="primaryColor"
            />
          </OField>

          <!-- TODO: https://github.com/oruga-ui/oruga/issues/823 -->
          <OField
            :label="`${$t('clan.update.form.field.secondaryColor')}:`"
            horizontal
          >
            <div class="text-content-100">
              {{ secondaryColorModel }}
            </div>
            <OInput
              v-model="secondaryColorModel"
              type="color"
              data-aq-clan-form-input="secondaryColor"
            />
          </OField>
        </div>
      </FormGroup>

      <FormGroup
        icon="banner"
        :label="$t('clan.update.form.field.bannerKey')"
      >
        <OField
          v-bind="{
            ...($v.bannerKey.$error && {
              variant: 'danger',
            }),
          }"
          data-aq-clan-form-field="bannerKey"
        >
          <template #message>
            <template v-if="$v.bannerKey.$error">
              {{ $v.bannerKey.$errors[0].$message }}
            </template>
            <template v-else>
              <i18n-t
                scope="global"
                keypath="clan.update.bannerKeyGeneratorTools"
                tag="div"
              >
                <template #link>
                  <a
                    href="https://bannerlord.party"
                    target="_blank"
                    class="text-content-link hover:text-content-link-hover"
                  >
                    bannerlord.party
                  </a>
                </template>
              </i18n-t>
            </template>
          </template>

          <OInput
            v-model="clanFormModel.bannerKey"
            counter
            expanded
            size="sm"
            :maxlength="clanBannerKeyMaxLength"
            data-aq-clan-form-input="bannerKey"
            @blur="$v.bannerKey.$touch"
            @focus="$v.bannerKey.$reset"
          />
        </OField>
      </FormGroup>

      <FormGroup
        icon="discord"
        :label="$t('clan.update.form.field.discord')"
      >
        <OField
          v-bind="{
            ...($v.discord.$error && {
              variant: 'danger',
              message: $v.discord.$errors[0].$message as string,
            }),
          }"
          data-aq-clan-form-field="discord"
        >
          <OInput
            v-model="clanFormModel.discord"
            type="text"
            size="sm"
            expanded
            :placeholder="`${$t('clan.update.form.field.discord')} (${$t('form.field.optional')})`"
            data-aq-clan-form-input="discord"
            @blur="$v.discord.$touch"
            @focus="$v.discord.$reset"
          />
        </OField>
      </FormGroup>

      <FormGroup
        icon="armory"
        :label="$t('clan.update.form.group.armory.label')"
      >
        <div class="grid grid-cols-2 gap-4">
          <OField
            data-aq-clan-form-field="armoryTimeout"
            :label="$t('clan.update.form.group.armory.field.armoryTimeout.label')"
            v-bind="{
              ...($v.armoryTimeout.$error
                ? {
                  variant: 'danger',
                  message: $v.armoryTimeout.$errors[0].$message as string,
                }
                : { message: $t('clan.update.form.group.armory.field.armoryTimeout.hint') }),
            }"
          >
            <OInput
              :model-value="parseTimestamp(clanFormModel.armoryTimeout).days"
              type="number"
              size="sm"
              expanded
              data-aq-clan-form-input="armoryTimeout"
              @update:model-value="days => (clanFormModel.armoryTimeout = daysToMs(Number(days)))"
              @blur="$v.armoryTimeout.$touch"
              @focus="$v.armoryTimeout.$reset"
            />
          </OField>
        </div>
      </FormGroup>
    </div>

    <div class="flex items-center justify-center gap-4">
      <template v-if="clanId === undefined">
        <OButton
          native-type="submit"
          variant="primary"
          size="xl"
          :label="$t('action.create')"
          data-aq-clan-form-action="create"
        />
      </template>

      <template v-else>
        <RouterLink
          :to="{ name: 'ClansId', params: { id: clanId } }"
          data-aq-clan-form-action="cancel"
        >
          <OButton
            variant="primary"
            outlined
            size="xl"
            :label="$t('action.cancel')"
          />
        </RouterLink>
        <OButton
          variant="primary"
          size="xl"
          :label="$t('action.save')"
          native-type="submit"
          data-aq-clan-form-action="save"
        />
      </template>
    </div>
  </form>
</template>
