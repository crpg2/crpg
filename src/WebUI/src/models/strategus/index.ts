import { type Party, type PartyCommon } from '@/models/strategus/party';
import { type SettlementPublic } from '@/models/strategus/settlement';

export interface StrategusUpdate {
  party: Party;
  visibleParties: PartyCommon[];
  visibleSettlements: SettlementPublic[];
}

export enum MovementType {
  Move = 'Move',
  Follow = 'Follow',
  Attack = 'Attack',
}

export enum MovementTargetType {
  Party = 'Party',
  Settlement = 'Settlement',
}
