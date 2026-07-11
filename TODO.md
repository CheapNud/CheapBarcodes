<!--
  TODO.md — CheapBarcodes project work tracker
  Last updated: 2026-07-11

  RULES FOR AI AGENTS:
  - Update the "Last updated" date above whenever you modify this file
  - Items use checkbox format: - [ ] incomplete, - [x] complete
  - Never remove completed items — they serve as history. Move them to "## Done" when a category gets cluttered.
  - Each item gets ONE line. Details go in sub-bullets indented with 2 spaces.
  - Prefix each item with the date it was added: - [ ] (2026-03-17) Description
  - When completing, change to: - [x] (2026-03-17 → 2026-03-18) Description
  - Tag the SOURCE of each item at the end in brackets:
      [code-todo] = from // TODO comment in source code
      [plan] = from a plan document or planning session
      [bug] = from a bug encountered during dev/deploy
      [audit] = from a code audit or review
      [user] = explicitly requested by the user
  - For [code-todo] items, ALWAYS include file:line reference so devs can navigate directly
  - Categories: Blocking, Planned, Future, Done
  - New items go at the TOP of their category
  - Do not create separate TODO_*.md files — everything goes here
  - Keep it terse. If it needs more than 3 sub-bullets, link to a plan document.
  - Do NOT create, rename, or remove categories — the fixed set is: Blocking, Planned, Future, Done
  - When asked for planned work or TODO analysis, ALWAYS include Future items too — list them below Planned and note them as future work
-->

# TODO

## Blocking

- [ ] (2026-07-11) Unpin CheapHelpers.Services and enable image scanning once barcode services ship in an Android-safe package [user]
  - Pinned to 1.1.2: newer versions drag in CheapHelpers.EF, whose Microsoft.AspNetCore.App framework reference has no Android runtime pack (NETSDK1082)
  - 1.1.2's `ReadBarcodeAsync` throws NotImplementedException, so image scanning is dead until then
  - On upgrade: `ReadBarcodeAsync(bytes)` drops the width/height args, `GetBarcode` becomes `GetBarcodeAsync` — Scanner.razor call sites

## Planned

_Nothing planned._

## Future

_Nothing in future._

## Done

- [x] (2026-07-11 → 2026-07-11) Bring repo in line with sibling projects: net11.0-android + MAUI 10.0.80 + MudBlazor 9.7.0, CodeQL (buildless) + dependency review workflows, README refresh [user]
