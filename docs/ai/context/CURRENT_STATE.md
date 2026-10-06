# Current State:

Status:

- Functional Prototype
- Active Development

---

# Current Development Focus:

## Phase 5: Dual UI Mode, Companion Comes to Life

Epic: #92. Sub-phases, in order (one issue and one PR each):

- 5.1 Dual UI mode (#93): shipped. A boy / girl palette in place of a regular light / dark mode, with a switch on the tab row (Windows; mobile placement is in #72). The choice is saved in Preferences. The app stays pinned to the light theme, with two light palettes.
- 5.2 Companion animations (#94): shipped. Cub is a Lottie animation (wave, clap, dance, nod) played through a shared `CubView`; it waves on the companion card, dances on "Tell me something", and claps or nods in toasts.
- 5.3 Companion customization (#95): basic customization of the companion's appearance and behavior.
- 5.4 Companion AI agent (#96): personalized tips and basic questions, under clear guidelines (not a doctor or therapist).
- 5.5 Companion personality (#97): light adjustment (e.g., more encouraging, more playful, more serious).

5.1 and 5.2 are done. Next: 5.3 (#95).

Open issues, all in Phase 6 (release preparation):

- Mobile Focus: #71 CI builds for the mobile targets, #72 mobile UI/UX polish pass.
- Database Preparation for Release: #76 EF Core migrations.
- Deployment and Distribution: #79 re-verify the vaccine catalog after the First Circuit ruling, before the first release.

After Phase 6: Phase 7 (accounts, cloud sync, backup), Phase 8 (advanced AI features).

See ROADMAP.md for the full Phase 5 breakdown.

---
