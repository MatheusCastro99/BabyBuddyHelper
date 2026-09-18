# ux-design

## Purpose

A UX/UI lens used alongside the working session. It keeps screens, XAML styling and user-facing copy consistent with the Parenting Companion identity and accessible to every caregiver.

## When to use

- Designing or reviewing a page or control
- Changing XAML styles, colors or layout
- Writing user-facing text: toasts, labels, empty states, companion lines
- Planning a redesign

## Context it loads

- [UI_GUIDELINES.md](../context/UI_GUIDELINES.md) is the authority for identity, principles, palette, typography and screen purposes.
- `Resources/Styles/Colors.xaml` and `Resources/Styles/Styles.xaml` hold the shared tokens, with light and dark values via `AppThemeBinding`.
- The page or control being changed.

## What it checks

- **Identity:** warm, calm, encouraging, low-stress; never corporate, clinical or guilt-inducing
- **Screen purpose:** each screen still answers its question (Dashboard: "What does my day look like?", Checklist: "What needs my attention?", Calendar: "When is everything happening?", Add/Edit: "What information do I need right now?")
- **Design system:** existing tokens and styles are used, with no hardcoded colors or sizes in pages; new tokens get light and dark values
- **Accessibility:** contrast ≥ 4.5:1 in both themes, touch targets ≥ 44/48, semantic descriptions, state never conveyed by color alone
- **Flow:** one clear primary action, and adding/editing route through `AddTaskPage` or `AddBabyPage` (ADR-004)
- **Copy:** short, friendly, reassuring

## Output

The design goal, a companion-alignment rating, findings with `file:line` and the suggested change, an accessibility result, and a recommendation (Approve / Adjust / Redesign) with trade-offs. It also lists what the maintainer should check on screen, since the skill reviews XAML and can't see the running app.

## Future

Figma may be added to the UX workflow. When a Figma design is provided, it becomes the design intent the XAML is reviewed against.
