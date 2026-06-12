export const ADDONS = [
  'Altoholic',
  'Auctionator',
  'Bagnon',
  'NovaWorldBuffs',
  'TitanGold',
] as const;

export type Addon = (typeof ADDONS)[number];
export type Faction = 'Alliance' | 'Horde';

/** Full character record used in React state (includes client-side id). */
export interface Character {
  id: string;
  characterName: string;
  realmName: string;
  account: string;
  faction: Faction;
  enabled: boolean;
}

/** Shape written to / read from config.json (no client-side id). */
export interface ConfigCharacter {
  characterName: string;
  realmName: string;
  account: string;
  faction: Faction;
  enabled: boolean;
}

export interface AppConfig {
  wtfPath: string;
  characters: ConfigCharacter[];
}

export interface CharacterEntry {
  name: string;
  realm: string;
}

export interface AccountInfo {
  name: string;
  characters: CharacterEntry[];
}

/** What the PushDialog needs to open. */
export interface PushTarget {
  addon: Addon;
  /** 'CharacterName-RealmName-Account' or 'all' */
  character: string;
  /** Human-readable label for the dialog header. */
  displayName: string;
}

export const ADDON_DESCRIPTIONS: Record<Addon, string | null> = {
  Altoholic: null,
  Auctionator:
    "This will copy any scan days for an item that haven't been seen by the destination account so that missing days will be merged in. This process may take a minute.",
  Bagnon:
    "This will allow you to view characters' bags/banks that are on other accounts.",
  NovaWorldBuffs:
    "This will allow you to view characters' world buffs and raid lockouts that are on other accounts.",
  TitanGold:
    "This will allow you to view characters' gold that are on other accounts.",
};
