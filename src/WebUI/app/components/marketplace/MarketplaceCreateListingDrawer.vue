<script setup lang="ts">
import useVuelidate from '@vuelidate/core'
import { helpers } from '@vuelidate/validators'
import { marketplaceGoldFeePercent, marketplaceListingDurationDays } from '~root/data/constants.json'

import type { ItemFlat, SelectedItem, SelectedUserItem, UserItemFlat } from '~/models/item'

import { useUser } from '~/composables/user/use-user'
import { calculateFixedListingFee, calculateGoldCommissionFee, calculateMaxOfferGold, calculateMaxRequestGold, canListingItem, canListingUserItem } from '~/services/marketplace-service'

interface OfferModel {
  gold: number
  heirloomPoints: number
  userItem: SelectedUserItem | null
}

interface RequestModel {
  gold: number
  heirloomPoints: number
  item: SelectedItem | null
}

const emit = defineEmits<{
  close: [
    value: boolean,
    offer?: {
      offered: OfferModel
      requested: RequestModel
    },
  ]
}>()

const offerModel = ref<OfferModel>({
  gold: 0,
  heirloomPoints: 0,
  userItem: null,
})

const requestModel = ref<RequestModel>({
  gold: 0,
  heirloomPoints: 0,
  item: null,
})

const toast = useToast()
const { t } = useI18n()

const hasTradeValue = helpers.withMessage(
  () => t('marketplace.createListing.validation.hasTradeValue'),
  (value: OfferModel | RequestModel) => {
    const hasItem = 'userItem' in value ? !!value.userItem : !!value.item
    return hasItem || value.gold > 0 || value.heirloomPoints > 0
  },
)

const $v = useVuelidate(
  {
    offerModel: { hasTradeValue },
    requestModel: { hasTradeValue },
  },
  { offerModel, requestModel },
)

const fixedListingFee = calculateFixedListingFee()

const goldCommissionFee = computed(() => calculateGoldCommissionFee(offerModel.value.gold + requestModel.value.gold))

const totalUpfrontGold = computed(() => offerModel.value.gold + goldCommissionFee.value + fixedListingFee)

const { user } = useUser()

const maxGold = computed(() => ({
  offer: calculateMaxOfferGold(user.value!.gold),
  request: calculateMaxRequestGold(user.value!.gold),
}))

const isOfferItemDisabled = (item: UserItemFlat) => !canListingUserItem(item.type, item.meta)
const isRequestItemDisabled = (item: ItemFlat) => !canListingItem(item.type)

const onCancel = () => emit('close', false)

const onSubmit = async () => {
  if (!(await $v.value.$validate())) {
    toast.add({
      title: t('marketplace.createListing.validation.toast'),
      color: 'warning',
      close: false,
    })
    return
  }

  emit('close', true, {
    offered: offerModel.value,
    requested: requestModel.value,
  })
}
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    handle-only
    :dismissible="false"
    :ui="{
      header: 'mb-6 flex items-center justify-center gap-4',
      container: 'w-full max-w-(--ui-container) mx-auto space-y-6',
      footer: 'flex flex-row gap-4 justify-between',
    }"
  >
    <template #header>
      <div class="flex flex-1 flex-wrap items-center justify-center gap-4">
        <UiTextView variant="h2">
          {{ $t('marketplace.createListing.title') }}
        </UiTextView>
        <AppCoin :value="user!.gold" size="xl" />
        <AppLoom :point="user!.heirloomPoints" size="xl" />
      </div>

      <div class="mr-0 ml-auto">
        <UButton
          color="neutral" variant="ghost" icon="i-lucide-x"
          @click="onCancel"
        />
      </div>
    </template>

    <template #body>
      <div class="grid grid-cols-[1fr_auto_1fr] gap-6">
        <MarketplaceCreateListingSide
          v-model:gold="offerModel.gold"
          v-model:heirloom-points="offerModel.heirloomPoints"
          :label="$t('marketplace.createListing.offer.label')"
          :gold-options="{
            max: maxGold.offer,
            disabled: !!requestModel.gold,
            tooltipText: $t('marketplace.createListing.offer.goldMutualExclusion'),
          }"
          :heirloom-points-options="{
            max: user!.heirloomPoints,
            disabled: !!requestModel.heirloomPoints,
            tooltipText: $t('marketplace.createListing.offer.heirloomPointsMutualExclusion'),
          }"
        >
          <template #item-select>
            <UserItemSelect
              v-model="offerModel.userItem"
              :label="$t('marketplace.createListing.offer.selectItem')"
              :is-item-disabled="isOfferItemDisabled"
            >
              <template #item-top-right="{ node }">
                <template v-if="node.item">
                  <UTooltip
                    v-if="node.item.meta.isBroken"
                    :text="$t('character.inventory.item.broken.tooltip.title')"
                  >
                    <UBadge
                      variant="subtle"
                      color="warning"
                      icon="crpg:error"
                      class="cursor-default"
                    />
                  </UTooltip>

                  <UTooltip
                    v-if="node.item.meta.isPersonal"
                    :text="$t('marketplace.createListing.personal')"
                  >
                    <UBadge
                      variant="subtle"
                      color="warning"
                      icon="crpg:vip"
                      class="cursor-default"
                    />
                  </UTooltip>

                  <UTooltip
                    v-if="node.item.meta.clanArmoryLender"
                    :text="$t('character.inventory.item.clanArmory.inArmory.title')"
                  >
                    <UBadge
                      variant="subtle"
                      color="warning"
                      icon="crpg:armory"
                      class="cursor-default"
                    />
                  </UTooltip>

                  <UTooltip
                    v-if="!canListingItem(node.item!.type)"
                    :text="$t('item.nonTradeable')"
                  >
                    <UBadge
                      variant="subtle"
                      color="warning"
                      icon="crpg:trade-banned"
                    />
                  </UTooltip>
                </template>
              </template>
            </UserItemSelect>
          </template>
        </MarketplaceCreateListingSide>

        <USeparator
          orientation="vertical"
          class="self-center"
          size="sm"
          icon="crpg:trade"
          :ui="{ icon: 'size-7' }"
        />

        <MarketplaceCreateListingSide
          v-model:gold="requestModel.gold"
          v-model:heirloom-points="requestModel.heirloomPoints"
          :label="$t('marketplace.createListing.request.label')"
          :gold-options="{
            max: maxGold.request,
            disabled: !!offerModel.gold,
            tooltipText: $t('marketplace.createListing.request.goldMutualExclusion'),
          }"
          :heirloom-points-options="{
            max: 100,
            disabled: !!offerModel.heirloomPoints,
            tooltipText: $t('marketplace.createListing.request.heirloomPointsMutualExclusion'),
          }"
        >
          <template #item-select>
            <ItemSelect
              v-model="requestModel.item"
              :label="$t('marketplace.createListing.request.selectItem')"
              :is-item-disabled="isRequestItemDisabled"
            >
              <template #item-top-right="{ node }">
                <UTooltip
                  v-if="node.disabled && node.item && !canListingItem(node.item.type)"
                  :text="$t('item.nonTradeable')"
                >
                  <UBadge
                    variant="subtle"
                    color="warning"
                    icon="crpg:trade-banned"
                  />
                </UTooltip>
              </template>
            </ItemSelect>
          </template>
        </MarketplaceCreateListingSide>
      </div>
    </template>

    <template #footer>
      <UiCard
        :ui="{ body: 'grid grid-cols-[1fr_auto_1fr] gap-6 items-end' }"
        variant="outline"
        class="w-full"
      >
        <div>
          <UiSimpleTableRow
            :label="$t('marketplace.createListing.fees.listingFee.label')"
            :tooltip="{
              title: $t('marketplace.createListing.fees.listingFee.label'),
              description: $t('marketplace.createListing.fees.listingFee.description', { days: marketplaceListingDurationDays }),
              validation: $t('marketplace.createListing.fees.listingFee.nonRefundable'),
            }"
          >
            <AppCoin :value="fixedListingFee" />
          </UiSimpleTableRow>

          <UiSimpleTableRow
            :label="$t('marketplace.createListing.fees.goldCommission.label')"
            :tooltip="{
              title: $t('marketplace.createListing.fees.goldCommission.label'),
              description: $t('marketplace.createListing.fees.goldCommission.description', { percent: marketplaceGoldFeePercent }),
            }"
          >
            <AppCoin :value="goldCommissionFee" />
          </UiSimpleTableRow>

          <UiSimpleTableRow
            :label="$t('marketplace.createListing.fees.totalUpfront.label')"
            :tooltip="{
              title: $t('marketplace.createListing.fees.totalUpfront.label'),
              description: $t('marketplace.createListing.fees.totalUpfront.description'),
            }"
          >
            <AppCoin :value="totalUpfrontGold" />
          </UiSimpleTableRow>
        </div>

        <div />

        <div class="flex gap-6">
          <UButton
            :label="$t('action.cancel')"
            block
            color="neutral"
            variant="soft"
            @click="onCancel"
          />

          <AppConfirmActionPopover @confirm="onSubmit">
            <UButton
              :label="$t('action.create')"
              block
              color="primary"
              variant="soft"
            />
          </AppConfirmActionPopover>
        </div>
      </UiCard>
    </template>
  </UDrawer>
</template>
