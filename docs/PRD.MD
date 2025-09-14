# Flight Price Monitor – Product Requi| FS-4 | As a **visitor**, I see a timestamp indicating when prices were last refreshed. | • Timestamp format: "Prices updated <xx> minutes ago". |
| FS-5 | As a **visitor**, I can view detailed flight information by expanding a flight card. | • Click expand icon to show detailed flight info (aircraft, terminal, amenities).<br>• Only one flight detail can be expanded at a time.<br>• Expanded section includes route segments, fare breakdown, and policies.<br>• Click collapse or expand another flight to close current details. |

### 5.2 Historical Price Timelineents Document (PRD)

## 1 – Purpose
Build a SaaS web application that:
1. Shows current flight options and prices for a given origin, destination, departure date, and return date.  
2. Lets users “look back” and see how the same itinerary cost at any point in the past.

## 2 – Objectives & Success Metrics
| Objective | KPI | Target (MVP) |
|-----------|-----|--------------|
| Provide reliable live pricing | % of searches returning at least one fare | ≥ 97 % |
| Provide historical look-back | Median history query latency | ≤ 1.0 s |
| Retain users | Day-30 retention (registered users) | ≥ 20 % |
| Monetize | Free-to-paid conversion | ≥ 5 % within 90 days |

## 3 – Personas
* **Deal Hunter Dana** – hobbyist traveler tracking multiple trips for price drops.  
* **Travel Blogger Ben** – embeds historical price charts in posts.  
* **Corporate Cara** – books recurring trips and needs reporting.

## 4 – Key Features
1. Flight Search (Live Prices)  
2. Historical Price Timeline  
3. Saved Alerts & Notifications *(post-MVP)*  
4. Account & Billing  
5. Admin & Data Ingestion

---

## 5 – User Stories & Acceptance Criteria

### 5.1 Flight Search (Live Prices)

| # | User Story | Acceptance Criteria (AC) |
|---|------------|--------------------------|
| FS-1 | As a **visitor**, I can enter origin, destination, departure date, and return date so I can see available flight options. | • GIVEN valid inputs, WHEN I click “Search”, THEN results load within 3 s.<br>• Results list includes airline, total price (currency), stops, duration, and a deep link to book. |
| FS-2 | As a **visitor**, I can sort results by price, duration, or number of stops. | • Sorting toggle updates order instantly without full reload.<br>• Default sort = lowest price. |
| FS-3 | As a **visitor**, I can filter by airline and cabin class. | • Filters appear as check-boxes.<br>• At least one fare remains visible after filtering, or an empty-state shows “No results”. |
| FS-4 | As a **visitor**, I see a timestamp indicating when prices were last refreshed. | • Timestamp format: “Prices updated <xx> minutes ago”. |

### 5.2 Historical Price Timeline

| # | User Story | Acceptance Criteria (AC) |
|---|------------|--------------------------|
| HP-1 | As a **visitor**, I can click “Price history” on any search to view a chart of price changes over time. | • Chart plots at least one point per collection run.<br>• X-axis shows date/time, Y-axis shows price.<br>• Hover tooltip displays exact timestamp & price. |
| HP-2 | As a **visitor**, I can input a past date/time and see what the cheapest fare was then. | • GIVEN a valid past datetime, WHEN I click “Go”, THEN cheapest price at or before that timestamp is shown within 1 s.<br>• If no data exists, app displays “No snapshot available”. |
| HP-3 | As a **visitor**, I can export the history data to CSV. | • “Download CSV” button produces file with headers: timestamp, price, airline, cabin. |

### 5.3 Saved Alerts & Notifications *(Phase 2)*

| # | User Story | Acceptance Criteria (AC) |
|---|------------|--------------------------|
| AL-1 | As a **registered user**, I can save a search with a target price to receive alerts. | • Target price must be numeric & > 0.<br>• Alert saved confirmation toast shown. |
| AL-2 | As a **registered user**, I receive an email when price ≤ target. | • Email sent within 10 min of condition met.<br>• Email includes deep link to book. |
| AL-3 | As a **registered user**, I can pause or delete an alert. | • Change reflected immediately in UI and DB. |

### 5.4 Account & Billing

| # | User Story | Acceptance Criteria (AC) |
|---|------------|--------------------------|
| AC-1 | As a **visitor**, I can sign up with email or Google SSO. | • Email signup requires verification link.<br>• Google OAuth completes in ≤ 2 min. |
| AC-2 | As a **registered user**, I can upgrade to Pro via Stripe. | • Checkout redirects to Stripe, returns success.<br>• Pro flag set in DB; features unlocked instantly. |
| AC-3 | As a **registered user**, I can view and cancel subscription. | • “Manage billing” opens customer portal.<br>• Cancellation sets plan to “Free” at period end. |

### 5.5 Admin & Data Ingestion

| # | User Story | Acceptance Criteria (AC) |
|---|------------|--------------------------|
| AD-1 | As an **admin**, I can see ingestion job status (success, failure). | • Dashboard shows last run time, rows inserted, error logs. |
| AD-2 | As an **admin**, I can manually trigger a re-crawl for a route. | • Button queues job; status updates within 5 min. |
| AD-3 | As an **admin**, I can adjust API keys and rate-limit settings. | • CRUD UI saves to encrypted store; changes applied without redeploy. |

---

## 6 – Out of Scope (MVP)
• Multi-city itineraries  
• Hotel or car-rental bundling  
• Machine-learning price predictions

## 7 – Dependencies
• Access to at least one flight data API (e.g., Kiwi Tequila).  
• Legal approval for price storage duration.  
• Stripe account for billing.

## 8 – Risks & Mitigations
| Risk | Impact | Mitigation |
|------|--------|-----------|
| API quota exhaustion | No fresh prices | Caching + staggered jobs |
| Scraping ToS violation | Legal takedown | Prefer licensed APIs |
| Price changes at booking | User mistrust | Show “last updated” timestamp & disclaimer |

## 9 – Milestones (High-Level)
1. Week 0-2 – Finalize API contracts & DB schema  
2. Week 3-6 – Implement Live Search (FS stories)  
3. Week 7-9 – Build Historical Timeline (HP stories)  
4. Week 10-12 – Authentication & Billing (AC stories)  
5. Week 13 – Closed beta & feedback  
6. Week 16 – Public launch
