# Research: Luxury Pool Construction Knowledge Website

> **Project:** Humphrey Luxury Pools — Educational Content Site
> **Date:** 2026-04-06 (Updated)
> **Status:** Research Complete — Ready for Architecture & Implementation
> **Last Verified:** April 6, 2026 — Astro 6.1.3 confirmed as latest stable; Cloudflare acquisition (Jan 2026) verified; all domain content cross-referenced with industry sources

---

## Key Research Sub-Questions (Prioritized by Impact)

1. **What technology stack best supports a content-heavy luxury educational site that shares the design DNA of the existing vanilla HTML HumphreyPools site?** — Determines every downstream technical decision. *(Answered: Astro 6.x — see §2)*
2. **What are all 10+ construction phases for a $250K+ luxury pool project, and what are the specific quality differences (budget vs. luxury) at each phase?** — The core content that drives the site's value proposition. *(Answered: 10 phases + 6 specialty features — see §6)*
3. **How should the progressive disclosure UX work — high-level overview → phase detail → deep-dive expandable sections — and what components are needed?** — Defines the site architecture and user experience. *(Answered: see §3.2)*
4. **What are the North Texas-specific considerations (expansive clay soil, municipal codes, climate) that make this content regionally authoritative?** — Differentiates from generic pool education content. *(Answered: see §1.5)*
5. **What specialty feature content (acrylic windows, fire features, outdoor kitchens, patio covers, turf) needs dedicated deep-dive pages?** — Expands content scope beyond basic pool construction. *(Answered: see §6, Specialty Features)*
6. **What luxury placeholder images and media strategy will convey the premium brand feel before original photography is available?** — Critical for first impressions and stakeholder buy-in. *(Answered: see §8.3)*
7. **How should the site be structured for future integration with the main HumphreyPools site (shared design tokens, subdomain vs. subdirectory, component reuse)?** — Ensures long-term architectural alignment. *(Answered: see §3.1, §7.3)*
8. **What is the optimal content production pipeline (Markdown/MDX authoring, image optimization, SEO metadata) to efficiently produce 30,000–40,000 words of expert content?** — Practical bottleneck for delivery timeline. *(Answered: see §8.4)*

---

## Table of Contents

1. [Domain & Market Research](#1-domain--market-research)
2. [Technology Stack Evaluation](#2-technology-stack-evaluation)
3. [Architecture Patterns & Design](#3-architecture-patterns--design)
4. [Libraries, Frameworks & Dependencies](#4-libraries-frameworks--dependencies)
5. [Security & Infrastructure](#5-security--infrastructure)
6. [Content Architecture: Pool Construction Phases](#6-content-architecture-pool-construction-phases)
7. [Risks, Trade-offs & Open Questions](#7-risks-trade-offs--open-questions)
8. [Implementation Recommendations](#8-implementation-recommendations)

---

## 1. Domain & Market Research

### 1.1 Core Domain Concepts & Terminology

This site targets the **luxury custom pool construction** market ($250K+ projects). Key terminology the content must cover:

| Category | Key Terms |
|----------|-----------|
| **Structural** | Gunite, shotcrete, rebar schedule, steel cage, PSI rating (4,000–7,000), engineered plans, stamped calculations, soil report, cold joints |
| **Finishes** | Waterline tile, coping, pebble finish (PebbleTec/PebbleSheen), quartz aggregate, glass bead, marcite plaster, travertine, porcelain pavers |
| **Water Features** | Infinity/vanishing edge, perimeter overflow, spillover spa, laminar jets, bubblers, deck jets, sheer descents, scuppers, grottos |
| **Fire Features** | Fire bowls, sunken fire pit, fire-and-water bowls, gas vs. ethanol burners, copper fire pots, linear fire troughs |
| **Specialty** | Acrylic pool windows, swim-up bars, tanning ledges (Baja shelves), beach entries, underwater benches |
| **Outdoor Living** | Outdoor kitchen, built-in grill, pergola/louvered roof, patio cover, pavilion, artificial turf, landscape lighting |
| **Equipment** | Variable-speed pumps, salt chlorine generators, automation systems (Pentair/Jandy), LED color lighting, ozone/UV sanitation |
| **Process** | Layout/staking, excavation, plumbing rough-in, steel/rebar, gunite shoot, tile & coping, decking, plaster/finish, startup, pool school |

### 1.2 Target Users

**Primary Audience:** Affluent homeowners ($500K–$5M+ home values) in North Dallas (Frisco, Prosper, McKinney, Celina, Allen, Plano) considering a luxury backyard transformation.

**Key User Workflows:**
1. **Research Phase** — High-level overview of what building a luxury pool entails ("What am I getting into?")
2. **Deep Dive** — Phase-by-phase education with quality vs. cut-corner comparisons ("How do I evaluate builders?")
3. **Feature Exploration** — Understanding specialty elements: acrylic windows, fire features, outdoor kitchens, turf
4. **Trust Building** — Seeing that Humphrey Pools understands every detail at an expert level
5. **Conversion** — Scheduling a consultation after being educated and impressed

### 1.3 Competitive Landscape

| Competitor Type | Examples | Gaps This Site Fills |
|----------------|----------|---------------------|
| Generic pool builder sites | Mission Pools, Premier Pools | Surface-level process pages; no quality comparison detail |
| Pool education blogs | Pool Research, River Pools blog | Informational but no luxury focus; not brand-aligned |
| Luxury pool builders | Platinum Pools, Southern Pool Designs | Beautiful portfolios but shallow on educational content |
| YouTube pool content | Pool Guy, Swimming Pool Steve | Video-first; no structured progressive-disclosure reading experience |

**Differentiator:** No luxury pool builder combines deep educational content (phase-by-phase quality comparisons) with a luxury brand experience. This site fills that exact gap.

### 1.4 Compliance & Standards

- **No regulated content** — This is educational/marketing, not e-commerce or healthcare
- **ADA/WCAG 2.1 AA** — Important for accessibility and SEO
- **Texas pool construction codes** — Referenced in content but not a compliance burden for the website itself
- **Image licensing** — Placeholder images must use royalty-free luxury pool photography (Unsplash, Pexels) replaceable with original photography later

### 1.5 North Texas Regional Considerations

This content targets a specific geography — North Dallas (Celina, Frisco, Prosper, McKinney, Allen, Plano). Regional expertise is a key differentiator:

| Factor | Details | Impact on Content |
|--------|---------|-------------------|
| **Expansive Clay Soil** | 90%+ of DFW sits on expansive clay that swells when wet, contracts when dry — causes ground movement of 2–6 inches seasonally | Every phase from excavation to decking must address this; major content differentiator |
| **Soil Stabilization** | Chemical soil injections (e.g., SWIMSOIL, Earthlok, ESSL, Atlas Soil Stabilization), engineered piers, helical piles driven to stable strata below clay; standard injection depth ~7 feet, deeper for high-swell zones | Quality-tier differentiator: budget builders skip this ($3K–$8K savings that causes $30K+ in damage) |
| **Geotechnical Reports** | Soil borings determine clay composition, plasticity index, bearing capacity | Content should explain why this $500–$1,500 report prevents catastrophic structural failure |
| **Climate** | Hot summers (100°F+), mild winters, occasional hard freezes, severe thunderstorms | Impacts material selection (heat-resistant deck materials, freeze-thaw tile considerations, drainage for heavy rain) |
| **Municipal Variations** | Each city (Frisco, Prosper, Celina, McKinney) has different setback requirements, permit timelines, and inspection schedules | Content opportunity: builder who handles all permitting across municipalities |
| **HOA Considerations** | Many luxury neighborhoods have Architectural Review Committees (ARC) with specific requirements | Adds a pre-construction approval phase not present in generic content |
| **Construction Season** | Year-round possible but optimal March–November; concrete curing affected by extreme heat/cold | Content about timing your project and seasonal quality considerations |

**Why this matters for the site:** Generic pool education sites ignore regional soil and climate. A dedicated "Building in North Texas Clay Soil" callout in the excavation, steel, gunite, and decking phases makes this content uniquely authoritative and locally trusted.

---

## 2. Technology Stack Evaluation

### 2.1 Existing Site Analysis (HumphreyPools GitHub Repo)

The reference repo (`azurenerd/HumphreyPools`) is a **single-page vanilla HTML/CSS/JS** site with:

| Aspect | Details |
|--------|---------|
| **Structure** | Single `index.html` (~23KB), `css/styles.css` (~24KB), `js/main.js` (~5KB) |
| **Design System** | CSS custom properties: `--color-primary: #0a1628`, `--color-accent: #c9a84c` (gold), `--color-offwhite: #f7f5f0` |
| **Typography** | Playfair Display (headings), Raleway (body), Cormorant Garamond (accent) — Google Fonts |
| **Interactivity** | IntersectionObserver for scroll reveals, smooth scrolling, counter animation, mobile menu, video autoplay on visibility |
| **No Dependencies** | Zero npm packages, no build step, no framework |
| **Content Sections** | Hero, About, Video Showcase, Process (4 steps), Services (3 cards), Gallery, Trust/Stats, CTA, Social, Footer |

**Critical Constraint:** The new site must share the same design DNA so it can be merged into or linked from the existing site seamlessly.

### 2.2 Candidate Technology Stacks

#### Option A: Astro 6 (⭐ RECOMMENDED)

| Dimension | Details |
|-----------|---------|
| **Framework** | Astro 6.1.3 (latest stable as of April 2026; 6.0 released March 10, 2026) |
| **Rendering** | Static Site Generation (SSG) — zero client JS by default |
| **Styling** | Vanilla CSS using existing design tokens from HumphreyPools, scoped component styles |
| **Content** | Astro Content Collections (Markdown/MDX files for each phase section) + Live Content Collections (new in 6.0 — can fetch from external CMS at runtime if needed later) |
| **Fonts** | Built-in Fonts API (new in Astro 6) — auto-downloads, caches, self-hosts Google Fonts for performance |
| **Interactivity** | Astro Islands — hydrate only expand/collapse and navigation components |
| **Build Tool** | Vite 7 (built-in, upgraded from Vite 6 in Astro 5) |
| **Runtime** | Node.js 22 (required by Astro 6; Node 18/20 no longer supported) |
| **Schema** | Zod 4 for content validation (upgraded from Zod 3 in Astro 5) |
| **Syntax Highlighting** | Shiki 4 (upgraded from Shiki 1.x in Astro 5) |
| **Output** | Static HTML/CSS/JS — identical output format to existing site |
| **Learning Curve** | Low-moderate (HTML-like `.astro` component syntax) |

**Why Astro 6 wins for this project:**
- **Design compatibility**: Astro outputs plain HTML/CSS/JS — can share stylesheets, fonts, and design tokens with the existing vanilla site 1:1
- **Content-first**: Built for exactly this use case — lots of editorial content with progressive disclosure
- **Islands architecture**: Only the interactive expand/collapse sections get JavaScript; everything else is pure HTML
- **Built-in Fonts API** (new in Astro 6): Automatically downloads, caches, and self-hosts Google Fonts (Playfair Display, Raleway, Cormorant Garamond) — eliminates render-blocking external font requests and improves Core Web Vitals
- **Content Security Policy (CSP) API** (new in Astro 6): Built-in CSP hashing for scripts and styles — security best practice with zero configuration
- **Redesigned Dev Server** (new in Astro 6): Uses Vite's Environment API so dev mirrors production runtime exactly
- **Content Layer with `glob()` loader**: Markdown builds are up to 5x faster with 25–50% less memory usage vs. legacy Content Collections
- **Zero lock-in**: If the team wants to migrate back to vanilla HTML later, Astro's output IS vanilla HTML
- **SEO**: Pre-rendered HTML with perfect Core Web Vitals scores
- **Component reuse**: Shared header, footer, design tokens across both sites without code duplication
- **Server Islands**: Can add dynamic components (e.g., contact form, analytics) to otherwise static pages without full SSR
- **Experimental Rust Compiler**: Astro 6 introduces an experimental Rust-based compiler (replacing the legacy Go compiler), anticipated to significantly improve build performance and scalability in future releases
- **Live Content Collections**: Previously build-time only, content collections can now fetch from external sources at runtime — future-proofs the site for headless CMS integration without refactoring
- **Note on Cloudflare acquisition**: Astro was acquired by Cloudflare in January 2026 but remains MIT-licensed and open source. Cloudflare Pages is now the "golden path" deployment target, but GitHub Pages, Netlify, and Vercel are still fully supported. The full Astro team joined Cloudflare and continues working on Astro full-time

#### Option B: Enhanced Vanilla HTML/CSS/JS

| Dimension | Details |
|-----------|---------|
| **Framework** | None |
| **Rendering** | Static HTML files |
| **Styling** | CSS with shared design tokens |
| **Content** | Hand-coded HTML sections |
| **Interactivity** | Custom JS for expand/collapse |
| **Build Tool** | None |
| **Output** | Static HTML/CSS/JS |
| **Learning Curve** | Lowest |

**Pros:** Zero tooling, identical to existing site, no build step
**Cons:** Content duplication across pages, no component reuse, manual maintenance of repeated elements (header, footer, nav), difficult to scale to 15+ deep-dive sections. Would result in massive HTML files or many nearly-identical HTML files with duplicated chrome.

#### Option C: Next.js 15 (Static Export)

| Dimension | Details |
|-----------|---------|
| **Framework** | Next.js 15.x with App Router |
| **Rendering** | Static Export (`output: 'export'`) |
| **Styling** | CSS Modules or Tailwind |
| **Content** | MDX with `next-mdx-remote` |
| **Interactivity** | React components |
| **Build Tool** | Webpack/Turbopack |
| **Output** | Static HTML/CSS/JS (with React runtime) |
| **Learning Curve** | Moderate-high |

**Pros:** Powerful ecosystem, great DX
**Cons:** Ships React runtime (~40KB+) for a content site that doesn't need it; design system diverges from vanilla CSS approach; overkill for this use case; harder to merge with existing vanilla site

### 2.3 Stack Decision Matrix

| Criteria (Weight) | Astro 6 | Vanilla HTML | Next.js 15 |
|-------------------|---------|-------------|------------|
| Design compatibility with existing site (25%) | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| Content management at scale (20%) | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| Performance / Core Web Vitals (15%) | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| Progressive disclosure UX (15%) | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Developer productivity (10%) | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| Future extensibility (10%) | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| Team learning curve (5%) | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Weighted Score** | **4.85** | **3.40** | **3.90** |

### 2.4 Recommended Stack

> **Primary: Astro 6.1.3** with vanilla CSS (porting existing design tokens), Content Collections for phase content, built-in Fonts API for self-hosted Google Fonts, deployed to **GitHub Pages** (free, matches existing repo hosting). Alternatively **Cloudflare Pages** (free unlimited bandwidth, now Astro's "golden path" after January 2026 acquisition).

---

## 3. Architecture Patterns & Design

### 3.1 Site Architecture Pattern: Static Site with Islands

```
┌─────────────────────────────────────────────┐
│                ASTRO BUILD                   │
│  ┌─────────────────────────────────────────┐ │
│  │  Content Collections (Markdown/MDX)     │ │
│  │  ├── phases/01-design-planning.md       │ │
│  │  ├── phases/02-excavation.md            │ │
│  │  ├── phases/03-steel-rebar.md           │ │
│  │  ├── phases/04-plumbing.md              │ │
│  │  ├── phases/05-gunite-shotcrete.md      │ │
│  │  ├── phases/06-tile-coping.md           │ │
│  │  ├── phases/07-decking-hardscape.md     │ │
│  │  ├── phases/08-equipment.md             │ │
│  │  ├── phases/09-interior-finish.md       │ │
│  │  ├── phases/10-startup-orientation.md   │ │
│  │  ├── features/acrylic-windows.md        │ │
│  │  ├── features/fire-features.md          │ │
│  │  ├── features/outdoor-kitchen.md        │ │
│  │  ├── features/patio-covers.md           │ │
│  │  └── features/turf-landscaping.md       │ │
│  └─────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────┐ │
│  │  Layouts & Components (.astro)          │ │
│  │  ├── LuxuryLayout.astro (shared chrome) │ │
│  │  ├── PhaseCard.astro                    │ │
│  │  ├── QualityComparison.astro            │ │
│  │  ├── ComparisonTable.astro              │ │
│  │  ├── ExpandableSection.astro            │ │
│  │  ├── PhaseTimeline.astro                │ │
│  │  └── HeroSection.astro                  │ │
│  └─────────────────────────────────────────┘ │
│                    ↓ BUILD ↓                 │
│  ┌─────────────────────────────────────────┐ │
│  │  Static Output (HTML/CSS/JS)            │ │
│  │  ├── index.html (overview + timeline)   │ │
│  │  ├── phases/design-planning/index.html  │ │
│  │  ├── phases/excavation/index.html       │ │
│  │  ├── ... (one page per phase)           │ │
│  │  ├── features/acrylic-windows/index.html│ │
│  │  └── css/styles.css (shared design)     │ │
│  └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

### 3.2 Page Structure: Progressive Disclosure Pattern

Each construction phase page follows this template:

```
┌──────────────────────────────────────────┐
│  PHASE HERO (image + title + number)     │
├──────────────────────────────────────────┤
│  EXECUTIVE SUMMARY                       │
│  "What luxury builders do differently"   │
│  (3-4 bullet gold-accented callout box)  │
├──────────────────────────────────────────┤
│  COMPARISON TABLE                        │
│  Standard Builder │ Quality │ Luxury     │
│  ─────────────────┼─────────┼──────────  │
│  Detail row 1     │  ...    │  ...       │
│  Detail row 2     │  ...    │  ...       │
├──────────────────────────────────────────┤
│  DEEP DIVE SECTIONS (expandable)         │
│  ▸ What Happens In This Phase            │
│  ▸ Materials & Specifications            │
│  ▸ Common Pitfalls & Red Flags           │
│  ▸ Questions to Ask Your Builder         │
│  ▸ What We Do Differently                │
├──────────────────────────────────────────┤
│  PHASE GALLERY (3-4 luxury images)       │
├──────────────────────────────────────────┤
│  NAVIGATION (← Previous | Next →)        │
└──────────────────────────────────────────┘
```

### 3.3 Information Architecture

```
HOME (Overview + Interactive Phase Timeline)
├── Phase 1: Design & Planning
├── Phase 2: Excavation & Layout
├── Phase 3: Steel & Rebar
├── Phase 4: Plumbing & Electrical
├── Phase 5: Gunite / Shotcrete
├── Phase 6: Tile, Coping & Masonry
├── Phase 7: Decking & Hardscape
├── Phase 8: Equipment & Automation
├── Phase 9: Interior Finish (Plaster/Pebble/Quartz)
├── Phase 10: Startup, Inspection & Pool School
│
├── SPECIALTY FEATURES
│   ├── Acrylic Pool Windows
│   ├── Fire Features (Bowls, Sunken Pits, Linear Troughs)
│   ├── Spas & Water Features
│   ├── Outdoor Kitchens
│   ├── Patio Covers & Pergolas
│   └── Artificial Turf & Landscaping
│
└── ABOUT / CONTACT (links back to main HumphreyPools site)
```

### 3.4 Data Storage Strategy

**No database required.** All content lives in Markdown files within the Astro project, managed via the **Astro 6 Content Layer** with Zod schemas for type safety. This is a purely static, content-driven site.

- **Phase content**: Markdown with YAML frontmatter (title, phase number, hero image, summary bullets, comparison table data), loaded via `glob()` loader
- **Schema validation**: Zod schemas ensure every phase file has required fields (title, phaseNumber, heroImage, executiveSummary, comparisonRows)
- **Images**: Static assets in `public/images/` (placeholder Unsplash/Pexels images initially), optimized at build time via Sharp
- **Design tokens**: Shared CSS custom properties file imported across all pages

**Example Content Layer config:**
```js
import { defineCollection, z } from 'astro:content';
import { glob } from 'astro/loaders';

const phases = defineCollection({
  loader: glob({ pattern: "**/*.md", base: "./src/content/phases" }),
  schema: z.object({
    title: z.string(),
    phaseNumber: z.number(),
    duration: z.string(),
    heroImage: z.string(),
    executiveSummary: z.string(),
    comparisonRows: z.array(z.object({
      aspect: z.string(),
      budget: z.string(),
      luxury: z.string(),
    })),
  }),
});

const features = defineCollection({
  loader: glob({ pattern: "**/*.md", base: "./src/content/features" }),
  schema: z.object({
    title: z.string(),
    heroImage: z.string(),
    category: z.enum(['water', 'fire', 'outdoor-living', 'landscape']),
  }),
});

export const collections = { phases, features };
```

### 3.5 No API Needed

This is a static content site. No REST, GraphQL, or backend API is required. If a contact form is desired later, it can use a third-party service (Formspree, Netlify Forms) without backend infrastructure.

---

## 4. Libraries, Frameworks & Dependencies

### 4.1 Core Dependencies

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| `astro` | `^6.1.3` (latest stable) | Framework / SSG | MIT |
| `@astrojs/mdx` | `^4.x` | MDX support for rich content | MIT |
| `sharp` | `^0.34.x` | Image optimization (built-in Astro integration) | Apache-2.0 |

> **Note:** Astro 6 requires **Node.js 22**, uses **Vite 7**, **Zod 4**, and **Shiki 4** internally. The built-in Fonts API replaces the need for manual Google Fonts `<link>` tags. The legacy Content Collections API has been removed — all collections must use the new Content Layer API with `glob()` loader.

### 4.2 Optional Enhancements

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| `astro-icon` | `^1.x` | SVG icon components | MIT |
| `@astrojs/sitemap` | `^3.x` | Auto-generated sitemap for SEO | MIT |
| `@astrojs/rss` | `^4.x` | RSS feed (if blog content added later) | MIT |

### 4.3 Development Tooling

| Tool | Purpose |
|------|---------|
| `prettier` + `prettier-plugin-astro` | Code formatting |
| `eslint` | JS linting |
| GitHub Actions | CI/CD pipeline for build + deploy to GitHub Pages |

### 4.4 Fonts (External, No Package)

Loaded via Google Fonts CDN (same as existing site):
- **Playfair Display** (headings) — OFL license
- **Raleway** (body) — OFL license  
- **Cormorant Garamond** (accent/italic) — OFL license

### 4.5 No Licensing Concerns

All recommended dependencies are MIT or Apache-2.0 licensed. No GPL or restrictive licenses in the dependency tree.

---

## 5. Security & Infrastructure

### 5.1 Security Profile

This is a **static site with no backend, no database, no user accounts, and no sensitive data**. The security surface is minimal:

| Concern | Mitigation |
|---------|------------|
| XSS | No user input; all content is pre-rendered at build time |
| Content injection | Markdown content is sanitized by Astro's built-in renderer |
| Dependencies | Minimal deps (3 core packages); `npm audit` in CI |
| Image hotlinking | Use optimized local images, not hotlinked external URLs |

### 5.2 Hosting & Deployment

**Recommended: GitHub Pages** (free tier)

| Aspect | Details |
|--------|---------|
| **Provider** | GitHub Pages via GitHub Actions |
| **Cost** | $0/month (included with GitHub free tier) |
| **CDN** | GitHub's global CDN (Fastly-backed) |
| **SSL** | Free, automatic HTTPS |
| **Custom Domain** | Supported (e.g., `guide.humphreyluxurypools.com`) |
| **Build** | GitHub Actions workflow: `npm run build` → deploy `dist/` |
| **Bandwidth** | 100GB/month soft limit (more than sufficient) |

**Alternative options if needs grow:**

| Provider | Free Tier | When to Consider |
|----------|-----------|------------------|
| **Netlify** | 100GB bandwidth, 300 build min/month | If form handling or serverless functions needed |
| **Vercel** | 100GB bandwidth, unlimited deploys | If SSR or edge functions needed later |
| **Cloudflare Pages** | Unlimited bandwidth | If global CDN performance becomes critical |

### 5.3 Infrastructure Cost Estimates

| Scale | Monthly Cost | Notes |
|-------|-------------|-------|
| **Small** (< 10K visitors/month) | **$0** | GitHub Pages free tier handles this easily |
| **Medium** (10K–100K visitors/month) | **$0–$20** | Still free on GitHub Pages; $20 if custom domain + Cloudflare |
| **Large** (100K+ visitors/month) | **$0–$50** | Cloudflare Pages (free unlimited bandwidth) or Netlify Pro |

### 5.4 CI/CD Pipeline

```yaml
# .github/workflows/deploy.yml
name: Deploy to GitHub Pages
on:
  push:
    branches: [main]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 22
      - run: npm ci
      - run: npm run build
      - uses: actions/upload-pages-artifact@v3
        with:
          path: dist/
  deploy:
    needs: build
    runs-on: ubuntu-latest
    permissions:
      pages: write
      id-token: write
    environment:
      name: github-pages
    steps:
      - uses: actions/deploy-pages@v4
```

---

## 6. Content Architecture: Pool Construction Phases

This section outlines the **domain content** that each phase page should cover. This is the core value of the site — deep, expert-level education that builds trust and differentiates Humphrey Pools.

### Phase 1: Design & Planning (2–6 weeks)

**Executive Summary:** The most important phase. Luxury builders invest weeks in detailed 3D design and engineering; budget builders rush to get a deposit and start digging.

| Aspect | Budget Builder | Quality Builder | Luxury Builder (Humphrey) |
|--------|---------------|-----------------|--------------------------|
| **Design Tool** | Hand sketch or basic 2D | 3D rendering software | Photorealistic 3D walkthrough + VR option |
| **Engineering** | Generic template plans | Engineered plans (basic) | Stamped engineer plans with soil report |
| **Site Assessment** | Visual only | Basic survey | Full geotechnical soil report + utility locate + drainage analysis |
| **Design Iterations** | 1 revision | 2–3 revisions | Unlimited until perfect |
| **Permit Handling** | Homeowner responsibility | Builder assists | Builder handles 100% |
| **Timeline** | 1 week | 2–3 weeks | 3–6 weeks (thoroughness over speed) |

**Deep dive topics:**
- Why a soil/geotech report matters ($500 that saves $50,000)
- How 3D design prevents costly change orders
- Red flags: builders who won't show engineering drawings
- What stamped engineer plans actually include and why they matter
- Permit process in North Dallas municipalities

### Phase 2: Excavation & Layout (1–2 weeks)

**Executive Summary:** Precision layout determines everything downstream. Off by 2 inches here = problems for months. In North Texas, expansive clay soil makes this phase especially critical.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Layout Method** | Spray paint from sketch | GPS/laser staking from CAD plans |
| **Excavation Equipment** | Standard backhoe | Precision excavator with experienced operator |
| **Depth Accuracy** | ±6 inches | ±1 inch |
| **Soil Hauling** | Dumped on-site | Hauled off or precision-graded for landscape |
| **Utility Protection** | Assumed locations | Professional utility locate (811 + private locate) |
| **Benching/Shoring** | Minimal | Full OSHA-compliant benching |
| **Soil Stabilization** | Skipped entirely | Chemical soil injection (SWIMSOIL or equivalent) for expansive clay |
| **Geotechnical Report** | Not performed | Full soil boring analysis with plasticity index and bearing capacity |

**Deep dive topics:**
- What happens when excavation reveals rock, water table, or unstable soil
- **North Texas Clay Alert**: Why expansive clay causes 2–6 inches of seasonal ground movement and how soil stabilization ($3K–$8K) prevents $30K+ in future damage
- How elevation changes affect pool/spa integration
- Common dig mistakes and their downstream costs
- Why luxury pools need a wider dig (for thick walls, equipment access)
- Geotechnical reports: what soil borings reveal and why the $500–$1,500 investment is non-negotiable in DFW clay

### Phase 3: Steel & Rebar (1–2 weeks)

**Executive Summary:** The skeleton of your pool. This is where the most corners are cut because it's hidden under concrete. A quality rebar job costs $3,000–$5,000 more but prevents $30,000+ in structural failure.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Rebar Gauge** | #3 (3/8") minimum | #4 (1/2") standard, #5 for walls |
| **Grid Spacing** | 12" on-center | 6"–8" on-center |
| **Overlap/Lap** | Minimal or missing | 40+ bar diameters per ACI standards |
| **Steel Placement** | Center of shell | Positioned per engineer's spec (tension side) |
| **Dowels at Steps/Benches** | Skipped | Full doweling at every transition |
| **Spa Connection** | Single tie | Continuous reinforcement with expansion joints |
| **Inspection** | Builder self-inspects | Independent structural inspection + city inspection |

**Deep dive topics:**
- Why rebar spacing matters (crack prevention, structural load distribution)
- The difference between #3 and #4 rebar in real-world durability
- How to read a steel inspection report
- What "cold joints" are and why they're dangerous
- Red flag: builder who won't let you see the rebar before gunite

### Phase 4: Plumbing & Electrical (1 week)

**Executive Summary:** Undersized plumbing is the #1 cause of poor water circulation, dead spots, and algae problems. Luxury pools need 2–3x the plumbing of a standard pool.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Pipe Size** | 1.5" PVC everywhere | 2"–3" mains, sized per hydraulic calculations |
| **Return Lines** | 2–3 returns | 6–10+ returns for uniform circulation |
| **Suction Lines** | Single main drain | Dual main drains + skimmer(s) per VGB Act |
| **Plumbing Material** | Schedule 40 PVC | Schedule 40 minimum, flex PVC at equipment |
| **Electrical** | Basic code compliance | Dedicated subpanel, surge protection, smart automation wiring |
| **Gas Lines** | Basic heater line | Sized for heater + spa + fire features + outdoor kitchen |
| **Bonding/Grounding** | Code minimum | Full equipotential bonding grid per NEC 680 |

**Deep dive topics:**
- Hydraulic design: flow rates, TDH (total dynamic head), and why they matter
- Why the VGB Act (Virginia Graeme Baker) matters for drain safety
- Automation pre-wiring: planning for future upgrades
- Common plumbing mistakes that cause air locks and poor circulation

### Phase 5: Gunite / Shotcrete Application (1–2 weeks + 7–14 day cure)

**Executive Summary:** The concrete shell IS your pool. Gunite vs. shotcrete both work when applied correctly, but application skill and proper curing are non-negotiable.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Method** | Shotcrete (pre-mixed, faster) | Gunite (dry-mix, on-site water control) for custom shapes |
| **Shell Thickness** | 6" minimum (may vary) | 8"–12" uniform, 12"–18" at raised walls/windows |
| **PSI Target** | 3,500+ | 5,000–7,000 PSI |
| **Nozzleman Experience** | Varies | ACI-certified nozzleman |
| **Curing Period** | 3–5 days | 7–14 days with active water curing |
| **Rebound Management** | Left in place | Fully cleaned out before sets |
| **Overspray Cleanup** | Minimal | Full cleanup of surrounding areas |

**Deep dive topics:**
- Gunite vs. shotcrete: when each is appropriate
- What happens during the curing process and why shortcuts crack pools
- How to verify shell thickness (core samples)
- The nozzleman's skill: why this single person determines your pool's lifespan
- Rebound material: what it is and why leaving it in is structural fraud

### Phase 6: Tile, Coping & Masonry (2–3 weeks)

**Executive Summary:** This is where your pool's character is defined. Tile and coping are the most visible elements and the first to show if corners were cut.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Waterline Tile** | Basic 6×6 ceramic | Custom glass mosaic, iridescent, or hand-painted |
| **Tile Depth** | Single row at waterline | Full waterline band (6–12" deep) + accent tiles |
| **Coping Material** | Poured concrete bull-nose | Natural travertine, limestone, or custom-cut stone |
| **Coping Attachment** | Mortar only | Mortar + mechanical anchoring |
| **Stone Sourcing** | Bulk import (inconsistent) | Hand-selected, grade-A, consistent color |
| **Grout** | Standard sanded grout | Epoxy grout (waterproof, stain-proof, lasts 25+ years) |

**Deep dive topics:**
- Glass tile vs. ceramic vs. porcelain vs. natural stone: durability and aesthetics
- Why travertine coping stays cool (thermal conductivity)
- Grout matters: standard vs. epoxy and the long-term cost difference
- Color coordination: how tile and coping interact with plaster color and water appearance
- Installation precision: lippage tolerance and why millimeters matter

### Phase 7: Decking & Hardscape (2–3 weeks)

**Executive Summary:** The deck connects pool to home and defines the outdoor living space. It's also the largest surface area and highest-traffic zone.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Material** | Stamped/stained concrete | Travertine pavers, porcelain tile, natural stone |
| **Base Preparation** | Minimal compaction | Engineered base: 4–6" compacted aggregate + sand leveling |
| **Drainage** | Surface grading only | Trench drains, channel drains, French drains as needed |
| **Expansion Joints** | Code minimum | Calculated per thermal expansion for material type |
| **Integration** | Pool deck only | Unified with outdoor kitchen, fire pit, walkways, turf zones |
| **Edge Treatment** | Square cut | Bull-nose, tumbled, or custom profile matching coping |

### Phase 8: Equipment & Automation (1–2 weeks)

**Executive Summary:** Equipment determines ongoing costs, noise, and convenience. A luxury equipment pad costs $5,000–$15,000 more but saves $2,000+/year in energy and chemicals.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Pump** | Single-speed (high energy cost) | Variable-speed (Pentair IntelliFlo, 80% energy savings) |
| **Sanitation** | Basic liquid chlorine | Salt chlorine generator + UV/ozone secondary |
| **Heating** | Gas heater only | Gas + heat pump hybrid (efficient for year-round use) |
| **Automation** | None or basic timer | Full automation (Pentair IntelliCenter / Jandy iAqualink) with app control |
| **Lighting** | Single white light | Color LED system (Pentair IntelliBrite) with zones |
| **Equipment Pad** | Exposed on concrete | Screened/landscaped enclosure with sound dampening |
| **Equipment Access** | Minimal clearance | Designed for service access with unions at all connections |

### Phase 9: Interior Finish — Plaster, Pebble & Quartz (1 week)

**Executive Summary:** The interior finish determines the color, feel, and lifespan of your pool surface. This is the last thing applied and the most visible.

| Finish Type | Lifespan | Feel | Luxury Look | Cost Range |
|-------------|----------|------|-------------|-----------|
| **White Plaster (Marcite)** | 7–10 years | Very smooth | Classic but basic | $ |
| **Colored Plaster** | 7–12 years | Smooth | Better depth | $$ |
| **Quartz Aggregate** | 12–18 years | Smooth-textured | Shimmering, vibrant | $$–$$$ |
| **Pebble (PebbleTec)** | 15–25+ years | Textured | Natural, exotic | $$$–$$$$ |
| **Glass Bead (PebbleSheen)** | 15–20+ years | Smooth-pebble | Luminous, refined | $$$$ |

**Deep dive topics:**
- How plaster color changes water appearance (white = light blue, dark gray = lagoon)
- Application day: why weather and crew timing are critical
- Startup chemistry: the 30-day process that determines finish longevity
- Why cheap plaster fails: improper calcium chloride, rushed application, bad water chemistry
- Quartz vs. pebble: the real-world comfort-under-foot comparison

### Phase 10: Startup, Inspection & Pool School (1–2 days)

**Executive Summary:** The first 30 days of a new pool are the most critical for surface longevity. Luxury builders provide hands-on startup service; budget builders hand you a manual.

| Aspect | Budget Builder | Luxury Builder |
|--------|---------------|----------------|
| **Startup Service** | Homeowner responsibility | Builder manages 30-day startup chemistry program |
| **Water Balancing** | Basic test + chemicals | LSI (Langelier Saturation Index) balanced bi-weekly |
| **Pool School** | 15-minute walkthrough | 1–2 hour hands-on session with automation demo |
| **Warranty** | 1 year structural | 5+ year structural, 2+ year equipment, finish warranty |
| **Post-Build Support** | "Call if there's a problem" | Scheduled 30/60/90-day check-ins |

---

### Specialty Feature: Acrylic Pool Windows

**Key content points:**
- Custom manufactured panels (lead time: 6–12 weeks) — panels are engineered per pool's water depth, size, and shape
- Require 15–18" thick reinforced walls (vs. standard 10") with concrete U-channel larger than the panel
- Panel thickness ranges from 1.5" to 8"+ depending on window size and water depth (2" typical for residential)
- U-channel construction with waterproof slurry and UV-resistant, non-corrosive sealants
- 92% light transmittance; scratch-repairable without draining (modern coatings help)
- Frame: marine-grade stainless steel recommended for all installations
- Cost: $15,000–$30,000 for standard residential panels (8'×4' range); $50,000–$150,000+ for panoramic, curved, or infinity-edge windows
- Requires specialist installation team (not standard pool crew) — panels delivered crated, positioned with cranes
- Seal inspection/replacement needed every 5 years; gentle cleaning required to prevent scratches
- Top manufacturers (Reynolds Polymer, Lucite) offer 20–30 year panel warranties
- **Luxury differentiator**: Budget builders won't even offer this; it requires structural engineering from day one

| Quality Tier | Standard Builder | Luxury Builder |
|-------------|-----------------|----------------|
| **Panel Sourcing** | N/A (not offered) | Custom-engineered by specialty fabricator |
| **Wall Thickness** | N/A | 15–18" reinforced concrete with U-channel |
| **Sealant** | N/A | UV-resistant, certified waterproof sealant system |
| **Installation Team** | N/A | Dedicated acrylic glazing specialists |
| **Warranty** | N/A | 20–30 year panel + installation warranty |

### Specialty Feature: Fire Features

**Types to cover:**
- **Fire bowls** — Copper or concrete, mounted on pedestals or pool wall; dramatic when lit with water features
- **Sunken fire pits** — Lowered seating area with built-in benches, can include water "moat" effect; creates intimate gathering zone
- **Linear fire troughs** — Modern, clean lines; gas-fed; ideal for contemporary pool designs
- **Fire-and-water bowls** — Dramatic combination, requires dual plumbing (gas + water supply)

**Key quality differences:**
- Gas line sizing (BTU calculations for each burner — undersized = weak flame, oversized = safety risk)
- Wind protection considerations (glass wind guards, recessed placement)
- Electronic vs. manual ignition (electronic with app control for luxury)
- Material quality: cast concrete vs. copper vs. stainless steel (copper develops patina, stainless resists corrosion)
- **Safety**: Non-slip materials for steps/seating around wet-to-dry transition zones
- **Integration**: Sunken fire pit placement relative to pool for optimal sightlines and traffic flow
- **Drainage**: Sunken areas require dedicated drainage to prevent water accumulation

| Quality Tier | Budget Builder | Luxury Builder |
|-------------|---------------|----------------|
| **Fire Bowl Material** | Pre-cast concrete | Hand-poured concrete, hammered copper, or stainless steel |
| **Gas Line** | Basic, minimum sizing | Engineered BTU calculations per burner + manifold |
| **Ignition** | Match-lit or manual | Electronic with smart home/app integration |
| **Wind Protection** | None | Tempered glass wind guards or recessed design |
| **Seating (Sunken Pit)** | Movable furniture | Built-in stone/concrete benches with drainage |
| **Electrical** | Basic outlet nearby | Integrated LED accent lighting + automated control |

### Specialty Feature: Outdoor Kitchens

**Key content points:**
- Stainless steel framing + marine-grade polymer cabinetry (resists moisture, UV, insects)
- Countertops: granite, quartzite, or soapstone (heat + weather resistant); avoid marble (etches/stains)
- Appliance integration: grill, side burners, refrigerator, wine cooler, pizza oven, ice maker
- Plumbing: sink with hot/cold water, gas lines, proper drainage with grease trap
- Electrical: GFCI outlets, task lighting, TV/audio pre-wire, dedicated circuits
- **Louvered pergola integration**: Position kitchen under cover for weather protection and year-round usability
- **Layout principle**: Design for natural traffic flow between cooking, dining, pool, and lounging zones
- **Proximity to house**: Closer = easier utility access, but consider views and spatial balance

| Quality Tier | Budget Builder | Luxury Builder |
|-------------|---------------|----------------|
| **Cabinetry** | Wood or basic metal (rots/rusts) | Marine-grade polymer or 304 stainless steel |
| **Countertop** | Tile or basic granite | Quartzite, soapstone, or premium granite (3cm thick) |
| **Grill** | Basic built-in | Premium brand (Lynx, Alfresco, Twin Eagles) with rotisserie |
| **Refrigeration** | Dorm-size fridge | Full-size outdoor-rated fridge + wine cooler + ice maker |
| **Sink** | Basic bar sink | Full prep sink with hot water, soap dispenser, disposal |
| **Electrical** | Single outlet | Dedicated subpanel, TV pre-wire, surround sound, smart controls |
| **Cover** | None or basic umbrella | Integrated louvered pergola with lighting, fans, heaters |

### Specialty Feature: Patio Covers & Pergolas

**Types to cover:**
- **Aluminum louvered roofs** (e.g., Azenco, StruXure) — Motorized, watertight when closed, adjustable slats for sun/shade control; integrated gutters, lighting, fans, and heating available; 15–25 year lifespan
- **Cedar/hardwood pergolas** — Classic aesthetic, requires periodic sealing (every 2–3 years); natural warmth
- **Steel frame with fabric** — Modern, lightweight; fabric replacement every 5–8 years
- **Full pavilion/roof structure** — Complete weather protection; requires engineered footings and may need building permit

| Quality Tier | Budget Builder | Luxury Builder |
|-------------|---------------|----------------|
| **Material** | Wood pergola (requires maintenance) | Aluminum louvered with motorized controls |
| **Weather Protection** | Partial shade only | Watertight when closed, built-in gutters |
| **Integration** | Standalone structure | Integrated lighting, ceiling fans, radiant heaters |
| **Automation** | Manual | App/remote controlled, rain sensor auto-close |
| **Warranty** | 1–3 years | 10–25 year structural warranty |

### Specialty Feature: Artificial Turf

**Key content points:**
- Base prep: 3–4" excavation, compacted crushed stone, weed barrier
- Quality: multi-tone nylon/polyethylene, 1.5"+ pile height, W-shaped blade for realism
- Infill: antimicrobial sand or rubber granules (silica sand for luxury; rubber for play areas)
- Edging: steel or composite for clean borders; aluminum edging lasts longest
- Heat consideration: turf near pool deck can get hot (140°F+ in Texas sun); specify cool-turf technology or TiO₂ coated fibers
- Drainage: minimum 30 inches/hour drainage rate; critical near pool to handle splash-out and rain
- Pet-friendly options: antimicrobial infill with enhanced drainage for households with pets

| Quality Tier | Budget Builder | Luxury Builder |
|-------------|---------------|----------------|
| **Blade Material** | Single-tone polypropylene (fades in 3–5 years) | Multi-tone nylon/polyethylene blend (10–15 year UV warranty) |
| **Pile Height** | 1" or less (looks flat/artificial) | 1.5–2.25" (realistic, soft underfoot) |
| **Infill** | Basic silica sand only | Antimicrobial coated sand + cool-turf technology |
| **Base Prep** | Minimal compaction | Full 4" compacted aggregate base with engineered drainage |
| **Edging** | Plastic landscape edging | Aluminum or composite with clean stone/paver border |
| **Seaming** | Visible seams | Invisible heat-seamed joints |

### Specialty Feature: Spa & Hot Tub Integration

**Key content points — why the spa deserves its own deep dive:**
- Spa construction is often the most complex element: separate heating, jets, controls, and often raised or spillover design
- Spillover spa (most luxury): spa water cascades into pool creating visual and auditory drama
- Structural: spa shell requires independent reinforcement with expansion joints at pool-spa connection
- Heating: dedicated heater (or heat pump) for rapid heat-up; luxury spas reach 104°F in 20–30 minutes
- Jets: hydrotherapy jet placement is both functional and aesthetic; luxury uses 8–16 jets with variable speed
- Controls: independent spa controls (heat, jets, lighting) separate from pool automation
- Seating: ergonomic benches at varying depths (18", 24", 36") with foot wells and armrests

| Quality Tier | Budget Builder | Luxury Builder |
|-------------|---------------|----------------|
| **Design** | Basic attached rectangle | Custom-shaped, raised spillover with stone veneer |
| **Shell** | Shared wall with pool (structural risk) | Independent shell with engineered expansion joints |
| **Jets** | 4–6 basic jets | 8–16 hydrotherapy jets with variable speed blower |
| **Heating** | Shared heater with pool (slow) | Dedicated spa heater for rapid heat-up |
| **Controls** | Same as pool | Independent spa panel + app control |
| **Interior** | Same finish as pool | Upgraded finish (full tile or premium pebble) |
| **Lighting** | Single light | Multi-zone LED with color scenes |

---

## 7. Risks, Trade-offs & Open Questions

### 7.1 Technical Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Content volume overwhelms development timeline | High | Medium | Phase content development: build framework first, populate content incrementally |
| Placeholder images don't convey luxury feel | Medium | High | Use curated Unsplash luxury pool photography; establish image criteria |
| Design drift from existing HumphreyPools site | Medium | High | Extract exact CSS custom properties and fonts into shared design token file |
| Astro version breaking changes | Low | Medium | Pin exact version; Astro has stable upgrade path |

### 7.2 Content Risks

| Risk | Mitigation |
|------|------------|
| Content feels generic / not specific to Humphrey | Include "What We Do Differently" callouts with Humphrey-specific details |
| Information overload for casual visitors | Progressive disclosure: summary first, expand for details |
| Comparison tables feel confrontational to other builders | Frame as "industry tiers" not "us vs. them" — educational tone |

### 7.3 Open Questions for Stakeholders

1. **Subdomain vs. subdirectory?** — Should this live at `guide.humphreyluxurypools.com` or `humphreyluxurypools.com/guide`?
2. **Contact form destination** — Does this site need its own contact form or link back to the main site?
3. **Photography timeline** — When will original project photos be available to replace placeholders?
4. **Content review process** — Who reviews the technical construction content for accuracy?
5. **Brand guidelines** — Is the existing site's design system (colors, fonts) the final brand standard?
6. **Analytics** — Should Google Analytics or a privacy-first alternative (Plausible/Fathom) be integrated?

### 7.4 Decisions to Make Upfront

- ✅ Technology stack (Astro 6) — Recommended above
- ✅ Hosting provider (GitHub Pages) — Recommended above
- ✅ Design system (inherit from existing site) — Recommended above
- ❓ Subdomain vs. path structure
- ❓ Analytics platform choice
- ❓ Content review/approval workflow

### 7.5 Decisions to Defer

- CMS integration (can add headless CMS later without refactoring)
- Blog/article section (Astro Content Collections make this trivial to add)
- Lead capture / CRM integration
- Multi-language support

---

## 8. Implementation Recommendations

### 8.1 Phased Implementation

#### Phase 1: Foundation (Week 1–2)
- Initialize Astro project with shared design tokens from HumphreyPools
- Build layout components (header, footer, nav, shared chrome)
- Create `PhaseCard`, `ComparisonTable`, `ExpandableSection` components
- Build overview/home page with interactive phase timeline
- Deploy to GitHub Pages with CI/CD

**Deliverable:** Working site with shared design, overview page, and infrastructure

#### Phase 2: Core Content (Week 3–5)
- Write and build Phase 1–5 deep-dive pages (Design through Gunite)
- Populate comparison tables with detailed quality tiers
- Add expandable deep-dive sections
- Curate and optimize placeholder luxury pool images
- Phase navigation (previous/next)

**Deliverable:** First half of construction phases with full progressive disclosure UX

#### Phase 3: Complete Content (Week 6–8)
- Write and build Phase 6–10 deep-dive pages (Tile through Startup)
- Build specialty feature pages (Acrylic Windows, Fire Features, Outdoor Kitchen, etc.)
- Cross-linking between related phases and features
- Mobile responsiveness polish

**Deliverable:** Full content site with all phases and features

#### Phase 4: Polish & Launch (Week 9–10)
- SEO optimization (meta tags, Open Graph, structured data)
- Performance audit (Lighthouse 95+ on all metrics)
- Accessibility audit (WCAG 2.1 AA)
- Analytics integration
- Final image swap with original photography (if available)
- Link integration with main HumphreyPools site

**Deliverable:** Production-ready site

### 8.2 Quick Wins (First Sprint)

1. **Scaffold Astro project** with existing design tokens — proves design compatibility immediately
2. **Build the overview/timeline page** — most impactful visual; demonstrates the progressive disclosure concept
3. **Create one complete phase page** (Phase 3: Steel & Rebar is ideal — most dramatic quality differences) — serves as the template for all others
4. **Deploy to GitHub Pages** — live URL for stakeholder review from day 1

### 8.3 Prototyping Recommendations

Before committing to full content production:

1. **Prototype the expand/collapse UX** — Test with 2–3 users: Do they discover the deep-dive content? Is the progressive disclosure intuitive?
2. **Prototype the comparison table design** — Ensure it reads well on mobile (tables are notoriously difficult on small screens; consider card-based layout on mobile)
3. **Validate image sourcing** — Confirm that Unsplash/Pexels luxury pool imagery meets the quality bar before investing in full content production

### 8.4 Content Production Approach

Each phase page requires:
- **Executive summary** (50–100 words)
- **Comparison table** (6–10 rows × 2–3 columns)
- **4–5 expandable deep-dive sections** (200–400 words each)
- **3–4 curated images**
- **"Questions to Ask Your Builder"** sidebar

**Total estimated content:** ~30,000–40,000 words across all phases and features. This is a significant content investment that should be planned alongside development, not after.

---

## Appendix: Design Token Reference (from existing site)

```css
/* Colors */
--color-primary: #0a1628;        /* Deep navy */
--color-primary-light: #142440;  /* Lighter navy */
--color-accent: #c9a84c;         /* Gold */
--color-accent-light: #e0c97f;   /* Light gold */
--color-white: #ffffff;
--color-offwhite: #f7f5f0;       /* Warm white background */
--color-cream: #ede8dd;           /* Cream */
--color-text: #2c2c2c;           /* Body text */
--color-text-light: #6b6b6b;     /* Secondary text */
--color-overlay: rgba(10, 22, 40, 0.65);

/* Typography */
--font-heading: 'Playfair Display', Georgia, serif;
--font-body: 'Raleway', 'Segoe UI', sans-serif;
--font-accent: 'Cormorant Garamond', Georgia, serif;

/* Layout */
--container-max: 1280px;
--section-padding: 100px 0;
--transition: 0.3s ease;
```

**Key design patterns from existing site:**
- `.section-overline` — Small caps gold text above section titles
- `.reveal` — IntersectionObserver-based scroll animations
- `.btn-primary` — Gold background with dark text
- `.btn-outline` — Transparent with gold border
- Dark navy sections alternate with warm white/cream sections
- Service cards use image backgrounds with gradient overlays

---

## Summary of Key Recommendations

| Decision | Recommendation | Confidence |
|----------|---------------|------------|
| **Framework** | Astro 6.1.3 | High ✅ |
| **Styling** | Vanilla CSS with existing design tokens | High ✅ |
| **Content Format** | Markdown/MDX Content Collections | High ✅ |
| **Hosting** | GitHub Pages (free) | High ✅ |
| **CI/CD** | GitHub Actions | High ✅ |
| **UX Pattern** | Progressive disclosure (overview → phase pages → expandable sections) | High ✅ |
| **Image Strategy** | Curated Unsplash/Pexels placeholders, replaced with originals later | Medium ✅ |
| **CMS** | Defer — add headless CMS only if content editors need it | Medium ✅ |
| **Analytics** | Defer decision to stakeholder input | Low ❓ |

---

## 9. Deep-Dive Analysis: Detailed Sub-Question Investigation

> **Purpose:** This section provides the in-depth, evidence-backed analysis for each of the 8 prioritized sub-questions. Each includes specific tools, version numbers, trade-offs, and concrete recommendations with reasoning.

---

### 9.1 Technology Stack Deep Dive: Astro 6.x as the Foundation

#### Key Findings (Verified April 2026)

| Dependency | Exact Version | Source |
|-----------|--------------|--------|
| **Astro** | 6.1.3 (latest stable) | GitHub releases, April 4 2026 |
| **Vite** | ^7.3.1 | Astro 6 package.json |
| **Zod** | ^4.3.6 | Astro 6 package.json |
| **Node.js** | >=22.12.0 | Astro 6 engines field |
| **TypeScript** | ^5.9.3 | Astro 6 package.json |
| **Shiki** | ^4.0.2 (syntax highlighting) | Astro 6 package.json |
| **Sharp** | ^0.34.x (image optimization) | Built-in image service |

**Community Health (April 2026):**
- GitHub Stars: **58,176** ⭐
- Forks: 3,309
- Open Issues: 289 (healthy signal — issues are triaged, not abandoned)
- Ownership: Acquired by Cloudflare (January 2026), full team joined, MIT license preserved
- Release cadence: Patch releases every 1–2 weeks

#### Astro 6.0 Key Features (What Changed from Astro 5)

1. **Built-in Fonts API** — Auto-downloads, caches, and self-hosts Google Fonts. Eliminates render-blocking `<link>` tags to `fonts.googleapis.com`. The existing site loads Playfair Display, Raleway, and Cormorant Garamond via external CDN — Astro 6 will self-host these automatically, improving LCP by 200–400ms.

2. **Content Security Policy (CSP) API** — Production-ready `context.csp` for automatic script/style hashing. Most-upvoted Astro feature request. Adds security headers with zero manual configuration.

3. **Live Content Collections** — Access externally-hosted content in real-time without rebuilds. Not needed for initial launch (all content is local Markdown), but enables future CMS integration without refactoring.

4. **Redesigned Dev Server** — Uses Vite 7's Environment API so dev runs the exact same runtime as production. Eliminates "works in dev, breaks in prod" bugs that plagued earlier versions.

5. **Content Layer with `glob()` loader** — Up to 5× faster Markdown builds, 25–50% less memory. Critical for a site with 15+ content-heavy pages.

6. **Server Islands (stabilized)** — Can add dynamic components (contact form, analytics) to otherwise static pages. New `security.serverIslandBodySizeLimit` config (defaults to 1 MB).

7. **Experimental Rust Compiler** — Future compilation speed improvements. Not production-ready yet but signals long-term performance investment.

#### Alternatives Evaluated and Rejected

| SSG | Build Speed | Frontend Perf | Component Model | Why Rejected |
|-----|------------|--------------|----------------|-------------|
| **Hugo** | ⚡ Fastest (1–5ms/page) | Standard static | Go templates only | No component reuse; Go template syntax steep learning curve; no islands architecture for interactive sections; can't share React/Vue/Svelte components |
| **Eleventy (11ty)** | Fast (~45ms/page) | Standard static | 12+ template languages | No built-in component model; progressive disclosure requires custom JS; no image optimization pipeline built-in; smaller ecosystem |
| **Next.js 15 (static export)** | Moderate | Ships React runtime (~40KB+) | React only | Overkill for content site; React runtime adds unnecessary JS weight; design system diverges from vanilla CSS approach; harder to merge with existing vanilla site |
| **Nuxt 4 (static)** | Moderate | Ships Vue runtime | Vue only | Same issues as Next.js — unnecessary framework runtime for a content site |

**Why Astro wins specifically for THIS project:**
1. **Output format matches existing site** — Astro outputs plain HTML/CSS/JS, identical to the vanilla HumphreyPools site. Hugo and Eleventy also do this, but lack the component model.
2. **Islands architecture** — Only the expand/collapse sections get JavaScript. Hugo/Eleventy would require custom JS solutions with no framework support.
3. **Content Collections** — Built for exactly this use case: typed Markdown files with Zod schema validation. Hugo has a similar concept but with Go template syntax.
4. **Shared design tokens** — Can import the existing `styles.css` custom properties directly. No Tailwind migration, no CSS-in-JS overhead.
5. **Cloudflare backing** — Financial stability and engineering investment ensure long-term viability. MIT license preserved; platform-agnostic deployment confirmed.

#### Concrete Recommendation

```bash
# Initialize project
npm create astro@latest humphrey-pool-guide -- --template minimal
cd humphrey-pool-guide
npm install @astrojs/mdx @astrojs/sitemap
```

**Lock versions in package.json:**
```json
{
  "dependencies": {
    "astro": "^6.1.3",
    "@astrojs/mdx": "^4.x",
    "@astrojs/sitemap": "^3.x"
  },
  "engines": {
    "node": ">=22.12.0"
  }
}
```

---

### 9.2 Construction Phases: Expert-Level Quality Differentiation

#### Key Findings

Research across luxury pool builder sites (Atlantis Luxury Pools, Morales Outdoor Living, Reinhard Pool, Backyard Vacation Oasis, Element Pools) and industry sources confirms the 10-phase model with these critical quality differentiators that website content must cover.

#### Equipment & Automation: Brand-Specific Comparison

The site should name specific products — this builds trust with informed buyers who recognize premium brands.

**Pool Automation Systems (control everything via app):**

| Feature | Pentair IntelliCenter | Jandy iAquaLink | Hayward OmniLogic |
|---------|----------------------|-----------------|-------------------|
| **Best With** | All-Pentair equipment | Jandy (+ some Pentair pumps) | All-Hayward equipment |
| **Mobile App** | Pentair Home | iAquaLink (iOS/Android/Web) | OmniLogic |
| **Smart Home** | Alexa, Apple HomeKit | Alexa, Google Home | Alexa, Google, IFTTT |
| **Chemistry Auto** | Via IntelliChem | Add-on options | Sense & Dispense (best) |
| **Lighting Scenes** | IntelliBrite colors | WaterColors | ColorLogic (best) |
| **Scalability** | Highly scalable | Needs expansion modules | 4–20 relays, multi-zone |
| **Firmware Updates** | Cloud OTA | Requires PCB swap | Cloud OTA |
| **Luxury Recommendation** | ⭐ Best for new all-Pentair builds | Best for retrofits | Best for smart home + chemistry |

**Site content recommendation:** Feature Pentair IntelliCenter as primary (most common in luxury new builds) but explain all three — educated buyers appreciate the comparison.

**Interior Finishes: PebbleTec Product Line Comparison:**

| Finish | Texture | Lifespan | Cost/sq.ft. | Look | Luxury Rating |
|--------|---------|----------|-------------|------|--------------|
| **PebbleTec (Original)** | Textured, larger pebbles | 20+ years | $8–$12 | Natural lagoon | ⭐⭐⭐ |
| **PebbleSheen** | Smoother, ground pebbles | 20+ years | $10–$14 | Refined shimmer | ⭐⭐⭐⭐ |
| **PebbleBrilliance** | Smooth, glass beads + pebbles | 20+ years | $14–$24+ | Sparkling, luminous | ⭐⭐⭐⭐⭐ |
| **White Plaster (Marcite)** | Very smooth | 7–10 years | $4–$6 | Basic, classic | ⭐ |
| **Quartz Aggregate** | Smooth-textured | 12–18 years | $7–$10 | Shimmering, vibrant | ⭐⭐⭐ |

**Site content recommendation:** Frame as a progression from budget (marcite) through luxury (PebbleBrilliance). Include the feel-underfoot comparison — PebbleTec can feel rough on sensitive feet, PebbleSheen is the sweet spot for comfort + durability, PebbleBrilliance is the showpiece choice.

#### Variable-Speed Pumps (the single biggest energy-cost differentiator):

| Model | Type | Energy Savings | Flow Rate | Noise | Smart Integration |
|-------|------|---------------|-----------|-------|-------------------|
| **Pentair IntelliFlo 3 VSF** | Variable Speed + Flow | Up to 90% vs single-speed | 160 GPM max | Ultra-quiet | Pentair Home app |
| **Jandy VS FloPro 2.7** | Variable Speed | Up to 80% | 148 GPM max | Quiet | iAquaLink |
| **Hayward Super Pump VS** | Variable Speed | Up to 80% | 115 GPM max | Moderate | OmniLogic |

**Key content point:** A $250K pool running a single-speed pump wastes $1,500–$2,500/year in electricity. VS pumps pay for themselves in 1–2 years. This is one of the most impactful "budget builder cuts corners" examples.

---

### 9.3 Progressive Disclosure UX: Component Architecture

#### Key Findings

The progressive disclosure pattern must work at three levels:
1. **Home page** — Visual timeline showing all 10 phases + 6 specialty features as cards
2. **Phase page** — Executive summary → comparison table → expandable deep-dive sections
3. **Expandable sections** — Click to reveal 200–400 word deep dives on specific topics

#### CSS-only `<details>`/`<summary>` vs JavaScript Accordion

| Approach | Pros | Cons | Recommendation |
|----------|------|------|---------------|
| **`<details>`/`<summary>` (HTML native)** | Zero JS; accessible by default; keyboard-navigable; works with SSG; no hydration needed | Limited animation control; can't "close others when one opens"; basic styling | ⭐ **Use this** for deep-dive sections |
| **JavaScript Accordion (Astro Island)** | Full animation control; "only one open" behavior; custom transitions | Requires client JS; needs ARIA implementation; adds bundle size | Use only if design requires coordinated open/close |
| **CSS-only with `:has()` selector** | No JS; modern CSS animation | Limited browser support history; complex selectors | Not recommended yet |

**Concrete recommendation:** Use native `<details>`/`<summary>` elements styled with CSS. This is zero-JS, fully accessible, and SEO-friendly (Google indexes content inside closed `<details>` elements). Astro's zero-JS-by-default philosophy aligns perfectly.

```astro
<!-- ExpandableSection.astro -->
---
interface Props {
  title: string;
  icon?: string;
}
const { title, icon = '▸' } = Astro.props;
---
<details class="deep-dive-section">
  <summary class="deep-dive-trigger">
    <span class="deep-dive-icon">{icon}</span>
    <span class="deep-dive-title">{title}</span>
  </summary>
  <div class="deep-dive-content">
    <slot />
  </div>
</details>

<style>
  .deep-dive-section {
    border-bottom: 1px solid var(--color-cream);
    padding: 0;
  }
  .deep-dive-trigger {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 20px 0;
    cursor: pointer;
    font-family: var(--font-heading);
    font-size: 1.25rem;
    color: var(--color-primary);
    list-style: none; /* Remove default marker */
  }
  .deep-dive-trigger::-webkit-details-marker { display: none; }
  .deep-dive-icon {
    transition: transform 0.3s ease;
    color: var(--color-accent);
  }
  details[open] .deep-dive-icon {
    transform: rotate(90deg);
  }
  .deep-dive-content {
    padding: 0 0 24px 32px;
    color: var(--color-text);
    line-height: 1.8;
  }
</style>
```

#### Mobile-Responsive Comparison Tables

Comparison tables are the core content element but tables notoriously break on mobile. Research shows three viable approaches:

| Approach | Mobile Experience | Implementation Complexity | Recommendation |
|----------|------------------|--------------------------|---------------|
| **Horizontal scroll with shadow hints** | Swipe left/right; shadow on edges indicates more content | Low | ⭐ **Use this** — simplest, preserves table structure |
| **Card stack (table → cards on mobile)** | Each row becomes a card with label-value pairs | Medium | Good alternative for simpler tables |
| **Responsive header pinning** | First column stays fixed, others scroll | Medium | Best for wide tables (5+ columns) |

**Implementation:** Wrap tables in a scrollable container with CSS gradient shadows:

```css
.comparison-table-wrapper {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  background:
    linear-gradient(to right, var(--color-white) 30%, transparent),
    linear-gradient(to left, var(--color-white) 30%, transparent),
    linear-gradient(to right, rgba(0,0,0,0.1), transparent),
    linear-gradient(to left, rgba(0,0,0,0.1), transparent);
  background-position: left, right, left, right;
  background-size: 40px 100%, 40px 100%, 14px 100%, 14px 100%;
  background-repeat: no-repeat;
  background-attachment: local, local, scroll, scroll;
}
```

#### Accessibility Requirements (WCAG 2.1 AA)

- `<details>`/`<summary>` provides native keyboard support (Enter/Space to toggle)
- Comparison tables need `<th scope="col">` and `<th scope="row">` for screen readers
- Color contrast: Gold (#c9a84c) on white (#fff) **fails** AA contrast — use on dark backgrounds only, or darken to #a08930 for light backgrounds
- Focus indicators: Ensure `:focus-visible` styles on all interactive elements
- Skip navigation: Add a "Skip to content" link for keyboard users

---

### 9.4 North Texas Regional Authority: DFW Soil & Permit Deep Dive

#### Key Findings (Verified via Geotechnical Sources)

**DFW Expansive Clay Soil — The #1 Content Differentiator:**

| Metric | DFW Reality | Content Implication |
|--------|------------|-------------------|
| **Dominant Soil Type** | Eagle Ford Shale, Austin Chalk Group clay formations | Name these specifically — builds geological authority |
| **Plasticity Index (PI)** | Commonly **30–60+** across North Dallas suburbs | PI > 30 = "highly expansive" — requires engineered pool design per 2021 IRC |
| **Potential Vertical Rise (PVR)** | 2–6 inches of seasonal ground movement | This is the key stat to communicate: your pool shell must handle 6" of differential movement |
| **Active Zone Depth** | 7–15 feet below surface | Soil stabilization injections must reach this depth to be effective |
| **Geotechnical Report Cost** | $500–$1,500 per site | Emphasize: this $1K investment prevents $30K–$50K+ in structural failure |

**Soil Stabilization Methods (specific to DFW pool construction):**

| Method | How It Works | Cost Range | When Required |
|--------|-------------|-----------|---------------|
| **SWIMSOIL chemical injection** | Proprietary chemical injected 7–10' deep, permanently alters expansive clay | $3,000–$8,000 | PI > 30, moderate PVR |
| **EcSS 3000 / ProChem injection** | Similar chemical stabilization, different chemistry | $3,000–$8,000 | PI > 30, moderate PVR |
| **Helical piers** | Screw-like steel piles driven to stable strata below active clay | $8,000–$20,000+ | Extreme PI/PVR, raised features, acrylic windows |
| **Drilled piers** | Concrete piers drilled to bedrock or stable soil | $10,000–$25,000+ | Extreme conditions, large structures |
| **Post-tensioned slab** | Steel cables in concrete shell tensioned after curing | Included in engineering | Standard for DFW gunite pools |
| **Lime treatment** | Quicklime mixed into excavated soil | $2,000–$5,000 | Moderate expansion, deck areas |

**Site content angle:** Budget builders skip soil stabilization ($3K–$8K savings) — this causes shell cracks, deck separation, plumbing leaks. The repair cost is $30K–$50K+. This is the most powerful "quality difference" example on the entire site.

#### Municipal Permit Requirements (North Dallas)

| Municipality | Geotech Required? | Stamped Engineer Plans? | Key Differences |
|-------------|-------------------|------------------------|----------------|
| **Frisco** | Yes — mandatory for pool permits | Yes — PE-stamped pool/foundation plan referencing geotech | Strict inspection schedule; will not issue CO without engineer's as-built letter |
| **Prosper** | Yes | Yes | Growing rapidly; permit timelines can stretch 4–6 weeks |
| **McKinney** | Yes | Yes | Requires soil boring analysis; multiple inspections |
| **Celina** | Yes | Yes | Newer city infrastructure; HOA ARC approvals common |
| **Allen** | Yes | Yes | Established inspection process; moderate timelines |
| **Plano** | Yes | Yes | Most established process; efficient permit office |

**Content opportunity:** "Your builder should handle 100% of permitting. If they ask you to pull the permit yourself, that's a red flag." This positions Humphrey Pools as the builder who handles the complexity.

#### 2021 IRC Code Requirements (applicable in all DFW municipalities)

- Foundations (including pools) must be designed to accommodate measured soil movement
- Geotechnical report must include: borehole logs (minimum 2, 15–20' deep), PI, soil strata, groundwater levels, PVR
- PE-stamped structural plans required referencing the geotech report
- As-built documentation required before final approval (engineer's letter certifying construction matches plans)

---

### 9.5 Specialty Features: Product-Level Detail for Content Authority

#### Acrylic Pool Windows — Deep Technical Specifications

**Primary Manufacturer:** Reynolds Polymer Technology (RPT) — Makers of R-Cast™ acrylic, the industry standard for luxury pool, aquarium, and architectural glazing.

| Specification | Details |
|--------------|---------|
| **Material** | R-Cast™ cell-cast acrylic (not extruded) |
| **Optical Clarity** | 92% light transmittance |
| **Impact Strength** | Up to 17× stronger than glass, 4× stronger than concrete |
| **Panel Thickness** | 40mm–300mm+ (1.5"–12"+), calculated per FEA analysis |
| **Max Single Panel Size** | 3m × 9m (10' × 30') in one piece |
| **Engineering** | Finite Element Analysis (FEA) for deflection, load, and safety factor |
| **Certification** | PE-stamped engineering drawings included |
| **Lead Time** | 6–12 weeks for custom panels |
| **Installation** | Dedicated acrylic glazing specialists (not standard pool crew) |
| **Maintenance** | Scratch-repairable without draining; gentle cleaning with non-abrasive products |
| **Seal Replacement** | Every 5–10 years |
| **Warranty** | 20–30 year panel warranty from manufacturer |

**Cost Ranges (verified from industry sources):**

| Window Size | Approximate Cost (Supply + Install) |
|------------|-------------------------------------|
| Small (2' × 3') | $15,000–$25,000 |
| Medium (3' × 5') | $25,000–$50,000 |
| Large (4' × 8') | $50,000–$80,000 |
| Panoramic (8' × 12'+) | $80,000–$150,000+ |

**Wall construction for acrylic windows:**
- Standard pool wall: 8"–10" thick
- Acrylic window wall: **15"–18" thick** minimum
- Requires U-channel construction larger than the panel
- UV-resistant, non-corrosive waterproof sealant system
- Structural engineering must be integrated from **day one** of design — cannot be added after the fact

**Content angle:** "If your builder says they can add a window later, they don't understand acrylic pool windows. The structural engineering starts at the design phase."

#### Outdoor Kitchen: Premium Brand Comparison

**Grill Brands for $250K+ Projects:**

| Brand | Main Burner BTU (42") | Stainless Grade | Signature Feature | Price Range (42" built-in) |
|-------|----------------------|-----------------|-------------------|--------------------------|
| **Lynx** | 75,000+ (ceramic) | 304 SS | Trident infrared sear burner, hot surface ignition | $6,000–$9,000 |
| **Alfresco** | 82,500+ (U-shaped SS) | 304 SS | AccuFire sear zone (1,500°F), internal rotisserie motor | $5,500–$8,500 |
| **Twin Eagles** | 75,000+ (U-shaped SS) | 304 SS | 3/8" hexagonal grates for superior sear marks | $5,500–$8,000 |
| **Hestan** | 75,000+ (Trellis) | 304 SS (316 marine option) | Custom color finishes, marine-grade upgrade | $7,000–$11,000 |
| **Kalamazoo** | 100,000+ (hybrid) | Heavy-gauge SS | Charcoal + gas hybrid, hand-built in USA | $15,000–$25,000 |

**Cabinetry Brands:**
- **Danver** — 304 stainless steel cabinetry, powder-coated finish options, lifetime warranty
- **NatureKast** — Marine-grade polymer (weatherproof), realistic wood-grain textures, 0% moisture absorption
- **Challenger Designs** — Modular stainless frames, quick-assemble island systems

**Countertop Recommendations for DFW Climate:**

| Material | Heat Resistance | Weather Durability | Maintenance | Luxury Rating |
|----------|----------------|-------------------|-------------|--------------|
| **Quartzite** | Excellent | Excellent (natural stone) | Seal annually | ⭐⭐⭐⭐⭐ |
| **Soapstone** | Excellent (won't crack from heat) | Excellent | Oil periodically | ⭐⭐⭐⭐⭐ |
| **Premium Granite (3cm)** | Good | Good | Seal annually | ⭐⭐⭐⭐ |
| **Porcelain (sintered stone)** | Excellent | Excellent | Zero maintenance | ⭐⭐⭐⭐ |
| **Marble** | Poor (etches/stains) | Poor (porous) | High maintenance | ❌ Avoid outdoors |
| **Engineered Quartz** | Poor (UV damage, resin melts) | Poor (UV discolors) | N/A | ❌ Avoid outdoors |

#### Patio Covers: StruXure vs Azenco Head-to-Head

| Feature | StruXure Pivot 6 | Azenco R-BLADE |
|---------|------------------|----------------|
| **Material** | Extruded powder-coated aluminum | Dual-wall powder-coated aluminum |
| **Louver Rotation** | 170° | 90° (gapless design) |
| **Watertight** | Yes (when closed) | Yes (concealed integrated gutters) |
| **Wind Rating** | Up to hurricane-rated (model dependent) | Miami-Dade approved (190 mph) |
| **Snow Load** | Model dependent | Up to 100 lb/sq.ft. |
| **Smart Controls** | Remote, app, weather sensors | Remote, app, wall switch, rain sensor |
| **Integrated Features** | LED lighting, heaters, fans, screens, audio | LED lighting, heaters, fans, misting, privacy screens, audio |
| **Cost per sq.ft. (installed)** | $150–$200 | $65–$180 (varies by configuration) |
| **Typical 12'×20' installed** | $36,000–$48,000 | $25,000–$50,000+ |
| **Colors** | 6 standard + custom | Multiple standard + custom |
| **Warranty** | Limited lifetime (structural) | 10–25 year structural |

**Recommendation for site content:** Present both brands as premium options. StruXure has better brand recognition in DFW residential. Azenco has superior wind ratings (relevant for North Texas storms).

#### Artificial Turf: Technology Comparison for Pool Areas

| Feature | SYNLawn | ForeverLawn |
|---------|---------|-------------|
| **Heat Reduction Tech** | HeatBlock™, COOLplus®, DualChill™ (TiO₂ + IR-reflective pigments) — up to 20% cooler | Advanced yarn formulas + cooling infill options |
| **Drainage Rate** | 30–90+ inches/hour/sq.yd. | 100% edge-to-edge drainage (K9Grass backing system) |
| **UV Resistance** | Plant-based fibers (sugar cane), PFAS-free | UV-stabilized nylon/polyethylene |
| **Fire Rating** | ASTM Class A fire rated | Standard fire resistance |
| **Cooling Infill** | TCool® sand infill (reduces surface temp by up to 50°F vs standard) | Compatible with TCool® |
| **Pile Height** | 1.5"–2.25" (Premium/Select LX lines) | 1.5"–2.25" (Fusion Elite line) |
| **Best For Pool Areas** | ⭐ Best documented heat reduction; ideal for Texas | Best drainage system; ideal for pet households |

**Critical Texas consideration:** Standard turf can reach **140°F+** in direct Texas sun. TiO₂-coated fibers + TCool® infill bring this down to ~100–110°F — still warm but safe for bare feet. Content should address this head-on: "No turf is 'cool' in July. But cool-technology turf is the difference between uncomfortable and dangerous."

---

### 9.6 Luxury Image Strategy: Technical Implementation

#### Key Findings

Images are the #1 factor in conveying luxury. Research shows the current approach needs precision:

#### Astro Built-in Image Optimization Pipeline

```astro
---
// Phase hero image example
import { Image } from 'astro:assets';
import heroImage from '../assets/phases/excavation-hero.jpg';
---
<Image
  src={heroImage}
  alt="Precision excavation of a luxury freeform pool in North Texas clay soil"
  width={1920}
  height={1080}
  format="webp"
  quality={80}
  loading="eager"  /* Hero images: eager. All others: lazy (default) */
/>
```

**Format Strategy:**

| Format | Use Case | Size Reduction | Browser Support |
|--------|----------|---------------|----------------|
| **WebP** | Default output for all images | 25–35% smaller than JPEG | 97%+ browsers |
| **AVIF** | Hero images (via `<Picture>` component) | 50%+ smaller than JPEG | 93%+ browsers |
| **JPEG** | Fallback for older browsers | Baseline | Universal |

**Use `<Picture>` for hero images (serves AVIF → WebP → JPEG):**
```astro
---
import { Picture } from 'astro:assets';
import hero from '../assets/hero.jpg';
---
<Picture
  src={hero}
  formats={['avif', 'webp']}
  alt="Infinity edge pool overlooking a North Texas sunset"
  width={1920}
  height={1080}
  loading="eager"
/>
```

#### Image Sizing Standards

| Image Type | Dimensions | File Size Target | Loading Strategy |
|-----------|-----------|-----------------|-----------------|
| **Phase Hero** | 1920×1080 | < 200KB (WebP) | `loading="eager"` |
| **Gallery Thumbnails** | 600×400 | < 50KB (WebP) | `loading="lazy"` |
| **Comparison Illustrations** | 800×600 | < 80KB (WebP) | `loading="lazy"` |
| **Inline Content Images** | 1200×800 | < 120KB (WebP) | `loading="lazy"` |
| **OG/Social Sharing** | 1200×630 | < 100KB (JPEG) | Build-time generation |

#### Placeholder Image Strategy

**Phase 1 (Launch):** Curated stock photography from royalty-free sources

| Source | License | Quality for Luxury Pool Content | API/Bulk Access |
|--------|---------|-------------------------------|----------------|
| **Unsplash** | Unsplash License (free, commercial use, no attribution required) | Good — 50+ high-quality luxury pool images | API: 50 req/hour (free) |
| **Pexels** | Pexels License (free, commercial use, no attribution required) | Good — strong outdoor living content | API: 200 req/hour (free) |
| **Adobe Stock** | Paid license ($30–$80/image or subscription) | Excellent — largest luxury pool library | Direct download |

**Recommendation:** Start with Unsplash/Pexels for speed and cost. Budget $500–$1,000 for 15–20 Adobe Stock images for hero shots where free sources don't have sufficient luxury quality. All images should be:
- Shot at eye level or slightly above (architectural perspective)
- Showing water features active (cascading water, lit at dusk)
- Dusk/golden hour lighting preferred (conveys luxury)
- Include lifestyle elements (outdoor furniture, fire features lit, landscaping)
- Minimum 3000×2000 source resolution (downsampled by Astro)

**Phase 2 (Post-Launch):** Replace with original Humphrey Pools project photography
- Professional photographer for 2–3 completed projects
- Drone aerial shots (dramatic for large projects)
- Dusk/twilight shoots (most impactful for luxury)
- Before/during/after sequences for construction phases

#### Blur-Up Placeholder Strategy

Store images in `/src/assets/` (NOT `/public/`) so Astro processes them at build time. Astro's Sharp integration generates optimized output automatically. For perceived performance:

1. **Dominant color placeholder** — Extract dominant color at build time, show colored rectangle while image loads
2. **LQIP (Low Quality Image Placeholder)** — 20px wide version of the image, CSS blur-filtered, transitions to full image on load
3. **For this project:** Use `loading="eager"` on hero images (above the fold) and `loading="lazy"` on everything else. The Astro `<Image>` component handles width/height to prevent CLS (Cumulative Layout Shift).

---

### 9.7 Future Integration with Main HumphreyPools Site

#### Key Finding: Subdirectory Wins Over Subdomain for SEO

**Google's 2025–2026 guidance:** While Google claims both are treated equally, real-world data consistently shows subdirectories outperform subdomains:

| Strategy | SEO Impact | Technical Complexity | Recommendation |
|----------|-----------|---------------------|---------------|
| **Subdirectory** (`humphreyluxurypools.com/guide/`) | ⭐ Inherits all root domain authority; link equity flows naturally; 8–25% traffic increase documented in migrations | Moderate — requires reverse proxy or build integration | ⭐ **Recommended long-term** |
| **Subdomain** (`guide.humphreyluxurypools.com`) | Authority is split; must build backlinks separately; Google treats as separate property | Low — separate deployment, separate DNS | Acceptable for Phase 1 launch |

**Industry evidence:**
- Monster.com saw **100%+ visibility increase** migrating localized content from subdomains to subdirectories
- Multiple case studies show 20–200% traffic jumps from subdomain→subdirectory migrations
- Google's Helpful Content Update (2023–2025) rewards tightly interlinked site structures — subdomains dilute topical signals

#### Phased Integration Strategy

**Phase 1 (Launch):** Deploy as subdomain `guide.humphreyluxurypools.com`
- Fastest path to launch
- Separate GitHub Pages deployment
- Shared visual design (CSS custom properties, fonts, layout patterns)
- Cross-linking: Main site hero/nav links to guide; guide links back to main site CTA

**Phase 2 (Integration):** Migrate to subdirectory `humphreyluxurypools.com/guide/`
- Option A: Combine both sites into one Astro project (Astro handles the existing vanilla pages as `src/pages/` and guide content as `src/pages/guide/`)
- Option B: Use Cloudflare Workers or nginx reverse proxy to serve the guide site at `/guide/` path
- Set up 301 redirects from subdomain to subdirectory
- Update all internal links and sitemap

#### Design Token Sharing Strategy

The existing site's CSS custom properties become the shared design system:

```
humphrey-pools/
├── shared/
│   ├── tokens.css          ← Extracted from existing styles.css :root block
│   ├── typography.css      ← .section-overline, .section-title, .section-subtitle
│   ├── buttons.css         ← .btn, .btn-primary, .btn-outline
│   └── layout.css          ← .container, .reveal
├── main-site/              ← Existing vanilla HTML site
│   └── css/styles.css      ← @import '../shared/tokens.css' + site-specific styles
└── guide-site/             ← New Astro project
    └── src/styles/
        ├── global.css      ← @import '../../shared/tokens.css'
        └── guide.css       ← Guide-specific component styles
```

**Existing design tokens to extract (verified from repo):**

```css
/* Verified from azurenerd/HumphreyPools css/styles.css */
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
  --color-overlay: rgba(10, 22, 40, 0.65);

  /* Typography */
  --font-heading: 'Playfair Display', Georgia, serif;
  --font-body: 'Raleway', 'Segoe UI', sans-serif;
  --font-accent: 'Cormorant Garamond', Georgia, serif;

  /* Layout */
  --container-max: 1280px;
  --section-padding: 100px 0;
  --transition: 0.3s ease;
}
```

**Font loading in Astro 6 (replaces external Google Fonts CDN):**

```js
// astro.config.mjs
import { defineConfig } from 'astro/config';

export default defineConfig({
  fonts: {
    families: [
      {
        name: 'Playfair Display',
        provider: 'google',
        weights: [400, 500, 600],
        styles: ['normal', 'italic'],
      },
      {
        name: 'Raleway',
        provider: 'google',
        weights: [300, 400, 500, 600, 700],
      },
      {
        name: 'Cormorant Garamond',
        provider: 'google',
        weights: [400, 500],
        styles: ['normal', 'italic'],
      },
    ],
  },
});
```

This eliminates the render-blocking `<link>` tags to `fonts.googleapis.com` in the existing site's `<head>`, self-hosting the fonts for better LCP scores.

#### Shared Navigation Pattern

The guide site header should visually match the existing site but add a "← Back to Main Site" link and guide-specific navigation:

```
EXISTING SITE NAV:  Our Story | Showcase | Process | Services | Portfolio | [Start Your Project]
GUIDE SITE NAV:     ← HumphreyPools.com | Overview | Phases | Features | [Start Your Project]
```

Both share the same logo SVG, font stack, and gold-on-navy color scheme.

---

### 9.8 Content Pipeline: Producing 30K–40K Words of Expert Content

#### Key Findings

The site requires approximately **30,000–40,000 words** across 10 phases + 6 specialty features = **16 major content pages**. This is a substantial editorial investment equivalent to a short book.

#### Content Per Page Breakdown

| Section | Count | Words/Page | Total Words |
|---------|-------|-----------|-------------|
| Phase pages (1–10) | 10 | 2,500–3,500 | 25,000–35,000 |
| Specialty feature pages | 6 | 1,500–2,500 | 9,000–15,000 |
| Home/overview page | 1 | 1,000–1,500 | 1,000–1,500 |
| **Total** | **17** | — | **35,000–51,500** |

#### Each Page Content Template (Markdown/MDX)

```markdown
---
# Frontmatter (validated by Zod schema)
title: "Phase 3: Steel & Rebar"
phaseNumber: 3
duration: "1–2 weeks"
heroImage: "./images/steel-rebar-hero.jpg"
executiveSummary: "The skeleton of your pool. This is where the most corners are cut..."
comparisonRows:
  - aspect: "Rebar Gauge"
    budget: "#3 (3/8\") minimum"
    luxury: "#4 (1/2\") standard, #5 for walls"
  - aspect: "Grid Spacing"
    budget: "12\" on-center"
    luxury: "6\"–8\" on-center"
---

import ComparisonTable from '../../components/ComparisonTable.astro';
import ExpandableSection from '../../components/ExpandableSection.astro';
import QualityCallout from '../../components/QualityCallout.astro';

<QualityCallout items={[
  "Quality rebar costs $3K–$5K more but prevents $30K+ in structural failure",
  "Luxury builders use #4–#5 rebar at 6\"–8\" spacing vs #3 at 12\"",
  "Independent structural inspection before gunite is non-negotiable",
]} />

<ComparisonTable rows={frontmatter.comparisonRows} />

<ExpandableSection title="What Happens In This Phase">
  Content here — 200–400 words of expert detail...
</ExpandableSection>

<ExpandableSection title="Materials & Specifications">
  Content here...
</ExpandableSection>

<ExpandableSection title="Common Pitfalls & Red Flags">
  Content here...
</ExpandableSection>

<ExpandableSection title="Questions to Ask Your Builder">
  Content here...
</ExpandableSection>

<ExpandableSection title="What We Do Differently">
  Content here — Humphrey-specific quality differentiators...
</ExpandableSection>
```

#### SEO Structured Data (JSON-LD)

Each phase page should include HowTo and FAQ structured data for Google featured snippets:

```json
{
  "@context": "https://schema.org",
  "@type": "HowTo",
  "name": "Pool Construction Phase 3: Steel & Rebar Installation",
  "description": "Expert guide to steel and rebar installation in luxury pool construction...",
  "step": [
    {
      "@type": "HowToStep",
      "name": "Rebar Layout",
      "text": "Position #4 rebar at 6-8 inch on-center grid spacing per engineer specifications..."
    }
  ]
}
```

**FAQ schema** for the expandable sections (Google indexes `<details>` content and can display as FAQ rich results):

```json
{
  "@context": "https://schema.org",
  "@type": "FAQPage",
  "mainEntity": [
    {
      "@type": "Question",
      "name": "What questions should I ask my pool builder about steel and rebar?",
      "acceptedAnswer": {
        "@type": "Answer",
        "text": "Ask about rebar gauge (#3 vs #4), grid spacing, overlap standards..."
      }
    }
  ]
}
```

#### Content Quality Assurance Pipeline

| Tool | Purpose | Integration Point |
|------|---------|------------------|
| **markdownlint** (`markdownlint-cli2`) | Markdown syntax consistency | Pre-commit hook + CI |
| **Hemingway App / Readable.com** | Readability scoring (target: Grade 8–10 reading level) | Manual review during writing |
| **Astro Check** (`astro check`) | TypeScript + content schema validation | CI build step |
| **Lighthouse CI** | Performance, accessibility, SEO auditing | CI post-build |
| **Schema.org Validator** | Verify structured data markup | Manual check per page |

#### Editorial Workflow (Without CMS)

1. **Write** content in Markdown/MDX files in `src/content/phases/` and `src/content/features/`
2. **Validate** with `astro check` (ensures Zod schema compliance — missing frontmatter fields will error)
3. **Preview** with `npm run dev` (Astro dev server with hot reload)
4. **Review** via Pull Request (GitHub PR review for content accuracy)
5. **Deploy** automatically on merge to `main` (GitHub Actions → GitHub Pages)

**Content production estimate:** At 500–800 words/hour for expert technical content (with research), expect **50–80 hours of writing** for the full site. This is the primary bottleneck — recommend starting content writing in parallel with development, not after.

---

## 10. Revised Implementation Timeline (with Deep-Dive Findings)

Based on the deep analysis above, the revised timeline accounts for content production as the critical path:

| Week | Development | Content Writing | Deliverable |
|------|------------|----------------|-------------|
| **1–2** | Astro project setup, design token extraction, layout components, CI/CD | Phase 1–3 content drafts | Working site skeleton on GitHub Pages |
| **3–4** | PhaseCard, ComparisonTable, ExpandableSection components; home page timeline | Phase 4–7 content drafts | Functional home page + Phase 1 complete page |
| **5–6** | Phase page template, image optimization pipeline, navigation | Phase 8–10 content drafts | First 5 phase pages live |
| **7–8** | Specialty feature pages, cross-linking, mobile polish | Specialty feature content (6 pages) | All 10 phases + 3 features live |
| **9** | SEO (JSON-LD, meta tags, OG images), accessibility audit | Remaining specialty features + revisions | All 16 pages complete |
| **10** | Performance audit (Lighthouse 95+), analytics integration, stakeholder review | Final content review, image replacements | **Production launch** |

**Critical path:** Content writing (50–80 hours) is the bottleneck, not development. Start writing Phase 1–3 immediately while scaffolding the Astro project.

---

## Appendix B: Reference Sources

| Topic | Key Sources |
|-------|------------|
| Astro 6.0 Release | astro.build/blog/astro-6, GitHub withastro/astro releases |
| Astro 6 Migration | byteiota.com/astro-6-migration-guide |
| Cloudflare Acquisition | blog.cloudflare.com/astro-joins-cloudflare |
| SSG Comparison | thesoftwarescout.com/best-static-site-generators-2026 |
| DFW Soil Engineering | ultimus.engineering/swimming-pool-engineering, swimsoil.com |
| DFW Soil Stabilization | dfwpoolandpatio.com, prochemtx.com, texasoutdooroasis.com |
| Helical Piers (DFW) | hubbell.com/chancefoundationsolutions (Chance Helical Pile case study) |
| Reynolds Polymer (Acrylic) | reynoldspolymer.com/capabilities/design-engineering |
| Pool Automation | poolburg.com/pool-automation-systems, superiorpoolservice.com |
| PebbleTec Finishes | pebbletec.com, troublefreepool.com |
| StruXure Pergolas | struxure.com, homeguide.com/costs/struxure-pergola-cost |
| Azenco R-BLADE | azenco-outdoor.com/r-blade |
| Outdoor Grills | thebbqdepot.com, bestofbackyard.com |
| SYNLawn Turf | synlawn.com, texasartificiallawns.com/heatblock-technology |
| ForeverLawn Turf | foreverlawntexas.com/specifications |
| Subdomain vs Subdirectory | backlinko.com/subdirectory-vs-subdomain, semrush.com |
| Astro Image Optimization | docs.astro.build/en/guides/images |
| Luxury Pool Process | backyardvacationoasis.com, atlantisluxurypools.com, moralesoutdoorliving.com |
