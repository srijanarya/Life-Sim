using UnityEngine;

namespace LifeCraft.Data
{
    [System.Serializable]
    public class Player
    {
        public string Id;
        public string Email;
        public string Username;
        public PlayerStatus Status;
        public SubscriptionTier SubscriptionTier;
        public string SubscriptionEnds;
        public string CreatedAt;
        public string UpdatedAt;
        public string LastLoginAt;
    }

    [System.Serializable]
    public enum PlayerStatus
    {
        Active,
        Suspended,
        Banned
    }

    [System.Serializable]
    public enum SubscriptionTier
    {
        Free,
        VipMonthly,
        VipYearly
    }

    [System.Serializable]
    public class PlayerProfile
    {
        public string Id;
        public string PlayerId;
        public int Age;
        public int Health;
        public int Happiness;
        public int Wealth;
        public int Intelligence;
        public int Charisma;
        public int Physical;
        public int Creativity;
        public int TotalPlaytime;
        public int GamesPlayed;
    }

    [System.Serializable]
    public class GameState
    {
        public string Id;
        public string PlayerId;
        public int CurrentYear;
        public int CurrentMonth;
        public int CurrentAge;
        public bool IsInRelationship;
        public string RelationshipId;
        public string CareerId;
        public int CareerLevel;
        public string LastEventTime;
    }

    [System.Serializable]
    public class EventTemplate
    {
        public string Id;
        public string Title;
        public string Description;
        public EventType EventType;
        public EventRarity Rarity;
        public int MinAge;
        public int MaxAge;
        public string RequiredCareer;
        public DecisionTemplate[] Decisions;
    }

    [System.Serializable]
    public enum EventType
    {
        LifeEvent,
        CareerEvent,
        RelationshipEvent,
        RandomEvent,
        DailyChallenge
    }

    [System.Serializable]
    public enum EventRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [System.Serializable]
    public class DecisionTemplate
    {
        public string Id;
        public string EventId;
        public string Text;
        public int Order;
        public string OutcomesJson;
    }

    [System.Serializable]
    public class PlayerEvent
    {
        public string Id;
        public string GameId;
        public string TemplateId;
        public int YearOccurred;
        public int MonthOccurred;
        public int AgeOccurred;
        public string CreatedAt;
    }

    [System.Serializable]
    public class PlayerDecision
    {
        public string Id;
        public string GameId;
        public string EventId;
        public string TemplateId;
        public string OutcomesJson;
        public string CreatedAt;
    }

    [System.Serializable]
    public class Career
    {
        public string Id;
        public string Name;
        public string Description;
        public CareerType Type;
        public int MinIntelligence;
        public int MinCreativity;
        public int MinCharisma;
        public int BaseSalary;
        public int MaxSalary;
        public int PromotionYears;
    }

    [System.Serializable]
    public enum CareerType
    {
        Corporate,
        Creative,
        Tech,
        Medical,
        Legal,
        Education,
        Entertainment,
        Entrepreneur
    }

    [System.Serializable]
    public class Achievement
    {
        public string Id;
        public string Name;
        public string Description;
        public AchievementType Type;
        public string Icon;
        public int RewardXp;
        public int RewardCurrency;
    }

    [System.Serializable]
    public enum AchievementType
    {
        Career,
        Wealth,
        Social,
        Longevity,
        Special
    }

    [System.Serializable]
    public class EconomyTransaction
    {
        public string Id;
        public string PlayerId;
        public TransactionType Type;
        public int Amount;
        public string Currency;
        public string ReceiptId;
        public string ProductId;
        public string MetadataJson;
        public string CreatedAt;
    }

    [System.Serializable]
    public enum TransactionType
    {
        IapPurchase,
        VipSubscription,
        RewardedAd,
        SystemGift,
        Refund
    }
}
