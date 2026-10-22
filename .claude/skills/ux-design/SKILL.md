---
name: ux-design
description: UX/UI lens for BabyBuddyHelper. Checks screens, XAML changes, flows and copy against the Parenting Companion identity, the UI guidelines, the shared style tokens and basic accessibility. Use when designing or reviewing a page or control, touching XAML styling, writing user-facing text (toasts, labels, empty states), or planning a redesign.
---

# UX Design

This skill adds a UX lens on top of the session context. It produces a **recommendation**. The project owner decides.

## 1. Load context

- `Harness_Config/Context/UI_GUIDELINES.md` is **the authority**: identity, principles, palette, typography, screen purposes. Don't restate it; apply it.
- `Resources/Styles/Palettes/BoyPalette.xaml` and `GirlPalette.xaml` hold the palette colors (`BabyBuddy*`, `Primary*`, ...) with identical keys; pages read them via `DynamicResource`, and the user switches palette at runtime. `Resources/Styles/Colors.xaml` holds the shared neutrals. `Resources/Styles/Styles.xaml` holds the named styles (`PrimaryAction`, `SecondaryAction`, `PageTitle`, `SectionTitle`, `BodyText`, `CaptionText`, `Headline`, `SubHeadline`), consumed via `StaticResource`. Both palettes are light; there is no dark mode, and `AppThemeBinding` is not used.
- The XAML and code-behind of the page being changed, in `UI/Pages/` or `UI/Controls/`.

You can't see the running app. Base findings on the XAML and say what the owner should look at on screen.

## 2. Check

**Identity.** Is it warm, calm, encouraging and low-stress? Avoid anything corporate, clinical, dense or guilt-inducing (UI_GUIDELINES → Product Identity).

**Screen purpose.** Does the screen still answer its question?
- Dashboard: "What does my day look like?"
- Checklist: "What needs my attention?"
- Calendar: "When is everything happening?"
- Baby profile: "How is my baby doing?"
- Add/Edit: "What information do I need right now?"

**Design system.**
- Use existing styles (`StaticResource`) and palette colors (`DynamicResource`). A hardcoded color (hex or named), a palette color read with `StaticResource`, or a hardcoded font size in **new or changed** XAML is a finding. So is a property that gets a `DynamicResource` from both its base value and a trigger setter: it stops following palette swaps. Check every change in both palettes.
- Known existing debt; don't report it as a regression: `TextColor="Gray"` on the Entry fields in `AddBabyPage.xaml` and `AddTaskPage.xaml`, which is also a contrast risk; `FontSize="18"` in `ChecklistPage.xaml`; and `FontSize="22"` on the emoji labels in `MainPage.xaml`.
- A new palette color goes into both palette files under the same key; a new neutral into `Colors.xaml`; a new style into `Styles.xaml`. Dark mode is not planned. If a change raises it, note it as an open question for the owner.
- Prefer reusing card and rounded-container patterns over one-off layouts.

**Accessibility.** This is never traded for looks.
- Text contrast is at least 4.5:1 (3:1 for large text).
- Touch targets are at least 44×44 (iOS) and 48×48 dp (Android).
- Interactive elements carry `SemanticProperties.Description` or `Hint` where the visual isn't self-explanatory.
- State is never shown by color alone, e.g. completion and priority.

**Flow.** One clear primary action per screen, and no hidden actions or deep navigation. Adding and editing still route through `AddTaskPage` and `AddBabyPage` (ADR-004).

**Copy.** Short, friendly and specific. Toasts and companion lines celebrate and reassure, and never scold. The companion is subtle, not a chatbot.

**MAUI caveats.** Check layout at narrow and very wide desktop windows, since the owner's manual pass doesn't cover unusual sizes. Keep Syncfusion scheduler styling inside `CalendarPage` (ADR-003).

## 3. Output

- **Design goal:** the problem being solved
- **Companion alignment:** Low / Medium / High
- **Findings,** each with `file:line`, what's wrong, and the suggested XAML or copy change
- **Accessibility result:** pass, or the specific failures
- **Recommendation:** Approve / Adjust / Redesign, with advantages vs. disadvantages
- **Check on screen:** the specific things the owner should look at while running the app

Future: the owner may add Figma to the UX workflow. When a Figma file is provided, treat it as the design intent and review the XAML against it.
