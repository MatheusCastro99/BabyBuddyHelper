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
- 5.3 AI groundwork (#95): shipped. ADRs for the off-device data path, the user-supplied key and a root-level `Remote/` folder; CommunityToolkit.Maui and the `Anthropic` SDK added; a "last updated" date each for a baby's weight and height. It replaces "Companion customization", which was dropped.
- 5.4 Chat page and Cub settings (#100): "Talk to Cub" on the companion card, the chat page, and a gear popup holding the user's API key.
- 5.5 Companion AI agent (#96): Cub answers basic questions through Claude, under clear guidelines (not a doctor or therapist).
- 5.6 Companion personality (#97): conversational style (supportive, neutral, direct) and verbosity.

5.1, 5.2 and 5.3 are done. Next: 5.4 (#100).

Open issues, all in Phase 6 (release preparation):

- Mobile Focus: #71 CI builds for the mobile targets, #72 mobile UI/UX polish pass.
- Database Preparation for Release: #76 EF Core migrations.
- Deployment and Distribution: #79 re-verify the vaccine catalog after the First Circuit ruling, before the first release.

After Phase 6: Phase 7 (accounts, cloud sync, backup), Phase 8 (advanced AI features).

See ROADMAP.md for the full Phase 5 breakdown.

---
