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
  <form
    data-aq-clan-form
    @submit.prevent="onSubmit"
  >
    <div class="mb-6 space-y-6">
      <UCard variant="outline">
        <template #header>
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:hash" class="size-6" />
            </template>
            <div class="text-sm">
              {{ $t('clan.update.form.field.mainInfo') }}
            </div>
          </UiDataCell>
        </template>

        <div class="grid grid-cols-2 gap-4">
          <UFormField
            :label="$t('clan.update.form.field.name')"
            :error="errorMessagesToString($v.name.$errors)"
            data-aq-clan-form-field="name"
          >
            <UInput
              v-model="clanFormModel.name"
              :minlength="clanNameMinLength"
              :maxlength="clanNameMaxLength"
              size="sm"
              aria-describedby="clan-name-count"
              class="w-full"
              data-aq-clan-form-input="name"
            >
              <!-- TODO: to cmp -->
              <template #trailing>
                <div
                  id="clan-name-count"
                  class="text-2xs text-muted tabular-nums"
                  aria-live="polite"
                  role="status"
                >
                  {{ clanFormModel.name.length }}/{{ clanNameMaxLength }}
                </div>
              </template>
            </UInput>
          </UFormField>

          <UFormField
            :label="$t('clan.update.form.field.tag')"
            :error="errorMessagesToString($v.tag.$errors)"
            data-aq-clan-form-field="tag"
          >
            <UInput
              v-model="clanFormModel.tag"
              :minlength="clanTagMinLength"
              :maxlength="clanTagMaxLength"
              size="sm"
              aria-describedby="clan-tag-count"
              class="w-full"
              data-aq-clan-form-input="tag"
            >
              <template #trailing>
                <div
                  id="clan-tag-count"
                  class="text-2xs text-muted tabular-nums"
                  aria-live="polite"
                  role="status"
                >
                  {{ clanFormModel.tag.length }}/{{ clanTagMaxLength }}
                </div>
              </template>
            </UInput>
          </UFormField>

          <UFormField
            :label="`${$t('clan.update.form.field.description')} (${$t('form.field.optional')})`"
            :error="errorMessagesToString($v.description.$errors)"
            class="col-span-2"
            data-aq-clan-form-field="description"
          >
            <UTextarea
              v-model="clanFormModel.description"
              :rows="5"
              :maxlength="clanDescriptionMaxLength"
              data-aq-clan-form-input="description"
              aria-describedby="clan-description-count"
              size="sm"
              class="w-full"
            >
              <template #trailing>
                <div
                  id="clan-description-count"
                  class="text-2xs text-muted tabular-nums"
                  aria-live="polite"
                  role="status"
                >
                  {{ clanFormModel.description.length }}/{{ clanDescriptionMaxLength }}
                </div>
              </template>
            </UTextarea>
          </UFormField>
        </div>
      </UCard>

      <UCard
        variant="outline"
        :ui="{ body: 'grid grid-cols-2 gap-4' }"
      >
        <template #header>
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:region" class="size-6" />
            </template>
            <div class="text-sm">
              {{ $t('region-title') }}
            </div>
          </UiDataCell>
        </template>

        <div class="space-y-6">
          <URadioGroup
            v-model="clanFormModel.region"
            :items="Object.values(REGION).map<RadioGroupItem>((region) => ({
              label: $t(`region.${region}`, 0),
              value: region,
            }))"
            data-aq-clan-form-input="region"
          />

          <UFormField :label="$t('clan.update.form.field.languages')">
            <USelect
              v-model="clanFormModel.languages"
              multiple
              :items="Object.values(LANGUAGE).map<SelectItem>((language) => ({
                label: `${$t(`language.${language}`)} - ${language}`,
                value: language,
              }))"
              class="w-full"
            />
          </UFormField>
        </div>
      </UCard>

      <UCard
        variant="outline"
        :ui="{
          body: 'space-y-6',
        }"
      >
        <template #header>
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:banner" class="size-6" />
            </template>
            <div class="text-sm">
              {{ $t('clan.update.form.field.appearance') }}
            </div>
          </UiDataCell>
        </template>

        <UFormField
          :error="errorMessagesToString($v.bannerKey.$errors)"
          :label="$t('clan.update.form.field.bannerKey')"
          data-aq-clan-form-field="bannerKey"
        >
          <template #hint>
            <i18n-t
              scope="global"
              keypath="clan.update.bannerKeyGeneratorTools"
              tag="div"
              class="!text-content-200"
            >
              <template #link>
                <a
                  href="https://bannerlord.party"
                  target="_blank"
                  class="hover:text-primary"
                >
                  bannerlord.party
                </a>
              </template>
            </i18n-t>
          </template>

          <UInput
            v-model="clanFormModel.bannerKey"
            :maxlength="clanBannerKeyMaxLength"
            size="sm"
            aria-describedby="clan-bannerKey-count"
            class="w-full"
            data-aq-clan-form-input="bannerKey"
          >
            <!-- TODO: to cmp -->
            <template #trailing>
              <div
                id="clan-bannerKey-count"
                class="text-2xs text-muted tabular-nums"
                aria-live="polite"
                role="status"
              >
                {{ clanFormModel.bannerKey.length }}/{{ clanBannerKeyMaxLength }}
              </div>
            </template>
          </UInput>
        </UFormField>

        <UFormField>
          <template #label>
            <div class="mb-4 flex items-center gap-2">
              <ClanTagIcon
                :color="clanFormModel.primaryColor"
                size="lg"
              />
              {{ $t('clan.update.form.field.colors') }}
            </div>
          </template>

          <div class="grid grid-cols-2 gap-4">
            <UFormField
              :label="`${$t('clan.update.form.field.primaryColor')}:`"
              :ui="{
                root: 'flex items-center gap-2',
                container: 'mt-0 flex items-center gap-2',
              }"
            >
              <div class="text-content-100">
                {{ primaryColorModel }}
              </div>
              <input
                v-model="primaryColorModel"
                variant="none"
                type="color"
                data-aq-clan-form-input="primaryColor"
              >
            </UFormField>

            <UFormField
              :label="`${$t('clan.update.form.field.secondaryColor')}:`"
              :ui="{
                root: 'flex items-center gap-2',
                container: 'mt-0 flex items-center gap-2',
              }"
            >
              <div class="text-content-100">
                {{ secondaryColorModel }}
              </div>
              <input
                v-model="secondaryColorModel"
                variant="none"
                type="color"
                data-aq-clan-form-input="secondaryColor"
              >
            </UFormField>
          </div>
        </UFormField>
      </UCard>

      <UCard
        variant="outline"
        :ui="{
          body: 'grid grid-cols-2 gap-4',
        }"
      >
        <template #header>
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:discord" class="size-6" />
            </template>
            <div class="text-sm">
              {{ $t('clan.update.form.field.discord') }}
            </div>
          </UiDataCell>
        </template>

        <UFormField
          :error="errorMessagesToString($v.discord.$errors)"
          data-aq-clan-form-field="discord"
        >
          <UInput
            v-model="clanFormModel.discord"
            :maxlength="clanBannerKeyMaxLength"
            size="sm"
            class="w-full"
            :placeholder="`${$t('clan.update.form.field.discord')} (${$t('form.field.optional')})`"
            data-aq-clan-form-input="discord"
          />
        </UFormField>
      </UCard>

      <UCard
        variant="outline"
        :ui="{
          body: 'grid grid-cols-2 gap-4',
        }"
      >
        <template #header>
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:armory" class="size-6" />
            </template>
            <div class="text-sm">
              {{ $t('clan.update.form.group.armory.label') }}
            </div>
          </UiDataCell>
        </template>

        <UFormField
          :label="$t('clan.update.form.group.armory.field.armoryTimeout.label')"
          :error="errorMessagesToString($v.armoryTimeout.$errors)"
          :help="$t('clan.update.form.group.armory.field.armoryTimeout.hint')"
          data-aq-clan-form-field="armoryTimeout"
        >
          <UInputNumber
            :model-value="parseTimestamp(Number(clanFormModel.armoryTimeout)).days"
            :maxlength="clanBannerKeyMaxLength"
            size="sm"
            class="w-36"
            data-aq-clan-form-input="armoryTimeout"
            @update:model-value="(days) => (clanFormModel.armoryTimeout = daysToMs(days))"
          />
        </UFormField>
      </UCard>
    </div>

    <div class="flex items-center justify-center gap-4">
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
          color="secondary"
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
