export enum PlayerStatus {
  ACTIVE = 'ACTIVE',
  SUSPENDED = 'SUSPENDED',
  BANNED = 'BANNED',
}

export enum SubscriptionTier {
  FREE = 'FREE',
  VIP_MONTHLY = 'VIP_MONTHLY',
  VIP_YEARLY = 'VIP_YEARLY',
}

export interface Player {
  id: string;
  email: string;
  username: string;
  status: PlayerStatus;
  subscriptionTier: SubscriptionTier;
  subscriptionEnds?: string;
  createdAt: string;
  updatedAt: string;
  lastLoginAt?: string;
}

export interface PlayerProfile {
  id: string;
  playerId: string;
  age: number;
  health: number;
  happiness: number;
  wealth: number;
  intelligence: number;
  charisma: number;
  physical: number;
  creativity: number;
  totalPlaytime: number;
  gamesPlayed: number;
}

export interface GameState {
  id: string;
  playerId: string;
  currentYear: number;
  currentMonth: number;
  currentAge: number;
  isInRelationship: boolean;
  relationshipId?: string;
  careerId?: string;
  careerLevel: number;
  lastEventTime: string;
}

export enum EventType {
  LIFE_EVENT = 'LIFE_EVENT',
  CAREER_EVENT = 'CAREER_EVENT',
  RELATIONSHIP_EVENT = 'RELATIONSHIP_EVENT',
  RANDOM_EVENT = 'RANDOM_EVENT',
  DAILY_CHALLENGE = 'DAILY_CHALLENGE',
}

export enum EventRarity {
  COMMON = 'COMMON',
  UNCOMMON = 'UNCOMMON',
  RARE = 'RARE',
  EPIC = 'EPIC',
  LEGENDARY = 'LEGENDARY',
}

export interface EventTemplate {
  id: string;
  title: string;
  description: string;
  eventType: EventType;
  rarity: EventRarity;
  minAge: number;
  maxAge: number;
  requiredCareer?: string;
}

export interface DecisionTemplate {
  id: string;
  eventId: string;
  text: string;
  order: number;
  outcomes: Record<string, unknown>;
}

export interface PlayerEvent {
  id: string;
  gameId: string;
  templateId: string;
  yearOccurred: number;
  monthOccurred: number;
  ageOccurred: number;
  createdAt: string;
}

export interface PlayerDecision {
  id: string;
  gameId: string;
  eventId: string;
  templateId: string;
  outcomes: Record<string, unknown>;
  createdAt: string;
}

export enum CareerType {
  CORPORATE = 'CORPORATE',
  CREATIVE = 'CREATIVE',
  TECH = 'TECH',
  MEDICAL = 'MEDICAL',
  LEGAL = 'LEGAL',
  EDUCATION = 'EDUCATION',
  ENTERTAINMENT = 'ENTERTAINMENT',
  ENTREPRENEUR = 'ENTREPRENEUR',
}

export interface Career {
  id: string;
  name: string;
  description: string;
  type: CareerType;
  minIntelligence: number;
  minCreativity: number;
  minCharisma: number;
  baseSalary: number;
  maxSalary: number;
  promotionYears: number;
}

export interface PlayerCareer {
  id: string;
  playerId: string;
  careerId: string;
  level: number;
  currentSalary: number;
  yearsInCareer: number;
  isCurrent: boolean;
  startedAt: string;
  endedAt?: string;
}

export enum AchievementType {
  CAREER = 'CAREER',
  WEALTH = 'WEALTH',
  SOCIAL = 'SOCIAL',
  LONGEVITY = 'LONGEVITY',
  SPECIAL = 'SPECIAL',
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  type: AchievementType;
  icon?: string;
  rewardXp: number;
  rewardCurrency: number;
}

export interface PlayerAchievement {
  id: string;
  playerId: string;
  achievementId: string;
  unlockedAt: string;
}

export enum TransactionType {
  IAP_PURCHASE = 'IAP_PURCHASE',
  VIP_SUBSCRIPTION = 'VIP_SUBSCRIPTION',
  REWARDED_AD = 'REWARDED_AD',
  SYSTEM_GIFT = 'SYSTEM_GIFT',
  REFUND = 'REFUND',
}

export interface EconomyTransaction {
  id: string;
  playerId: string;
  type: TransactionType;
  amount: number;
  currency: string;
  receiptId?: string;
  productId?: string;
  metadata?: Record<string, unknown>;
  createdAt: string;
}

export interface LeaderboardEntry {
  id: string;
  playerId: string;
  score: number;
  rank?: number;
  season: string;
}
