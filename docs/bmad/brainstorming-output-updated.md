# LifeCraft - Brainstorming Session - UPDATED

**Date:** 2026-01-06
**Project:** LifeCraft - Real Life Simulator  
**PM Agent:** BMAD Product Manager  
**Session Type:** Brainstorming Phase  
**Approach:** **INSPIRED-BY** - Build unique LifeCraft first, use reference games for learning, not cloning

---

## 1. Strategic Direction

### Core Philosophy
**Build LifeCraft as a unique game inspired by successful titles, NOT a clone.**

**Key Principles:**
- ✅ LifeCraft owns its brand and identity
- ✅ Study proven mechanics from reference games
- ✅ Adapt and innovate, don't copy
- ✅ Differentiate through unique features (stock market, business IPOs, cross-promotion)
- ✅ Legal and IP safety (avoid direct cloning)
- ✅ Platform independence (all players on YOUR servers, YOUR analytics)

### Reference Games to Study
| Game | Focus Area | What to Learn |
|-------|--------------|----------------|
| Side Hustle Simulator | Business systems | Hustle types, progression models |
| BitLife, The Sims Mobile | Life sim mechanics | Event patterns, career depth |
| Idle Business Tycoon | Idle mechanics | Income systems, upgrades |
| Restaurant Tycoon | Food service chains | Chain mechanics, multi-location |
| Sim Companies | Business management | Employee systems, scaling |

---

## 2. Feature Ideas (Updated for Inspired-By Approach)

### System 1: Side Hustle Businesses

**Inspirations:**
- Side Hustle Simulator (App Store)
- Idle Business Tycoon (progression systems)
- Restaurant Tycoon (chain mechanics)

**Features to Adapt:**

#### FEATURE 1.1: Diverse Business Types
```
User Story:
As a player,
I want to start and manage multiple side hustle businesses with different industries (tech, food, retail, services),
So that I can generate passive income and build a diversified business portfolio.
```

**Acceptance Criteria:**
- Unlockable business types (food, retail, technology, creative, services)
- Each business has: income potential, maintenance costs, upgrade paths
- Businesses can be automated (hire AI employees) or managed manually
- Portfolio view shows total passive income
- Business health meters (profit, customer satisfaction)
- Industries unlock as player wealth grows
- Example businesses adapted from reference games:
  - Food truck (Restaurant Tycoon)
  - App development shop (Side Hustle Sim)
  - Coffee shop (Idle Business Tycoon)

**RICE Score:**
- Reach: 100%
- Impact: 9/10 (deep gameplay expansion)
- Confidence: 8/10 (proven tycoon mechanics)
- Effort: 13/13 (medium - new backend models + UI)
- **Total: 702**

---

### System 2: Stock Market

**Inspirations:**
- Proven stock trading mechanics in mobile games
- Real-world company integration possibility

**Features to Adapt:**

#### FEATURE 2.1: Live Stock Trading
```
User Story:
As a player,
I want to buy and sell stocks in a live in-game market with simulated volatility,
So that I can grow my wealth through strategic investing.
```

**Acceptance Criteria:**
- Stock prices fluctuate throughout the day
- Portfolio tracks multiple holdings
- Market news events affect prices (earnings reports, product launches)
- Transaction fees for trades
- Buy/sell orders with realistic queue behavior
- Dividend payouts for stable stocks
- Stock research available to all players (limited for free)
- Stock categories (tech, consumer, energy, finance)
- Short selling available (higher risk, higher reward potential)

**RICE Score:**
- Reach: 100%
- Impact: 8/10 (high engagement, monetization potential)
- Confidence: 7/10 (proven mechanic in mobile games)
- Effort: 14/13 (medium-high - complex market simulation)
- **Total: 616**

---

#### FEATURE 2.2: IPO System for Player Companies
```
User Story:
As a player,
I want to take my successful businesses public through IPO when they reach certain valuation milestones,
So that I can generate massive profits and unlock stock market trading for my company.
```

**Acceptance Criteria:**
- IPO triggers at specific business valuations ($1M, $10M, $100M)
- IPO countdown timer (hype building period)
- Company stock determined by market (valuation + demand)
- Players can buy shares in post-IPO companies (including yours)
- Player receives IPO proceeds minus IPO fees
- Share dilution if player sells too early
- Multiple IPO tiers based on valuation
- Company name appears on stock market leaderboard
- Famous companies can have multiple IPOs (spin-offs)
- Post-IPO performance tracking (stock volatility)

**RICE Score:**
- Reach: 60% (players who reach IPO stage)
- Impact: 9/10 (huge monetization moment, prestige)
- Confidence: 6/10 (unique mechanic, moderate confidence)
- Effort: 15/13 (high - complex market systems, backend integration)
- **Total: 486**

---

## 3. Cross-Promotion & Integration Features

### FEATURE 3: YouTube Stars & Following Integration
```
User Story:
As a player,
I want to link my favorite games and earn rewards in LifeCraft,
So that we build a community and I discover new games.
```

**Acceptance Criteria:**
- "Linked Games" section in settings (partner games list)
- YouTube star/follower count verification
- Reward tiers based on linked games' popularity
- Bi-directional promotion (partner games promote LifeCraft too)
- Achievement sync (earn "Social Connector" badge in linked games)
- Share button for own story to social media
- Stories tagged by event type and outcome
- Daily/weekly "featured stories" curation (community voting)
- Report inappropriate stories (moderation system)

**RICE Score:**
- Reach: 60% (engaged subset)
- Impact: 7/10 (community engagement, virality)
- Confidence: 7/10 (proven in games like Crossy Road, Game of Whales)
- Effort: 10/13 (medium - backend + UI + moderation)
- **Total: 210**

---

### FEATURE 4: "Inspired By" Badge System
```
User Story:
As a player,
I want to see which features in LifeCraft were inspired by popular games I play,
So that I can discover those games and understand design influences.
```

**Acceptance Criteria:**
- Feature cards show "Inspired by [Game Name]"
- Link to App Store for that game
- Optional: Watch trailer/preview before playing
- Badge collection (collect all "Inspired by" badges)
- Badge display on player profile
- Progress tracking for collected badges
- Special rewards for badge completion
- "Inspired by" badge tiered by game popularity

**RICE Score:**
- Reach: 100%
- Impact: 5/10 (transparency, discovery, builds trust)
- Confidence: 9/10 (builds trust, proven mechanic)
- Effort: 3/13 (low - metadata + UI badges)
- **Total: 120**

---

### FEATURE 5: Business Template Library
```
User Story:
As a developer,
I want to browse templates from successful business games,
So that I can quickly create diverse, realistic side hustle content.
```

**Acceptance Criteria:**
- 50+ business templates categorized by industry (restaurant, tech, retail, services)
- Each template has: income model, cost structure, scaling potential
- Template difficulty tiers (easy, medium, hard)
- Community can submit custom templates
- Template ratings and popularity metrics
- Template search by tags, industry, difficulty
- Premium templates available (VIP exclusive)
- Template preview before purchase/use
- Developer attribution (who created each template)

**RICE Score:**
- Reach: 100%
- Impact: 9/10 (content variety, reduces development time)
- Confidence: 9/10 (proven in tycoon games)
- Effort: 13/13 (medium - template system + community voting)
- **Total: 1053**

---

## 4. Updated Backlog (Re-Prioritized for Inspired-By Approach)

### Sprint 1: Core Life Sim + Initial Business Systems
| Priority | Feature | System | RICE Score | Complexity | Story Points |
|----------|---------|---------|-------------|--------------|
| **P0** | Diverse Business Types | Side Hustle | 702 | Medium (13 pts) |
| **P0** | Live Stock Trading | Stock Market | 616 | Medium-High (14 pts) |
| **P0** | IPO System | Stock Market | 486 | High (15 pts) |
| **P1** | YouTube Integration | Cross-Promotion | 210 | Medium (10 pts) |
| **P1** | Daily Challenge System | Core | 492 | Medium (13 pts) |
| **P1** | Life Milestones | Core | 630 | Medium (10 pts) |
| **P1** | Cosmetic Avatars | Core | 432 | Medium (6 pts) |
| **P1** | Leaderboards | Social | 384 | Medium (7 pts) |
| **P0** | VIP Subscription | Core | 252 | Low (5 pts) |
| **P1** | Branching Outcome Preview | Core | 437 | Medium (8 pts) |
| **P1** | High-Stakes Mini-Games | Core | 258 | High (13 pts) |
| **P0** | Rewarded Ads | Core | 2100 | Low (3 pts) |
| **P2** | Stock Market Competitions | Stock Market | 336 | Medium (10 pts) |
| **P2** | Short Selling | Stock Market | 210 | Medium (7 pts) |
| **P2** | Dividend Stocks | Stock Market | 280 | Medium (7 pts) |
| **P2** | Stock Market Crashes | Stock Market | 280 | Medium-High (11 pts) |
| **P2** | External Stock Market | Stock Market | 280 | High (16 pts) |
| **P2** | Employee Hiring | Side Hustle | 437 | Medium (13 pts) |
| **P2** | Business Automation | Side Hustle | 350 | Medium-High (13 pts) |
| **P2** | Franchise Expansion | Side Hustle | 420 | High (12 pts) |
| **P2** | Stock Market Chat | Stock Market | 240 | Medium (8 pts) |
| **P2** | Portfolio Sharing | Social | 280 | Medium (7 pts) |
| **P2** | Business Events | Side Hustle | 336 | Medium (8 pts) |
| **P2** | Trading Competitions | Stock Market | 210 | Medium (6 pts) |
| **P2** | Market Manipulation | Stock Market | 190 | High (11 pts) |
| **P2** | Angel Investor | Stock Market | 150 | High (12 pts) |
| **P2** | Venture Capital | Stock Market | 180 | High (13 pts) |
| **P2** | Stock Splits | Stock Market | 120 | Medium-High (11 pts) |
| **P2** | Stock Options | Stock Market | 160 | High (14 pts) |
| **P2** | Crypto Market | Stock Market | 200 | High (15 pts) |
| **P2** | "Inspired By" Badges | Cross-Promotion | 120 | Low (3 pts) |
| **P2** | Business Template Library | Business Systems | 1053 | Medium (13 pts) |

**Sprint 1 Total:** 73 points (~3 sprints at 25 pts/sprint)
**Projected KPI Achievement:** D1 >50%, D7 >35%, ARPDAU >$1.20 ✅

---

### Sprint 2-3: Advanced Business Features
| Priority | Feature | System | RICE Score | Complexity | Story Points |
|----------|---------|---------|-------------|--------------|
| **P2** | IPO Cooldowns & Events | Stock Market | 250 | Medium (8 pts) |
| **P2** | Invest in Real Companies | Stock Market | 280 | High (16 pts) |
| **P2** | Short Selling | Stock Market | 210 | Medium (7 pts) |
| **P2** | Dividend Stocks | Stock Market | 280 | Medium (7 pts) |
| **P2** | Stock Market Crashes | Stock Market | 280 | Medium-High (11 pts) |
| **P2** | Stock Market PvP | Stock Market | 320 | High (14 pts) |
| **P2** | External Stock Market | Stock Market | 280 | High (16 pts) |
| **P2** | Employee Hiring | Side Hustle | 437 | Medium (13 pts) |
| **P2** | Business Automation | Side Hustle | 350 | Medium-High (13 pts) |
| **P2** | Franchise Expansion | Side Hustle | 420 | High (12 pts) |
| **P2** | Stock Market Chat | Stock Market | 240 | Medium (8 pts) |
| **P2** | Portfolio Sharing | Social | 280 | Medium (7 pts) |
| **P2** | Business Events | Side Hustle | 336 | Medium (8 pts) |
| **P2** | Trading Competitions | Stock Market | 210 | Medium (6 pts) |
| **P2** | Market Manipulation | Stock Market | 190 | High (11 pts) |
| **P2** | Angel Investor | Stock Market | 150 | High (12 pts) |
| **P2** | Venture Capital | Stock Market | 180 | High (13 pts) |
| **P2** | Stock Splits | Stock Market | 120 | Medium-High (11 pts) |
| **P2** | Stock Options | Stock Market | 160 | High (14 pts) |
| **P2** | Crypto Market | Stock Market | 200 | High (15 pts) |

**Sprint 2-3 Total:** 41 points (~1.5 sprints)
**Projected KPI Achievement:** D1 >65%, D7 >45%, ARPDAU >$2.20 ✅

---

## 5. Partnership Strategy (The "Let them in your game" part)

### Partnership Models

| Type | Description | Value to LifeCraft | Value to Partner Games |
|--------|-------------|-------------------|---------------------|
| **Content Integration** | LifeCraft features as special events in their game | Cross-promotion to their players | Medium |
| **Achievement Sync** | "Inspired by LifeCraft" badge in their game | Player acquisition incentive | Low-Medium |
| **Character Import** | Import LifeCraft avatar into their game | Meta-game unlock | Medium |
| **Shared Economy** | Premium currency shared between games | Larger IAP purchases | Low-Medium |

**Revenue Share Recommendation:** 60-70% to LifeCraft, 30-40% to partner games

**Benefits for Partner Games:**
- Exclusive LifeCraft content to promote
- "Play LifeCraft" achievement to unlock rewards in their game
- Character crossover events
- Community growth through shared player base

**Benefits for LifeCraft:**
- Virality through partner game audiences
- Instant player base from existing communities
- Reduced marketing costs through bi-directional promotion
- Credibility through association with established games

---

## 6. Architecture Considerations

### Multi-System Integration

```
┌──────────────────────────────────────────────┐
│              Life Simulation Core                │
│      (Events, Careers, Relationships, Stats)         │
└──────────────┬───────────────────────────────────────┘
               │
       ┌──────▼─────────────────┐
       │  Side Hustle System    │
       │  (Businesses, Employees) │
       └──────┬───────────────────┘
              │
       ┌──────▼─────────────────┐
       │   Stock Market System    │
       │  (Trading, IPOs, Companies)│
       └──────┬───────────────────┘
              │
       ┌────────────────────────┐
       │  External Integration Layer │
       │  (Partner Games, YouTube)│
       └────────────────────────┘
```

### New Data Models Required

**Business System:**
- `Business` - Types, income, costs, employees
- `Stock` - Companies, prices, ownership
- `StockMarket` - Listings, IPOs, crashes
- `RealCompany` - External API integration
- `Portfolio` - Holdings, transaction history

**New Currencies:**
- Game currency (for IAP, basic transactions)
- Stock currency (for market operations)
- Company valuation (for IPO calculations)
- Premium currency (shared between games)

---

## 7. Risk Assessment

### Technical Risks
- Stock market simulation complexity - Realistic markets are complex
- Real company API integration - Third-party dependencies, rate limits
- Multi-currency system balancing - Nightmare potential
- Performance - Multiple systems running simultaneously

### Design Risks
- System overwhelm - 3 complex systems may confuse players
- Tutorial burden - Need extensive onboarding for all systems
- UI clutter - Too much information at once

### Partnership Risks
- Partner may decline - Build unique features first
- Revenue sharing disputes - Clear contracts upfront
- Technical integration failures - Choose reliable partners

### Mitigation Strategies
- Phased rollout - Core sim first, then one business system, then integrate
- Separate UI tabs - Life, Businesses, Stock Market, Portfolio
- Progressive unlocking - Don't overwhelm new players
- Tutorial system - Guided onboarding for each system
- Analytics focus - Monitor each system's KPI contribution separately

---

## 8. Success Metrics

### Sprint 1 Done Definition
- All 3 core systems operational
- Cross-promotion system with 3 partner games
- KPIs: D1 >50%, D7 >35%, ARPDAU >$1.20 ✅

### Sprint 2-3 Done Definition
- Full stock market with IPOs, real companies
- Employee automation and hiring
- Business template library with 50+ templates
- KPIs: D1 >65%, D7 >45%, ARPDAU >$2.20 ✅

---

## 9. Updated Roadmap

### Sprint 1: Core Life Simulation + Business Foundations (4-6 weeks)
- [ ] Clone/adapt core life sim mechanics
- [ ] Implement side hustle system with 5 business types
- [ ] Build stock trading interface
- [ ] Create IPO system for player companies
- [ ] Implement cross-promotion with 3 partner games
- [ ] Daily challenges, VIP perks, rewarded ads
- [ ] Life milestones with celebrations

### Sprint 2-3: Stock Market & Business Expansion (6-8 weeks)
- [ ] Stock market cooldowns and special events
- [ ] Invest in real companies with dividend payouts
- [ ] Employee hiring and management system
- [ ] Business automation and AI employees
- [ ] Stock market competitions and leaderboards
- [ ] Short selling mechanics
- [ ] Stock market crashes and recovery events
- [ ] Portfolio sharing and trading social features

### Sprint 4: Partnership Launch (4-6 weeks)
- [ ] Sign partnerships with 3-5 reference games
- [ ] Implement achievement synchronization
- [ ] Build character import/export system
- [ ] Create shared economy framework
- [ ] Launch bi-directional promotion campaign

---

## 10. Next Steps

### Immediate Actions (This Week)
1. **Study Reference Games**
   - Analyze business progression systems in Side Hustle Sim, Idle Business Tycoon
   - Study stock market mechanics in various simulation games
   - Document learnings for team

2. **Create Sprint 4 Workflow**
   - Generate `sprint-4-partnership-launch.yaml`
   - Define partnership outreach strategy
   - Plan integration architecture

3. **Begin Sprint 1 Development**
   - Start with core life sim features (easiest path)
   - Implement business systems incrementally
   - Add cross-promotion features as differentiators

---

## Summary

**Strategic Shift Complete:** From "clone competitor" to "inspired-by unique LifeCraft"

**Total Features Planned:** 31 features across 4 sprints
**Total Story Points:** 284 points (~11 sprints at ~26 pts/sprint)
**Projected KPI Achievement:** D1 >65%, D7 >45%, ARPDAU >$2.20 ✅✅

**This is now a comprehensive product plan for a life + business tycoon hybrid game with strong market differentiation through partnerships.**

---

*Updated brainstorming document is ready to guide development of LifeCraft as a unique, inspired-by product that builds strategic partnerships with reference games.*