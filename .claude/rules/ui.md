---
paths:
  - "UI/**"
  - "**/*.xaml"
---

# UI essentials

The full guidelines, screen by screen, are in `Harness_Config/Context/UI_GUIDELINES.md`; read the section for the screen you're changing. For a UX review, use the `ux-design` skill.

- **Identity:** a warm, calm, encouraging Parenting Companion. Never a spreadsheet, productivity or clinical dashboard, generic to-do app or AI console. No punitive, guilt-inducing or alarming copy.
- **Styles and tokens** live in `Resources/Styles/`: `Palettes/BoyPalette.xaml` and `GirlPalette.xaml` (palette colors and icons), `Colors.xaml` (shared neutrals), `Styles.xaml` (named styles). New tokens and styles go there, not inline.
- **Palettes:** the two palette files hold identical keys; a new color goes into both. Read palette colors with `DynamicResource`; consume styles with `StaticResource`. Never use `AppThemeBinding` (the app is pinned to the light theme). A property must not get a `DynamicResource` from both its base value and a trigger setter.
- **Contrast:** buttons fill with `PrimaryDark` so white text reaches 4.5:1. Success stays green in both palettes.
- **Status** is always spelled out in words, never shown by color alone. Overdue and due states are gentle reminders, with no red or alarm styling.
- **Controls:** tap targets at least 44x44, with screen-reader labels on icon buttons. Deleting asks first.
- **Feedback:** brief toasts for success; failures the user must acknowledge use `AlertService` (ADR-013). Cub claps for a completion, nods for an add or update, and stays still for a removal (the API key is the exception: claps when saved, nods when removed). Each animation plays once and never loops.
- **Chat:** opt-in. Cub never opens it, prompts for it, or interrupts with it. A saved API key is never shown again.
- **Medical content** is reference data, not advice: name the source, point to the pediatrician, never diagnose or prescribe.
- **Navigation:** the baby profile is read-only and pushed with a back arrow (ADR-004, ADR-016); edits route to their add/edit pages.
