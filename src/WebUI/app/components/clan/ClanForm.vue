<script setup lang="ts">
import type { RadioGroupItem, SelectItem } from '@nuxt/ui'

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

import { LANGUAGE } from '~/models/language'
import { REGION } from '~/models/region'
import {
  clanBannerKeyPattern,
  clanTagPattern,
  discordLinkPattern,
  errorMessagesToString,
  integer,
  maxLength,
  minLength,
  minValue,
  required,
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
      region: REGION.Eu,
      secondaryColor: rgbHexColorToArgbInt('#000000'),
      tag: '',
    }),
  },
)

const emit = defineEmits<{
  submit: [Omit<Clan, 'id'>]
}>()

const { t } = useI18n()
const toast = useToast()

const clanFormModel = ref<Omit<Clan, 'id'>>(props.clan)

const $v = useVuelidate(
  {
    name: {
      required: required(),
      maxLength: maxLength(clanNameMaxLength),
      minLength: minLength(clanNameMinLength),
    },
    tag: {
      required: required(),
      clanTagPattern: clanTagPattern(),
      maxLength: maxLength(clanTagMaxLength),
      minLength: minLength(clanTagMinLength),
    },
    description: {
      maxLength: maxLength(clanDescriptionMaxLength),
    },
    bannerKey: {
      required: required(),
      clanBannerKeyPattern: clanBannerKeyPattern(),
      maxLength: maxLength(clanBannerKeyMaxLength),
    },
    discord: {
      discordLinkPattern: discordLinkPattern(),
    },
    armoryTimeout: {
      integer: integer(),
      minValue: minValue(1),
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
    toast.add({
      title: t('form.validate.invalid'),
      close: false,
      color: 'warning',
    })
    return
  }

  emit('submit', {
    ...clanFormModel.value,
    discord: clanFormModel.value.discord === '' ? null : clanFormModel.value.discord,
  })
}
</script>

<template>
  <form data-aq-clan-form @submit.prevent="onSubmit">
    <div class="mb-6 space-y-6">
      <UiCard icon="crpg:hash" :label="$t('clan.update.form.field.mainInfo')">
        <div class="grid grid-cols-2 gap-4">
          <UFormField
            :label="$t('clan.update.form.field.name')"
            :error="errorMessagesToString($v.name.$errors)"
            data-aq-clan-form-field="name"
            size="xl"
            required
          >
            <UInput
              v-model="clanFormModel.name"
              :minlength="clanNameMinLength"
              :maxlength="clanNameMaxLength"
              aria-describedby="clan-name-count"
              class="w-full"
              data-aq-clan-form-input="name"
            >
              <template #trailing>
                <UiInputCounter
                  id="clan-name-count"
                  :current="clanFormModel.name.length"
                  :max="clanNameMaxLength"
                />
              </template>
            </UInput>
          </UFormField>

          <UFormField
            :label="$t('clan.update.form.field.tag')"
            :error="errorMessagesToString($v.tag.$errors)"
            data-aq-clan-form-field="tag"
            size="xl"
            required
          >
            <UInput
              v-model="clanFormModel.tag"
              :minlength="clanTagMinLength"
              :maxlength="clanTagMaxLength"
              aria-describedby="clan-tag-count"
              class="w-full"
              data-aq-clan-form-input="tag"
            >
              <template #trailing>
                <UiInputCounter
                  id="clan-tag-count"
                  :current="clanFormModel.tag.length"
                  :max="clanTagMaxLength"
                />
              </template>
            </UInput>
          </UFormField>

          <UFormField
            :label="`${$t('clan.update.form.field.description')} (${$t('form.field.optional')})`"
            :error="errorMessagesToString($v.description.$errors)"
            class="col-span-2"
            size="xl"
            data-aq-clan-form-field="description"
          >
            <UTextarea
              v-model="clanFormModel.description"
              autoresize
              :maxlength="clanDescriptionMaxLength"
              data-aq-clan-form-input="description"
              aria-describedby="clan-description-count"
              class="w-full"
            />
            <template #hint>
              <UiInputCounter
                id="clan-description-count"
                :current="clanFormModel.description.length"
                :max="clanDescriptionMaxLength"
              />
            </template>
          </UFormField>
        </div>
      </UiCard>

      <UiCard icon="crpg:region" :label="$t('region-title')">
        <div class="space-y-6">
          <URadioGroup
            v-model="clanFormModel.region"
            size="xl"
            :items="Object.values(REGION).map<RadioGroupItem>((region) => ({
              label: $t(`region.${region}`, 0),
              value: region,
            }))"
            data-aq-clan-form-input="region"
          />

          <UFormField :label="$t('clan.update.form.field.languages')" size="xl">
            <USelect
              v-model="clanFormModel.languages"
              multiple
              :items="Object.values(LANGUAGE).map<SelectItem>((language) => ({
                label: `${$t(`language.${language}`)} - ${language}`,
                value: language,
              }))"
              :placeholder="$t('action.selectFromList')"
              class="w-full"
            />
          </UFormField>
        </div>
      </UiCard>

      <UiCard icon="crpg:banner" :label="$t('clan.update.form.field.appearance')">
        <div class="space-y-6">
          <UFormField
            :error="errorMessagesToString($v.bannerKey.$errors)"
            :label="$t('clan.update.form.field.bannerKey')"
            data-aq-clan-form-field="bannerKey"
            size="xl"
            required
          >
            <template #help>
              <i18n-t
                scope="global"
                keypath="clan.update.bannerKeyGeneratorTools"
              >
                <template #link>
                  <ULink
                    href="https://bannerlord.party"
                    target="_blank"
                  >
                    bannerlord.party
                  </ULink>
                </template>
              </i18n-t>
            </template>

            <template #hint>
              <UiInputCounter
                id="clan-bannerKey-count"
                :current="clanFormModel.bannerKey.length"
                :max="clanBannerKeyMaxLength"
              />
            </template>

            <UTextarea
              v-model="clanFormModel.bannerKey"
              :maxlength="clanBannerKeyMaxLength"
              aria-describedby="clan-bannerKey-count"
              class="w-full"
              autoresize
              data-aq-clan-form-input="bannerKey"
            />
          </UFormField>

          <UFormField
            size="xl"
            :label="$t('clan.update.form.field.colors')"
          >
            <ClanColorSelect
              v-model:primary="primaryColorModel"
              v-model:secondary="secondaryColorModel"
              :color="clanFormModel.primaryColor"
            />
          </UFormField>
        </div>
      </UiCard>

      <UiCard icon="crpg:discord" :label="$t('clan.update.form.field.social')">
        <UFormField
          :error="errorMessagesToString($v.discord.$errors)"
          data-aq-clan-form-field="discord"
          size="xl"
        >
          <UInput
            v-model="clanFormModel.discord"
            :model-modifiers="{ nullable: true }"
            :maxlength="clanBannerKeyMaxLength"
            class="w-full"
            icon="crpg:discord"
            :placeholder="`${$t('clan.update.form.field.discord')} (${$t('form.field.optional')})`"
            data-aq-clan-form-input="discord"
          />
        </UFormField>
      </UiCard>

      <UiCard icon="crpg:armory" :label="$t('clan.update.form.group.armory.label')">
        <UFormField
          :label="$t('clan.update.form.group.armory.field.armoryTimeout.label')"
          :error="errorMessagesToString($v.armoryTimeout.$errors)"
          :help="$t('clan.update.form.group.armory.field.armoryTimeout.hint')"
          size="xl"
          data-aq-clan-form-field="armoryTimeout"
        >
          <UInputNumber
            :model-value="parseTimestamp(Number(clanFormModel.armoryTimeout)).days"
            :maxlength="clanBannerKeyMaxLength"
            class="w-32"
            color="neutral"
            data-aq-clan-form-input="armoryTimeout"
            @update:model-value="(days) => { clanFormModel.armoryTimeout = daysToMs(days || 0) }"
          />
        </UFormField>
      </UiCard>
    </div>

    <div
      class="
        sticky bottom-0 left-0 flex w-full items-center justify-center gap-2 py-4 backdrop-blur-sm
      "
    >
      <UButton
        v-if="clanId === undefined"
        type="submit"
        size="xl"
        :label="$t('action.create')"
        data-aq-clan-form-action="create"
      />
      <template v-else>
        <UButton
          :to="{ name: 'clans-id', params: { id: clanId } }"
          variant="outline"
          color="neutral"
          size="xl"
          :label="$t('action.cancel')"
          data-aq-clan-form-action="cancel"
        />
        <UButton
          size="xl"
          :label="$t('action.save')"
          type="submit"
          data-aq-clan-form-action="save"
        />
      </template>
    </div>
  </form>
</template>
