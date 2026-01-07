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
  passwordHash: string;
  status: PlayerStatus;
  subscriptionTier: SubscriptionTier;
  subscriptionEnds?: Date;
  createdAt: Date;
  updatedAt: Date;
  lastLoginAt?: Date;
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
  createdAt: Date;
  updatedAt: Date;
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
  lastEventTime: Date;
  createdAt: Date;
  updatedAt: Date;
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
  requiredRelationship?: boolean;
  minStats?: {
    health?: number;
    happiness?: number;
    wealth?: number;
    intelligence?: number;
    charisma?: number;
    physical?: number;
    creativity?: number;
  };
  weightMultiplier?: number;
  followUpEvent?: string;
  cooldownYears?: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  decisions?: DecisionTemplate[];
}

export enum DecisionOutcomeType {
  STAT_BOOST = 'STAT_BOOST',
  STAT_PENALTY = 'STAT_PENALTY',
  WEALTH_CHANGE = 'WEALTH_CHANGE',
  RELATIONSHIP_CHANGE = 'RELATIONSHIP_CHANGE',
  CAREER_CHANGE = 'CAREER_CHANGE',
  SPECIAL_REWARD = 'SPECIAL_REWARD',
}

export interface DecisionTemplate {
  id: string;
  eventId: string;
  text: string;
  order: number;
  outcomes: {
    healthBoost?: number;
    healthPenalty?: number;
    happinessBoost?: number;
    happinessPenalty?: number;
    wealthChange?: number;
    intelligenceBoost?: number;
    charismaBoost?: number;
    physicalBoost?: number;
    creativityBoost?: number;
    careerChange?: string;
    relationshipChange?: boolean;
    successChance?: number;
    specialReward?: string;
  };
  createdAt: Date;
  updatedAt: Date;
}

export interface PlayerEvent {
  id: string;
  gameId: string;
  templateId: string;
  template?: EventTemplate;
  yearOccurred: number;
  monthOccurred: number;
  ageOccurred: number;
  createdAt: Date;
  decisions?: PlayerDecision[];
}

export interface PlayerDecision {
  id: string;
  gameId: string;
  eventId: string;
  event?: PlayerEvent;
  templateId: string;
  template?: DecisionTemplate;
  outcomes: Record<string, unknown>;
  createdAt: Date;
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
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface PlayerCareer {
  id: string;
  playerId: string;
  careerId: string;
  career?: Career;
  level: number;
  currentSalary: number;
  yearsInCareer: number;
  isCurrent: boolean;
  startedAt: Date;
  endedAt?: Date;
  createdAt: Date;
  updatedAt: Date;
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
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface PlayerAchievement {
  id: string;
  playerId: string;
  achievementId: string;
  achievement?: Achievement;
  unlockedAt: Date;
  createdAt: Date;
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
  createdAt: Date;
}
