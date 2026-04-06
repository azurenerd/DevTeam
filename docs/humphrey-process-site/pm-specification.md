# Product Specification: Humphrey Luxury Pools — Process & Education Site

**Document Version:** 1.0  
**Date:** April 6, 2026  
**Author:** Program Management  
**Status:** Draft  
**Project Codename:** Humphrey Process Site

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Business Goals & Objectives](#2-business-goals--objectives)
3. [Target Audience](#3-target-audience)
4. [User Stories & Acceptance Criteria](#4-user-stories--acceptance-criteria)
5. [Scope Definition](#5-scope-definition)
6. [Information Architecture](#6-information-architecture)
7. [Functional Requirements](#7-functional-requirements)
8. [Non-Functional Requirements](#8-non-functional-requirements)
9. [Content Requirements](#9-content-requirements)
10. [Design & Brand Requirements](#10-design--brand-requirements)
11. [Technology Stack](#11-technology-stack)
12. [Integration & Deployment](#12-integration--deployment)
13. [Constraints & Assumptions](#13-constraints--assumptions)
14. [Success Metrics & Definition of Done](#14-success-metrics--definition-of-done)
15. [Risks & Mitigations](#15-risks--mitigations)
16. [Milestones & Estimated Effort](#16-milestones--estimated-effort)
17. [Appendix A — Content Matrix](#appendix-a--content-matrix)
18. [Appendix B — Design Token Reference](#appendix-b--design-token-reference)

---

## 1. Executive Summary

Humphrey Luxury Pools needs a comprehensive, visually premium educational website that walks prospective customers through every phase of building a luxury outdoor living space — pools, spas, sunken firepits, firebowls, acrylic pool windows, outdoor kitchens, patio covers, and turf. The site targets homeowners planning **$250K+ projects** and must communicate both the *process* and the *quality differentiators* that separate luxury builders from standard contractors.

The site will use a **3-tier progressive disclosure** pattern: a high-level overview of all categories, a category-level timeline of phases, and individual deep-dive pages for each phase. Each deep-dive includes a concise quality summary, a detailed comparison table (standard vs. luxury builder), a narrative educational section, common pitfalls, and luxury differentiators.

The site shares the design language of the existing [HumphreyPools](https://github.com/azurenerd/HumphreyPools) site (dark luxury palette, Playfair Display/Raleway typography, gold accents) and is architected for eventual integration as a subdirectory (`/process/`) of the main site.

---

## 2. Business Goals & Objectives

| # | Goal | Measurable Objective |
|---|------|---------------------|
| **G1** | **Educate prospective buyers** on what a luxury pool build entails | Visitors read ≥3 phase deep-dive pages per session within 6 months of launch |
| **G2** | **Differentiate Humphrey Luxury Pools** from standard builders | Every phase page includes a comparison table with ≥5 rows contrasting standard vs. luxury practices |
| **G3** | **Generate qualified leads** by demonstrating expertise | Each section includes a clear CTA to "Schedule a Consultation"; measurable via CTA click-through rate |
| **G4** | **Reduce sales cycle friction** by pre-educating customers | Sales team reports shorter discovery calls because customers arrive informed (qualitative, measured via post-launch survey at 90 days) |
| **G5** | **Establish SEO authority** for luxury pool construction education | Achieve indexed ranking for ≥20 long-tail luxury pool construction keywords within 6 months |
| **G6** | **Create a reusable content platform** that scales | Adding a new service category requires only Markdown files — no template changes |

---

## 3. Target Audience

### Primary Persona: "The Discerning Homeowner"

- **Demographics:** Homeowner in North Dallas (Celina, Prosper, Frisco, McKinney), household income $500K+, planning a $250K–$1M+ outdoor living project
- **Behavior:** Researches extensively before engaging a builder; reads reviews, watches YouTube, compares multiple bids; skeptical of builder claims
- **Pain Points:** Cannot distinguish quality builders from budget-cutters; doesn't know what questions to ask; afraid of hidden shortcuts
- **Goals:** Understand *exactly* what each phase involves, what to inspect, and what differentiates a 20-year pool from a 50-year pool
- **Device:** 60% mobile (initial research), 40% desktop (deep reading, sharing with spouse/partner)

### Secondary Persona: "The Referring Professional"

- **Demographics:** Real estate agents, architects, interior designers, landscape architects who recommend pool builders to their clients
- **Behavior:** Shares links to specific sections to help clients understand the process
- **Goals:** Needs shareable, credible, bookmark-worthy content that reflects well on their recommendation

---

## 4. User Stories & Acceptance Criteria

### Epic 1: Content Discovery & Navigation

#### US-1.1: Browse All Service Categories
**As a** prospective homeowner,  
**I want to** see all luxury outdoor living categories at a glance,  
**So that** I can understand the full scope of what Humphrey builds and find the category relevant to my project.

**Acceptance Criteria:**
- [ ] Landing page (`/process/`) displays all 8 service categories as visually rich cards
- [ ] Each card shows: hero image, category name, 1-line tagline, and phase count
- [ ] Cards use scroll-reveal animations consistent with the existing site's `.reveal` pattern
- [ ] Page loads with LCP < 2.5 seconds on 4G mobile
- [ ] Responsive layout: 1 column on mobile, 2 columns on tablet, 3–4 columns on desktop

#### US-1.2: View Category Phase Timeline
**As a** homeowner planning a pool,  
**I want to** see all build phases for my chosen category in chronological order,  
**So that** I understand the full timeline from design to completion.

**Acceptance Criteria:**
- [ ] Category page (e.g., `/process/pool/`) displays a vertical timeline of all phases, numbered sequentially
- [ ] Each phase card shows: phase number, title, hero thumbnail, 2–3 sentence summary
- [ ] A "Quick Quality Summary" callout at the top provides a 3–5 sentence overview of luxury vs. standard differences for this category
- [ ] A "Category Comparison Overview" table summarizes key differences across all phases
- [ ] Each phase card links to its deep-dive page
- [ ] Breadcrumb navigation displays: `Process → Pool`

#### US-1.3: Read Phase Deep-Dive
**As a** homeowner,  
**I want to** read the complete details of a specific build phase,  
**So that** I understand materials, techniques, pitfalls, and what to demand from my builder.

**Acceptance Criteria:**
- [ ] Deep-dive page (e.g., `/process/pool/gunite/`) includes all 5 required content sections (see [Section 9](#9-content-requirements))
- [ ] A concise quality summary (3–5 bullets) appears immediately below the hero image
- [ ] A comparison table with ≥5 rows is rendered with clear visual distinction between "Standard Builder" and "Luxury Builder" columns
- [ ] Pitfalls section uses a visually distinct warning/caution treatment
- [ ] Luxury Differentiators section uses the gold accent treatment
- [ ] Narrative body content supports headings, images, and expandable `<details>` sections
- [ ] Breadcrumb navigation displays: `Process → Pool → Gunite & Shotcrete`
- [ ] Previous/Next phase navigation links appear at page bottom

#### US-1.4: Navigate Between Phases
**As a** homeowner reading a deep-dive,  
**I want to** easily move to the next or previous phase,  
**So that** I can read through the entire build process sequentially.

**Acceptance Criteria:**
- [ ] Previous Phase and Next Phase links appear at the bottom of every deep-dive page
- [ ] Links include the phase title and number (e.g., "← 04. Plumbing" / "06. Tile & Coping →")
- [ ] A "Back to All Phases" link returns to the category overview
- [ ] First phase shows only "Next"; last phase shows only "Previous"

#### US-1.5: Navigate via Breadcrumbs
**As a** user at any depth,  
**I want to** see where I am in the site hierarchy and navigate upward,  
**So that** I never feel lost.

**Acceptance Criteria:**
- [ ] Breadcrumbs appear on all Layer 2 and Layer 3 pages
- [ ] Breadcrumb format: `Process → [Category] → [Phase]` with each segment being a clickable link
- [ ] Styled using the `--color-accent` overline pattern from the existing site

---

### Epic 2: Quality Comparison & Education

#### US-2.1: Understand Standard vs. Luxury Differences at a Glance
**As a** homeowner comparing builders,  
**I want to** quickly see a side-by-side comparison of standard vs. luxury practices for each phase,  
**So that** I can ask the right questions during builder interviews.

**Acceptance Criteria:**
- [ ] Every deep-dive page renders a comparison table from structured frontmatter data
- [ ] Table columns: "Detail", "Standard Builder", "Luxury Builder"
- [ ] "Standard Builder" column uses muted/neutral styling
- [ ] "Luxury Builder" column uses gold accent styling to visually signal the preferred approach
- [ ] Table is horizontally scrollable on mobile without breaking layout
- [ ] Minimum 5 comparison rows per phase

#### US-2.2: Identify Pitfalls and Red Flags
**As a** homeowner,  
**I want to** know the common mistakes and shortcuts other builders take,  
**So that** I can recognize red flags during my own project.

**Acceptance Criteria:**
- [ ] Pitfalls section renders as a visually distinct list (caution/warning icon + amber/red accent)
- [ ] Each pitfall is a concise, actionable statement (1–2 sentences)
- [ ] Minimum 3 pitfalls per phase

#### US-2.3: Understand Luxury Differentiators
**As a** homeowner considering a premium builder,  
**I want to** see exactly what luxury builders do differently,  
**So that** I understand the value of the investment.

**Acceptance Criteria:**
- [ ] Luxury Differentiators section renders as a gold-accented feature list
- [ ] Each differentiator is a concise statement explaining the *what* and *why*
- [ ] Minimum 3 luxury differentiators per phase
- [ ] Section appears below pitfalls for narrative contrast (problem → solution)

---

### Epic 3: Lead Generation

#### US-3.1: Request a Consultation from Any Page
**As a** prospective customer who feels informed,  
**I want to** easily initiate contact with Humphrey Luxury Pools,  
**So that** I can start my project.

**Acceptance Criteria:**
- [ ] A "Schedule a Consultation" CTA button appears in the site header/navigation on every page
- [ ] A contextual CTA appears at the bottom of every deep-dive page (e.g., "Ready to build your dream pool? Let's talk.")
- [ ] CTA links to the contact section of the main Humphrey Pools site (`/#contact`)
- [ ] CTA uses the existing `.btn-primary` styling (gold background, dark text)

---

### Epic 4: Responsive & Accessible Experience

#### US-4.1: Read Content Comfortably on Mobile
**As a** homeowner browsing on my phone,  
**I want to** read all content including comparison tables without horizontal scrolling issues,  
**So that** I can research on the go.

**Acceptance Criteria:**
- [ ] All pages score ≥90 on Google Lighthouse mobile performance
- [ ] Comparison tables scroll horizontally within their container on screens < 768px, with a visual scroll hint
- [ ] Images are responsive with appropriate `srcset` breakpoints (400, 800, 1200, 1600px)
- [ ] Touch targets (buttons, links) are ≥ 44x44px
- [ ] Text is readable without zooming (minimum 16px body font)

#### US-4.2: Navigate with Keyboard and Screen Reader
**As a** user with accessibility needs,  
**I want to** navigate and read all content using keyboard and assistive technology,  
**So that** I have equal access to the educational content.

**Acceptance Criteria:**
- [ ] All interactive elements are keyboard-accessible with visible focus indicators
- [ ] Images have descriptive `alt` text
- [ ] Comparison tables use proper `<thead>`, `<th>` scope attributes
- [ ] Expandable sections (`<details>/<summary>`) are natively accessible
- [ ] Color contrast ratios meet WCAG 2.1 AA (4.5:1 for body text, 3:1 for large text)
- [ ] Page structure uses semantic HTML5 landmarks (`<main>`, `<nav>`, `<article>`, `<section>`)

---

### Epic 5: SEO & Discoverability

#### US-5.1: Rank for Luxury Pool Construction Queries
**As** Humphrey Luxury Pools (the business),  
**I want** each phase page to be independently indexable and optimized,  
**So that** we attract organic search traffic from homeowners researching luxury pool construction.

**Acceptance Criteria:**
- [ ] Each page has a unique `<title>` tag (format: `[Phase] — [Category] | Humphrey Luxury Pools Process`)
- [ ] Each page has a unique `<meta name="description">` (150–160 characters)
- [ ] Each page has Open Graph (`og:title`, `og:description`, `og:image`) and Twitter Card meta tags
- [ ] `HowTo` structured data (JSON-LD) is included on phase deep-dive pages where applicable
- [ ] A sitemap.xml is auto-generated at build time
- [ ] Canonical URLs are set on all pages
- [ ] Internal linking: each deep-dive links to related phases and back to category overview

---

## 5. Scope Definition

### In Scope (v1.0)

| Item | Description |
|------|-------------|
| **8 service categories** | Pool, Spa, Sunken Firepit, Firebowls, Acrylic Pool Window, Outdoor Kitchen, Patio Cover, Turf |
| **Phase deep-dives** | 8–12 phases per category (~64–96 total deep-dive pages) |
| **3-tier navigation** | Overview → Category Timeline → Phase Deep-Dive |
| **Comparison tables** | Standard vs. Luxury builder comparison on every deep-dive page |
| **Quality summaries** | Concise luxury-vs-standard summary at top of each section |
| **Pitfalls & differentiators** | Dedicated sections on every deep-dive page |
| **Luxury placeholder imagery** | Unsplash/Pexels images (free commercial license) for all pages |
| **Responsive design** | Mobile-first, matching existing site breakpoints |
| **SEO foundation** | Meta tags, OG images, structured data, sitemap |
| **Shared design system** | CSS custom properties from existing HumphreyPools site |
| **Static site generation** | Astro 5.x with Content Collections |
| **Netlify deployment** | Automated build and deploy on git push |
| **Scroll-reveal animations** | IntersectionObserver-based, matching existing site |
| **Consultation CTAs** | Links to existing site contact section |
| **Phase navigation** | Previous/Next and breadcrumb navigation |

### Out of Scope (v1.0)

| Item | Rationale |
|------|-----------|
| **Contact form on the process site** | CTA links to existing main site contact section; avoids duplicate form handling |
| **User accounts / login** | No personalization needed for educational content |
| **CMS / admin panel** | Content managed via Markdown in Git; CMS is a future enhancement |
| **E-commerce / pricing calculator** | Luxury projects require custom consultation; online pricing undermines the premium positioning |
| **Blog / news section** | Separate content strategy; can be added later |
| **Customer portal / project tracking** | Different product entirely |
| **Original photography** | v1 uses placeholder images; original luxury build photography is a post-launch investment |
| **Video production** | v1 reuses existing site videos; new phase-specific videos are post-launch |
| **Multi-language support** | English-only for North Dallas market |
| **Analytics integration** | Recommended for v1.1; not a launch blocker |
| **A/B testing** | Post-launch optimization |
| **Monorepo unification** | The existing vanilla HTML site and this Astro site remain separate repos for v1; monorepo is a future architecture decision |
| **Custom domain / subdirectory routing** | v1 deploys to Netlify with its own URL; subdirectory integration (`/process/`) is a v1.1 deployment task |

---

## 6. Information Architecture

### Site Map

```
/process/                                    Layer 1 — Overview
├── /process/pool/                           Layer 2 — Pool Category
│   ├── /process/pool/design/                Layer 3 — Deep-Dive
│   ├── /process/pool/excavation/
│   ├── /process/pool/steel-rebar/
│   ├── /process/pool/plumbing/
│   ├── /process/pool/electrical/
│   ├── /process/pool/gunite/
│   ├── /process/pool/tile-coping/
│   ├── /process/pool/decking/
│   ├── /process/pool/equipment/
│   ├── /process/pool/landscaping/
│   ├── /process/pool/plaster-finish/
│   └── /process/pool/startup/
├── /process/spa/                            Layer 2 — Spa Category
│   ├── /process/spa/design/
│   ├── ...
├── /process/sunken-firepit/                 Layer 2 — Sunken Firepit
│   ├── ...
├── /process/firebowls/                      Layer 2 — Firebowls
│   ├── ...
├── /process/acrylic-window/                 Layer 2 — Acrylic Pool Window
│   ├── ...
├── /process/outdoor-kitchen/                Layer 2 — Outdoor Kitchen
│   ├── ...
├── /process/patio-cover/                    Layer 2 — Patio Cover
│   ├── ...
└── /process/turf/                           Layer 2 — Turf
    ├── ...
```

### Page Templates

| Template | Used On | Key Sections |
|----------|---------|-------------|
| **Overview Template** | `/process/` | Hero, category card grid, intro narrative |
| **Category Template** | `/process/[category]/` | Hero, quality summary callout, phase timeline, category comparison table |
| **Deep-Dive Template** | `/process/[category]/[phase]/` | Hero image, quality summary bullets, comparison table, narrative body, pitfalls, luxury differentiators, phase navigation |

---

## 7. Functional Requirements

### FR-1: Content Rendering

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-1.1 | Render Markdown body content with full support for headings (h2–h4), paragraphs, lists, bold, italic, images, and links | **Must Have** |
| FR-1.2 | Render YAML frontmatter comparison arrays as styled HTML tables | **Must Have** |
| FR-1.3 | Render YAML frontmatter pitfall arrays as styled warning lists | **Must Have** |
| FR-1.4 | Render YAML frontmatter luxury differentiator arrays as styled feature lists | **Must Have** |
| FR-1.5 | Render quality summary from frontmatter as a gold-accented callout box | **Must Have** |
| FR-1.6 | Support `<details>/<summary>` for expandable content within Markdown body | **Should Have** |
| FR-1.7 | Provide "Expand All / Collapse All" toggle for pages with multiple `<details>` sections | **Could Have** |

### FR-2: Navigation

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-2.1 | Render breadcrumb navigation on all Layer 2 and Layer 3 pages | **Must Have** |
| FR-2.2 | Render Previous/Next phase links at the bottom of every Layer 3 page | **Must Have** |
| FR-2.3 | Render a site-wide header with logo and navigation links | **Must Have** |
| FR-2.4 | Mobile hamburger menu with overlay, matching existing site pattern | **Must Have** |
| FR-2.5 | Smooth scroll to anchor targets within the same page | **Should Have** |
| FR-2.6 | Sticky phase sidebar navigation on desktop for Layer 2 pages (showing all phases, highlighting current) | **Could Have** |

### FR-3: Visual Effects

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-3.1 | Scroll-reveal animations on category cards and phase cards using IntersectionObserver | **Must Have** |
| FR-3.2 | Autoplay muted looping video background on Layer 1 hero (with image fallback) | **Should Have** |
| FR-3.3 | High-quality hero images on Layer 2 and Layer 3 pages (no video) | **Must Have** |
| FR-3.4 | Subtle hover effects on cards and CTAs matching existing site transitions | **Must Have** |

### FR-4: Image Optimization

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-4.1 | All images served in AVIF format with WebP and JPEG fallbacks via `<picture>` element | **Must Have** |
| FR-4.2 | Responsive `srcset` at 400px, 800px, 1200px, and 1600px widths | **Must Have** |
| FR-4.3 | Lazy loading (`loading="lazy"`) on all below-fold images | **Must Have** |
| FR-4.4 | Eager loading with `fetchpriority="high"` on hero/above-fold images | **Must Have** |
| FR-4.5 | Explicit `width` and `height` attributes on all images to prevent CLS | **Must Have** |

### FR-5: SEO

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-5.1 | Unique `<title>` and `<meta description>` per page, derived from frontmatter | **Must Have** |
| FR-5.2 | Open Graph and Twitter Card meta tags on all pages | **Must Have** |
| FR-5.3 | Auto-generated `sitemap.xml` at build time | **Must Have** |
| FR-5.4 | `HowTo` JSON-LD structured data on applicable phase deep-dive pages | **Should Have** |
| FR-5.5 | Canonical URL tags on all pages | **Must Have** |
| FR-5.6 | `robots.txt` allowing full crawling | **Must Have** |

---

## 8. Non-Functional Requirements

### NFR-1: Performance

| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| **Largest Contentful Paint (LCP)** | < 2.5 seconds on 4G mobile | Google Lighthouse, WebPageTest |
| **First Input Delay (FID)** | < 100ms | Google Lighthouse |
| **Cumulative Layout Shift (CLS)** | < 0.1 | Google Lighthouse |
| **Time to First Byte (TTFB)** | < 200ms | WebPageTest (Netlify CDN) |
| **Total page weight** | < 2MB per page (including all lazy-loaded images) | DevTools Network tab |
| **Above-fold image weight** | < 300KB (hero + first 2–3 visible images) | DevTools Network tab |
| **Lighthouse Performance score** | ≥ 90 on mobile, ≥ 95 on desktop | Google Lighthouse |
| **Client-side JavaScript** | 0 KB for content pages (Astro zero-JS output) | Build output inspection |
| **Build time** | < 60 seconds for full site build | Netlify build log |

### NFR-2: Security

| Requirement | Implementation |
|-------------|----------------|
| HTTPS everywhere | Netlify provides automatic TLS certificates |
| No user-generated content | Static site; no input surfaces to exploit |
| Content Security Policy | Strict CSP headers via Netlify `_headers` file |
| Dependency security | `npm audit` in CI pipeline; Dependabot enabled on GitHub repo |
| No third-party tracking scripts at launch | No analytics, ad trackers, or chat widgets in v1 |

### NFR-3: Scalability

| Requirement | Details |
|-------------|---------|
| Adding a new service category | Requires only: (1) new Markdown files in `src/content/`, (2) images in the appropriate folder. No template code changes. |
| Adding a new phase to existing category | Requires only: (1) one new Markdown file with proper frontmatter schema. Phase numbering and navigation auto-update. |
| Content volume | Astro Content Collections with Zod validation handles 500+ Markdown files without performance degradation |
| Traffic | Static site on CDN scales to millions of requests per month on Netlify free tier |

### NFR-4: Reliability & Availability

| Requirement | Details |
|-------------|---------|
| Uptime target | 99.9% (Netlify CDN SLA) |
| Failure mode | Static files; no server-side processing to fail. CDN serves cached assets. |
| Build failure handling | Netlify retains last successful deploy; failed builds do not affect production |
| Rollback | One-click rollback to any previous deploy in Netlify dashboard |

### NFR-5: Maintainability

| Requirement | Details |
|-------------|---------|
| Content authoring | Non-technical authors can write Markdown with YAML frontmatter; schema validates at build time |
| Build-time validation | Zod schemas catch missing fields, wrong types, and content inconsistencies before deployment |
| Code style | Astro components follow single-responsibility principle; 5 reusable components handle all content presentation |
| Documentation | README in repo root documents: content authoring workflow, schema reference, local development setup, deployment process |

### NFR-6: Browser Support

| Browser | Version |
|---------|---------|
| Chrome / Edge | Last 2 major versions |
| Safari | Last 2 major versions (including iOS Safari) |
| Firefox | Last 2 major versions |
| Samsung Internet | Last 2 major versions |

**Note:** AVIF image fallback to WebP/JPEG ensures compatibility with older browsers. `<details>/<summary>` is natively supported in all target browsers.

---

## 9. Content Requirements

### Content Structure Per Deep-Dive Page

Every Layer 3 (deep-dive) page **must** contain the following sections in this order:

| # | Section | Source | Format | Required |
|---|---------|--------|--------|----------|
| 1 | **Hero Image** | Frontmatter `heroImage` | Full-width responsive image | Yes |
| 2 | **Quick Quality Summary** | Frontmatter `luxurySummary` | Gold-bordered callout, 3–5 sentences | Yes |
| 3 | **Comparison Table** | Frontmatter `comparison[]` | 3-column table (Detail / Standard / Luxury), min. 5 rows | Yes |
| 4 | **Narrative Body** | Markdown body | Educational long-form content with headings, images, expandable sections | Yes |
| 5 | **Pitfalls & Red Flags** | Frontmatter `pitfalls[]` | Warning-styled list, min. 3 items | Yes |
| 6 | **Luxury Differentiators** | Frontmatter `luxuryDifferentiators[]` | Gold-accented feature list, min. 3 items | Yes |
| 7 | **Phase Navigation** | Auto-generated | Previous / All Phases / Next links | Yes |
| 8 | **Consultation CTA** | Template-level | "Ready to build?" callout with button | Yes |

### Content Tone & Style Guide

| Attribute | Guideline |
|-----------|-----------|
| **Voice** | Authoritative, educational, confident — not salesy. Think "trusted advisor" not "used car salesman." |
| **Reading level** | College-educated professional (Flesch-Kincaid grade 10–12) |
| **Person** | Second person ("your pool", "you should expect") for engagement; third person for industry references |
| **Luxury signals** | Use words like: bespoke, engineered, precision, curated, artisan, hand-selected, structural integrity, longevity |
| **Avoid** | Superlatives without substance ("best ever"), negative competitor bashing, pricing specifics, jargon without explanation |
| **Comparison framing** | "Standard builder" vs. "Luxury builder" — factual, not disparaging. Focus on *what they do differently*, not *how bad standard builders are* |

### Service Categories & Phase Inventory

| Category | Slug | Estimated Phases |
|----------|------|-----------------|
| Pool | `pool` | 12 (Design, Permits, Excavation, Steel & Rebar, Plumbing, Electrical, Gunite/Shotcrete, Tile & Coping, Decking & Hardscape, Equipment, Plaster/Finish, Startup & Chemistry) |
| Spa | `spa` | 10 (Design, Excavation, Shell Construction, Hydrotherapy Jets, Heating Systems, Tile & Finish, Equipment, Integration with Pool, Lighting, Startup) |
| Sunken Firepit | `sunken-firepit` | 8 (Design, Excavation, Gas Line & Burner, Masonry & Stonework, Seating Integration, Drainage, Lighting, Finishing) |
| Firebowls | `firebowls` | 6 (Design & Placement, Gas Supply, Bowl Selection & Materials, Installation, Lighting Integration, Maintenance Considerations) |
| Acrylic Pool Window | `acrylic-window` | 8 (Structural Engineering, Panel Fabrication, Excavation & Forming, Steel & Support Structure, Panel Installation, Waterproofing & Sealing, Lighting, Maintenance) |
| Outdoor Kitchen | `outdoor-kitchen` | 10 (Design & Layout, Foundation & Framing, Gas & Plumbing, Electrical & Lighting, Countertops & Materials, Appliance Selection, Ventilation, Roofing/Cover Integration, Finish & Tile, Final Inspection) |
| Patio Cover | `patio-cover` | 8 (Design & Engineering, Permitting, Foundation & Posts, Framing & Structure, Roofing Material, Electrical & Fans, Finish & Paint, Integration) |
| Turf | `turf` | 6 (Site Assessment, Base Preparation, Drainage System, Turf Selection, Installation, Infill & Grooming) |

**Total estimated deep-dive pages: ~68 pages**

---

## 10. Design & Brand Requirements

### Visual Identity (Inherited from Existing Site)

The process site **must** use the identical visual language as the existing HumphreyPools site:

| Element | Specification |
|---------|---------------|
| **Primary color** | `#0a1628` (deep navy) |
| **Primary light** | `#142440` |
| **Accent color** | `#c9a84c` (warm gold) |
| **Accent light** | `#e0c97f` |
| **Background (off-white)** | `#f7f5f0` |
| **Background (cream)** | `#ede8dd` |
| **Text color** | `#2c2c2c` on light backgrounds; `#ffffff` on dark |
| **Heading font** | `'Playfair Display', Georgia, serif` |
| **Body font** | `'Raleway', 'Segoe UI', sans-serif` |
| **Accent font** | `'Cormorant Garamond', Georgia, serif` |
| **Container max-width** | `1280px` |
| **Border radius** | `0` (sharp, editorial aesthetic) |
| **Section spacing** | `120px` vertical (desktop), `80px` (mobile) |
| **Animations** | Subtle fade-up on scroll, 0.8s ease transitions |

### Component Design Specifications

#### Comparison Table
- Header row: dark navy background (`--color-primary`), white text
- "Standard Builder" column: light gray background (`#f0f0f0`), muted text
- "Luxury Builder" column: faint gold tint background (`rgba(201, 168, 76, 0.08)`), gold left border
- Row alternating: subtle zebra striping
- Mobile: horizontal scroll with shadow fade indicator

#### Quality Summary Callout
- Left border: 4px solid `--color-accent`
- Background: `rgba(201, 168, 76, 0.05)`
- Font: `--font-accent` (Cormorant Garamond), italic
- Padding: 24px 32px

#### Pitfalls Section
- Icon: Warning triangle (SVG) in `#c0392b` (muted red)
- Background: `rgba(192, 57, 43, 0.04)`
- List style: numbered for priority, each item as a card with subtle border

#### Luxury Differentiators Section
- Icon: Diamond/star (SVG) in `--color-accent`
- Background: `rgba(201, 168, 76, 0.04)`
- List style: check marks in gold accent, each item as a feature card

#### Phase Timeline (Category Page)
- Vertical line: 2px solid `--color-accent`
- Phase numbers: Circular badges using existing `.process-number` pattern (gold border, navy background, white number)
- Alternating left/right layout on desktop; single column on mobile

---

## 11. Technology Stack

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **SSG Framework** | Astro | 5.7+ | Static site generation, Content Collections, zero-JS output |
| **Content Schema** | Zod (via Astro) | Built-in | Type-safe frontmatter validation for all 68+ content files |
| **Content Format** | Markdown + YAML frontmatter | — | Structured data in frontmatter, narrative in Markdown body |
| **Image Pipeline** | `astro:assets` | Built-in | Automatic AVIF/WebP, responsive srcset, lazy loading |
| **Styling** | CSS (shared custom properties) | CSS3 | Reuses existing HumphreyPools design tokens |
| **Animations** | IntersectionObserver | Native JS | Scroll-reveal matching existing site |
| **Fonts** | Google Fonts | — | Playfair Display, Raleway, Cormorant Garamond |
| **Placeholder Images** | Unsplash / Pexels | — | Free commercial license; to be replaced with original photography post-launch |
| **Hosting** | Netlify | Free tier | Auto-build, CDN, deploy previews, TLS |
| **Version Control** | GitHub | — | Source of truth; Netlify auto-deploys on push to `main` |
| **Package Manager** | npm | 10+ | Dependency management |
| **Node.js** | Node.js | 20 LTS+ | Build environment |

### Project Structure

```
humphrey-process-site/
├── astro.config.mjs
├── package.json
├── tsconfig.json
├── README.md
├── netlify.toml
├── public/
│   ├── robots.txt
│   ├── _headers              (CSP, cache headers)
│   └── images/
│       ├── pool/             (category-specific images)
│       ├── spa/
│       ├── ...
│       └── shared/           (icons, logos, patterns)
├── src/
│   ├── content.config.ts     (Zod schemas for Content Collections)
│   ├── layouts/
│   │   └── BaseLayout.astro  (shared HTML shell, head, header, footer)
│   ├── components/
│   │   ├── ComparisonTable.astro
│   │   ├── QualitySummary.astro
│   │   ├── PitfallsList.astro
│   │   ├── LuxuryDifferentiators.astro
│   │   ├── PhaseNavigation.astro
│   │   ├── Breadcrumbs.astro
│   │   ├── CategoryCard.astro
│   │   ├── PhaseTimelineCard.astro
│   │   └── ConsultationCTA.astro
│   ├── styles/
│   │   ├── design-tokens.css (extracted from existing styles.css)
│   │   ├── components.css
│   │   └── utilities.css
│   ├── content/
│   │   ├── pool/
│   │   │   ├── 01-design.md
│   │   │   ├── 02-permits.md
│   │   │   ├── 03-excavation.md
│   │   │   └── ...
│   │   ├── spa/
│   │   ├── sunken-firepit/
│   │   ├── firebowls/
│   │   ├── acrylic-window/
│   │   ├── outdoor-kitchen/
│   │   ├── patio-cover/
│   │   └── turf/
│   └── pages/
│       ├── index.astro          (Layer 1 — overview)
│       ├── [category]/
│       │   ├── index.astro      (Layer 2 — category timeline)
│       │   └── [phase].astro    (Layer 3 — phase deep-dive)
│       └── 404.astro
```

---

## 12. Integration & Deployment

### v1 Deployment (Standalone)

- **Repository:** Separate GitHub repo (`humphrey-process-site`)
- **Build command:** `npm run build` → outputs to `dist/`
- **Hosting:** Netlify auto-deploys from `main` branch
- **URL:** `https://humphrey-process.netlify.app/` (Netlify default)
- **Cross-linking:** Process site header links back to main HumphreyPools site; main site's "Process" nav link updated to point to process site

### v1.1 Integration (Subdirectory)

- **Target URL:** `https://humphreyluxurypools.com/process/`
- **Astro config:** `base: '/process'` in `astro.config.mjs`
- **Routing:** Netlify `_redirects` or reverse proxy to serve process site under `/process/` path
- **Shared assets:** Both sites load the same Google Fonts and shared CSS tokens

### Future: Monorepo Unification

- Convert existing vanilla `index.html` to `index.astro`
- Merge both sites into a single Astro project
- Single build, single deployment, unified routing

### Deployment Configuration

```toml
# netlify.toml
[build]
  command = "npm run build"
  publish = "dist"

[build.environment]
  NODE_VERSION = "20"

[[headers]]
  for = "/*"
  [headers.values]
    X-Frame-Options = "DENY"
    X-Content-Type-Options = "nosniff"
    Referrer-Policy = "strict-origin-when-cross-origin"
```

---

## 13. Constraints & Assumptions

### Constraints

| # | Constraint | Impact |
|---|-----------|--------|
| **C1** | Must visually match existing HumphreyPools site's luxury aesthetic | Design system is inherited, not created from scratch; all new components must use existing design tokens |
| **C2** | No client-side JavaScript frameworks (React, Vue, etc.) for content pages | Maintains parity with existing vanilla site; Astro enforces this via zero-JS output |
| **C3** | Content authoring via Markdown in Git (no CMS in v1) | Authors need basic Git/Markdown knowledge, or changes go through a developer |
| **C4** | Free-tier hosting (Netlify) | 100GB bandwidth/month, 300 build minutes/month — sufficient for projected traffic but must be monitored |
| **C5** | Placeholder images only in v1 | Luxury credibility is somewhat reduced until original photography is obtained |
| **C6** | Content is the primary time investment | ~50–70 hours for content vs. ~10–20 hours for technology — content quality depends on luxury pool domain expertise |

### Assumptions

| # | Assumption | Risk if Wrong |
|---|-----------|---------------|
| **A1** | Target audience primarily uses modern browsers (Chrome, Safari, Edge from the last 2 years) | Older browser support would require polyfills and fallback testing |
| **A2** | Luxury pool domain expertise is available to write or review all 68+ phase deep-dives | Without expert review, content may contain inaccuracies that damage credibility |
| **A3** | Unsplash/Pexels have sufficient luxury pool/outdoor living photography for all phases | Some niche phases (acrylic window fabrication, rebar tying) may have limited free imagery |
| **A4** | Netlify free tier bandwidth (100GB/mo) is sufficient for initial traffic | If the site goes viral or gets significant organic traffic, may need to upgrade to paid tier |
| **A5** | The existing HumphreyPools site domain and hosting will remain accessible for cross-linking | If the main site moves domains, all cross-links need updating |
| **A6** | No legal restrictions on describing industry practices (standard vs. luxury comparisons) | Content should avoid naming specific competitors; use generic "standard builder" framing |
| **A7** | The site does not need to support multiple languages in v1 | North Dallas market is primarily English-speaking |

---

## 14. Success Metrics & Definition of Done

### Launch Criteria (Definition of Done for v1.0)

All of the following must be true before the site is considered launch-ready:

| # | Criterion | Verification Method |
|---|-----------|-------------------|
| **DOD-1** | All 8 service category overview pages are live and rendering correctly | Manual review + automated link checking |
| **DOD-2** | ≥ 6 phase deep-dive pages per category are published (minimum 48 total) | Content inventory count |
| **DOD-3** | Every published deep-dive page contains all 5 required content sections (summary, comparison table, narrative, pitfalls, differentiators) | Zod schema validation passes at build time |
| **DOD-4** | Comparison tables on all deep-dive pages have ≥ 5 rows | Schema validation (min array length) |
| **DOD-5** | Google Lighthouse mobile performance score ≥ 90 on all page templates | Lighthouse CI or manual audit on 3 representative pages |
| **DOD-6** | All pages render correctly on Chrome, Safari, Edge, and Firefox (latest 2 versions) | Cross-browser manual testing |
| **DOD-7** | All pages render correctly on mobile (375px), tablet (768px), and desktop (1280px+) | Responsive manual testing |
| **DOD-8** | Breadcrumb and phase navigation works on all published pages | Manual click-through testing |
| **DOD-9** | Site has `sitemap.xml`, `robots.txt`, unique meta descriptions, and OG tags on all pages | Automated SEO audit (Screaming Frog or similar) |
| **DOD-10** | Site is deployed to Netlify with automatic build-on-push configured | Verify by pushing a test commit and confirming auto-deploy |
| **DOD-11** | WCAG 2.1 AA compliance on color contrast and keyboard navigation | Lighthouse accessibility audit ≥ 90 |
| **DOD-12** | README documents local development setup, content authoring workflow, and deployment process | README review |

### Post-Launch Success Metrics (Measured at 30/60/90 Days)

| Metric | Target (90 days) | Measurement |
|--------|------------------|-------------|
| **Organic search impressions** | ≥ 1,000/month for luxury pool keywords | Google Search Console |
| **Average pages per session** | ≥ 3 phase deep-dives per visit | Google Analytics (once added in v1.1) |
| **Bounce rate on deep-dive pages** | < 40% | Google Analytics |
| **CTA click-through rate** | ≥ 2% of page views result in "Schedule Consultation" click | Event tracking (v1.1) |
| **Indexed pages** | 100% of published pages indexed by Google | Google Search Console |
| **Sales team feedback** | "Customers arrive more informed" in qualitative survey | Internal survey at 90 days |
| **Content coverage** | All 68 phase pages published | Content inventory |

---

## 15. Risks & Mitigations

| # | Risk | Probability | Impact | Mitigation |
|---|------|-------------|--------|------------|
| **R1** | Content authoring is the bottleneck (50–70 hrs of domain-specific writing) | High | High | Start with the Pool category (most critical, most phases) as a template; parallelize remaining categories once the pattern is established |
| **R2** | Placeholder images lack luxury credibility | Medium | Medium | Curate highest-quality Unsplash images; plan original photography shoot for v1.1; clearly mark timeline for image replacement |
| **R3** | Content inconsistency across 68+ pages (varying tone, depth, quality) | Medium | High | Establish content style guide (Section 9) before writing; template Markdown files with required frontmatter fields; Zod schema enforces structure |
| **R4** | Some niche phases lack good free placeholder imagery (e.g., acrylic window fabrication) | Medium | Low | Use related luxury imagery (e.g., architectural glass, precision manufacturing); add text overlay indicating "Luxury Detail" |
| **R5** | Subdirectory integration in v1.1 requires reverse proxy configuration | Low | Medium | Document integration plan now; Netlify `_redirects` handles most cases; subdomain fallback is simple alternative |
| **R6** | SEO competition for luxury pool keywords is intense | Medium | Medium | Focus on long-tail keywords specific to phases (e.g., "luxury pool gunite PSI requirements"); `HowTo` structured data for featured snippets |
| **R7** | Astro introduces Node.js build dependency where none existed | Low | Low | One-time setup cost; Netlify handles build environment; local development is `npm install && npm run dev` |
| **R8** | Content accuracy — incorrect construction details damage credibility | Medium | High | All content reviewed by domain expert (pool builder) before publication; flag uncertain claims for review |

---

## 16. Milestones & Estimated Effort

### Phase 1: Foundation (Estimated: 12–18 hours)

| Task | Hours | Deliverable |
|------|-------|-------------|
| Astro project scaffolding | 1–2 | Working project with build pipeline |
| Design system integration (CSS tokens, fonts, base layout) | 2–3 | `BaseLayout.astro` with matching luxury aesthetic |
| Content Collection schema (Zod) | 2–3 | `content.config.ts` with full schema |
| 5 reusable components | 4–6 | `ComparisonTable`, `QualitySummary`, `PitfallsList`, `LuxuryDifferentiators`, `PhaseNavigation` |
| Page templates (overview, category, deep-dive) | 3–4 | 3 working page templates |
| Netlify deployment | 1 | Auto-deploying on push |

### Phase 2: Pool Category (Pilot) (Estimated: 15–20 hours)

| Task | Hours | Deliverable |
|------|-------|-------------|
| Pool category overview content | 2–3 | Category page with timeline |
| 12 pool phase deep-dives (content research + writing) | 10–14 | 12 fully authored Markdown files with all required sections |
| Image curation (pool phases) | 2–3 | 15–20 curated placeholder images |
| QA and cross-browser testing | 1–2 | Verified on all target browsers/devices |

### Phase 3: Remaining Categories (Estimated: 35–50 hours)

| Task | Hours | Deliverable |
|------|-------|-------------|
| 7 remaining category overviews | 5–7 | 7 category pages |
| ~56 remaining phase deep-dives | 25–35 | Content authored and schema-validated |
| Image curation (all categories) | 4–6 | 80–100 curated placeholder images |
| Cross-linking and navigation QA | 1–2 | All navigation paths verified |

### Phase 4: Polish & Launch (Estimated: 5–8 hours)

| Task | Hours | Deliverable |
|------|-------|-------------|
| SEO audit and structured data | 2–3 | Meta tags, OG images, JSON-LD, sitemap verified |
| Performance audit | 1–2 | Lighthouse ≥ 90 on all templates |
| Accessibility audit | 1–2 | WCAG 2.1 AA compliance verified |
| Documentation (README) | 1 | Complete README with authoring + deployment guides |

### **Total Estimated Effort: 67–96 hours**

---

## Appendix A — Content Matrix

The following matrix shows the expected phase deep-dive pages. Each cell represents one Markdown file with full frontmatter (summary, comparison table, pitfalls, differentiators) and narrative body.

| Phase | Pool | Spa | Firepit | Firebowls | Acrylic Window | Kitchen | Patio Cover | Turf |
|-------|------|-----|---------|-----------|----------------|---------|-------------|------|
| Design & Planning | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Permits & Engineering | ✓ | ✓ | — | — | ✓ | ✓ | ✓ | — |
| Excavation | ✓ | ✓ | ✓ | — | ✓ | — | — | ✓ |
| Structural (Steel/Rebar/Framing) | ✓ | ✓ | — | — | ✓ | ✓ | ✓ | — |
| Plumbing | ✓ | ✓ | — | — | — | ✓ | — | — |
| Electrical & Lighting | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | — |
| Shell / Core (Gunite/Masonry/etc.) | ✓ | ✓ | ✓ | — | ✓ | — | ✓ | — |
| Materials & Surfaces | ✓ | ✓ | ✓ | ✓ | — | ✓ | ✓ | ✓ |
| Equipment & Systems | ✓ | ✓ | — | — | — | ✓ | — | — |
| Integration | — | ✓ | — | ✓ | — | ✓ | ✓ | — |
| Finishing & Detail | ✓ | ✓ | ✓ | — | ✓ | ✓ | ✓ | ✓ |
| Startup & Maintenance | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | — | ✓ |

**Legend:** ✓ = applicable deep-dive page, — = not applicable for this category

---

## Appendix B — Design Token Reference

Extracted from the existing HumphreyPools `css/styles.css` for shared use:

```css
/* ===== SHARED DESIGN TOKENS ===== */
:root {
  /* Colors */
  --color-primary: #0a1628;
  --color-primary-light: #142440;
  --color-accent: #c9a84c;
  --color-accent-light: #e0c97f;
  --color-white: #ffffff;
  --color-offwhite: #f7f5f0;
  --color-cream: #ede8dd;
  --color-text: #2c2c2c;
  --color-text-light: #6b6b6b;
  --color-border: #e0dcd4;

  /* Typography */
  --font-heading: 'Playfair Display', Georgia, serif;
  --font-body: 'Raleway', 'Segoe UI', sans-serif;
  --font-accent: 'Cormorant Garamond', Georgia, serif;

  /* Layout */
  --container-max: 1280px;
  --section-padding: 120px;
  --section-padding-mobile: 80px;

  /* Transitions */
  --transition-base: 0.3s ease;
  --transition-slow: 0.8s ease;
}
```

---

*End of Specification*
