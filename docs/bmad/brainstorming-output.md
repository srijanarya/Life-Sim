# LifeCraft - Brainstorming Session

## Product Management Brainstorming for LifeCraft

**Date:** 2026-01-06
**Project:** LifeCraft - Real Life Simulator
**Platform:** iOS (Unity) + Node.js/TypeScript Backend

---

## BMAD Framework: Brainstorming Phase

This document captures brainstormed ideas for feature development.

---

## Engagement & Retention Ideas

### 1. Daily Challenge System

**User Story:**
As a player, I want to complete a new challenge each day, so that I have a reason to return daily and feel continuous progression.

**Acceptance Criteria:**
- New challenge appears daily with countdown timer
- Completing challenge rewards currency and XP
- Streak system tracks consecutive days
- Difficulty scales with player level

**RICE Score:**
- Reach: 100% (all players)
- Impact: 8/10 (drives daily retention)
- Confidence: 8/10 (proven in sims)
- Effort: 13/13 (medium)
- **Total: 492**

**Technical Complexity:** Medium

---

### 2. Life Milestones with Narrative

**User Story:**
As a player, I want to experience memorable life milestones (graduation, marriage, career achievements), so that my character's life feels meaningful and emotional.

**Acceptance Criteria:**
- Visual celebration animations for milestones
- Milestone gallery to replay memories
- Social sharing capability for milestones
- Milestones unlock new content/cosmetics

**RICE Score:**
- Reach: 100%
- Impact: 9/10 (core life sim experience)
- Confidence: 7/10 (high engagement value)
- Effort: 10/13 (requires content + UI)
- **Total: 630**

**Technical Complexity:** Medium

---

### 3. Legacy & Reincarnation System

**User Story:**
As a player, I want to pass on traits/skills to my next life, so that I can build a legacy over multiple playthroughs and experience different life paths.

**Acceptance Criteria:**
- End-game summary unlocks "New Life" bonus
- 2-3 traits transfer (selected by player)
- Legacy score tracks overall achievements
- Multiple generations in same save file

**RICE Score:**
- Reach: 100%
- Impact: 9/10 (huge replayability)
- Confidence: 6/10 (unknown appeal)
- Effort: 12/13 (complex system)
- **Total: 450**

**Technical Complexity:** High

---

## Monetization Ideas (Fair, Not Predatory)

### 4. Time-Skip Boost (VIP Perk)

**User Story:**
As a VIP subscriber, I want to skip waiting times between events, so that I can progress faster and focus on decision-making.

**Acceptance Criteria:**
- VIP subscribers can skip cooldowns (3x/day)
- Non-VIP players see VIP upsell
- Time-skip creates fair, cosmetic-only advantage
- Does NOT affect game balance or outcomes

**RICE Score:**
- Reach: 20% (VIP subset)
- Impact: 7/10 (convenience without P2W)
- Confidence: 9/10 (standard pattern)
- Effort: 5/13 (easy implementation)
- **Total: 252**

**Technical Complexity:** Low
**Monetization Impact:** High (VIP value)

---

### 5. Cosmetic Avatars & Themes

**User Story:**
As a player, I want to unlock avatar cosmetics and UI themes, so that I can express my personality and personalize the game.

**Acceptance Criteria:**
- IAP shop for avatar items (clothing, hairstyles, accessories)
- Seasonal themes (Halloween, holiday themes)
- Premium currency purchase
- Cosmetics are 100% optional (no gameplay advantage)

**RICE Score:**
- Reach: 80%
- Impact: 6/10 (personalization value)
- Confidence: 9/10 (proven mobile pattern)
- Effort: 6/13 (requires art assets + shop UI)
- **Total: 432**

**Technical Complexity:** Medium
**Monetization Impact:** Medium-High

---

### 6. Rewarded Ads for Currency Boosts

**User Story:**
As a free player, I want to watch ads to earn premium currency, so that I can access paid content without spending money.

**Acceptance Criteria:**
- Ad placement at natural break points (after major events)
- 2x currency reward for 30-second ad
- Daily cap: 3-4 ads
- No gameplay interruption during ad watch

**RICE Score:**
- Reach: 100%
- Impact: 7/10 (monetization access)
- Confidence: 9/10 (standard pattern)
- Effort: 3/13 (ad SDK integration)
- **Total: 2100**

**Technical Complexity:** Low
**Monetization Impact:** High

---

## Social & Virality Ideas

### 7. Life Comparison Feed

**User Story:**
As a player, I want to see how other players handled similar life situations, so that I can learn strategies and feel connected to community.

**Acceptance Criteria:**
- Feed shows anonymized life stories
- Players can "upvote" interesting decisions
- Filter by age/career/milestone
- Share button for my own story to social media

**RICE Score:**
- Reach: 60% (engaged subset)
- Impact: 7/10 (community engagement)
- Confidence: 7/10 (moderate success in other games)
- Effort: 10/13 (backend + UI)
- **Total: 294**

**Technical Complexity:** Medium

---

### 8. Weekly Leaderboards

**User Story:**
As a player, I want to compete on leaderboards (wealth, longest life, most achievements), so that I can measure my progress and compete with friends.

**Acceptance Criteria:**
- Multiple leaderboard categories
- Weekly reset with rewards for top 100
- Friend leaderboard subset
- Leaderboard history (personal best tracking)

**RICE Score:**
- Reach: 80%
- Impact: 6/10 (social motivation)
- Confidence: 8/10 (proven mechanic)
- Effort: 7/13 (backend service)
- **Total: 384**

**Technical Complexity:** Medium

---

### 9. Referral Bonus System

**User Story:**
As a player, I want to invite friends and earn rewards, so that I can share my experience and benefit from growth.

**Acceptance Criteria:**
- Unique referral code per player
- Referral earns bonus currency for both parties
- Referral milestone rewards (5 friends, 10 friends)
- Cap on rewards to prevent abuse

**RICE Score:**
- Reach: 70% (network size dependent)
- Impact: 5/10 (moderate virality)
- Confidence: 8/10 (proven growth mechanic)
- Effort: 6/13 (backend + tracking)
- **Total: 280**

**Technical Complexity:** Low-Medium

---

## Core Gameplay Innovations

### 10. Personality Test at Start

**User Story:**
As a new player, I want to take a personality test that influences starting stats and life path options, so that my character feels unique and personalized from the start.

**Acceptance Criteria:**
- 10-question personality quiz
- Results map to starting stat bonuses
- Affects available event options
- Personality displayed on profile

**RICE Score:**
- Reach: 100%
- Impact: 8/10 (personalization + replayability)
- Confidence: 9/10 (proven onboarding pattern)
- Effort: 8/13 (content + stat integration)
- **Total: 720**

**Technical Complexity:** Medium

---

### 11. Branching Event Outcomes Preview

**User Story:**
As a player, I want to see potential outcomes before deciding (without revealing exact results), so that I can make informed choices and feel in control.

**Acceptance Criteria:**
- "Preview" button on decisions
- Shows 2-3 outcome categories (e.g., "Career boost", "Health risk")
- Preview is vague (not spoil actual values)
- Preview hints at stat requirements for good outcome

**RICE Score:**
- Reach: 100%
- Impact: 7/10 (reduces decision anxiety)
- Confidence: 7/10 (used in some sims)
- Effort: 8/13 (event data structure + UI)
- **Total: 437**

**Technical Complexity:** Medium

---

### 12. Mini-Games for High-Stakes Decisions

**User Story:**
As a player, I want to play mini-games for critical decisions (job interviews, athletic competitions, creative challenges), so that I actively participate in my character's life.

**Acceptance Criteria:**
- 3 mini-game types (timing, memory, logic)
- Triggers on specific event types
- Performance affects outcome
- Simple, mobile-optimized gameplay

**RICE Score:**
- Reach: 60% (subset of events)
- Impact: 8/10 (high engagement moments)
- Confidence: 7/10 (success in The Sims)
- Effort: 13/13 (game development)
- **Total: 258**

**Technical Complexity:** High

---

## Prioritized Backlog

| Priority | Feature | Category | RICE Score | Complexity | Effort |
|----------|---------|-------------|-------------|----------|
| P0 | Daily Challenge System | Engagement | 492 | Medium | 13 |
| P0 | Legacy System | Gameplay | 450 | High | 12 |
| P1 | Rewarded Ads | Monetization | 2100 | Low | 3 |
| P1 | Life Milestones | Engagement | 630 | Medium | 10 |
| P1 | Cosmetic Avatars | Monetization | 432 | Medium | 6 |
| P2 | Time-Skip VIP | Monetization | 252 | Low | 5 |
| P2 | Leaderboards | Social | 384 | Medium | 7 |
| P2 | Personality Test | Gameplay | 720 | Medium | 8 |
| P2 | Comparison Feed | Social | 294 | Medium | 10 |
| P3 | Referral System | Social | 280 | Low-Med | 6 |
| P3 | Branching Preview | Gameplay | 437 | Medium | 8 |
| P4 | Mini-Games | Gameplay | 258 | High | 13 |

**Total Planned Features:** 12
**Total Estimated Effort:** 95 story points (1 week = 13 points)

---

## PM Recommendations

### Quick Wins (Sprint 1)
**Implement immediately for KPI impact:**

1. **Rewarded Ads Implementation** (3 pts)
   - Immediate monetization access
   - No gameplay balance changes
   - Ad SDK integration is straightforward

2. **Time-Skip VIP Perk** (5 pts)
   - High-value VIP feature
   - Easy to implement
   - Revenue-generating

3. **Daily Challenge System** (13 pts)
   - D1/D7 retention driver
   - Reuses existing event system
   - Streak mechanics boost engagement

**Sprint 1 Total:** 21 points (~1.5 sprints)

### Strategic Bets (Sprint 2-3)
**High-impact, medium complexity:**

1. **Life Milestones** (10 pts)
   - Core life sim mechanic
   - Emotional engagement hooks
   - Social sharing (virality)

2. **Cosmetic Avatars & Shop** (6 pts)
   - IAP revenue stream
   - Personalization value
   - Art-intensive (start early)

3. **Personality Test Onboarding** (8 pts)
   - Improves D1 retention
   - Personalized experience
   - Reusable assets

**Sprint 2-3 Total:** 24 points (~2 sprints)

### Future Considerations (Sprint 4+)

1. **Legacy & Reincarnation System** (12 pts)
   - Major replayability driver
   - Complex stat inheritance
   - Requires save system overhaul

2. **Mini-Games for High-Stakes** (13 pts)
   - Engagement spike
   - Different skillset needed
   - Can be implemented iteratively

3. **Leaderboards & Comparison Feed** (17 pts)
   - Social engagement
   - Backend infrastructure
   - Community building

### Avoid/Defer
**Low impact or high complexity for now:**

- Branching Outcome Preview - Nice-to-have, adds complexity
- Referral System - Moderate virality, low impact
- Mini-Games - Save for later, requires game dev expertise

---

## KPI Impact Forecast

| Feature | D1 Retention | D7 Retention | ARPDAU Impact |
|----------|---------------|---------------|-----------------|
| Daily Challenges | +15% | +8% | +$0.15 |
| Rewarded Ads | +5% | +3% | +$0.25 |
| Life Milestones | +10% | +5% | $0.00 |
| Cosmetics | +3% | +2% | +$0.20 |
| VIP Perks | 0% | +5% | +$0.35 |
| **Combined Sprint 1** | **+33%** | **+23%** | **+$0.95** |

**Note:** KPI targets are D1 >40%, D7 >25%, ARPDAU >$0.80

**Sprint 1 is positioned to meet or exceed all KPI targets.**

---

## Next Steps

1. Review backlog with team
2. Refine acceptance criteria
3. Sprint 1 planning with SM agent
4. Begin implementation with DEV agent
5. QA with TEA agent

---

**Brainstorming Complete!** Ready to enter BMAD Planning phase.
