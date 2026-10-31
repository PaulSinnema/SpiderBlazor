# Changelog

All notable changes to Spider Solitaire are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [1.0.3] - 2026-10-31

### Added
- Automatic detection of slow devices: frame times are measured while animations run; after repeated stutter the app switches to simple animations and remembers this per device.
- Simple ("lite") animation mode: no card shadows, shorter transitions, fade-in when dealing, no tilt or stagger when flying cards, lighter confetti and no blurred menu background.
- Settings → Animations: choose Automatic, Full or Simple. Automatic shows which mode was detected.
- The system "Reduce motion" setting enables simple animations immediately.

### Changed
- Card fly animations (completed run, undo of a deal) read all positions before moving cards, reducing layout work on every device.

## [1.0.2] - 2026-10-14

### Fixed
- Update on iPad and iPhone loaded the old version again because of the GitHub Pages / Safari HTTP cache (max-age 600 s). Update now compares a server build id with the running one, re-fetches all files bypassing the cache and reloads fresh.
- Service worker used the site root as base path; it now uses its own folder so offline caching works under `/SpiderBlazor/`.
- Service worker is registered with `updateViaCache: 'none'` so it is never served stale.

### Added
- `version.json` with a unique build id stamped by the GitHub Actions workflow.
- Build id shown next to the version number (top left).
- Notice at startup when a newer version is available.
- "You already have the latest version" message when nothing is to be updated.

## [1.0.1] - 2026-10-14

### Added
- Version number shown in the top left corner.

### Fixed
- When dragging a run of cards the cards were stacked in the wrong order; they now keep their order in the column while dragging and landing.

## [1.0.0] - 2026-10-13

### Added
- Spider Solitaire as a Blazor WebAssembly Progressive Web App, playable in the browser and offline on iPad, iPhone, Android, Windows and Mac.
- Difficulty levels: Easy (1 suit), Medium (2 suits) and Hard (4 suits), with win counters per level.
- Drag and drop with mouse and touch, or tap to move to the best spot.
- Undo with animation, hint, new game and restart (replay the same deal).
- Long columns are compressed; press and hold a fixed card to inspect it.
- Optional highlighting of movable cards.
- Large sans-serif card indices, one large suit symbol on number cards and drawn court cards.
- Completed runs and the stock shown full size in the top row; completed runs fly to the top with an animation.
- Confetti shower when the game is won.
- Bottom menu sheet, opened by tapping next to the cards.
- Languages: English, Dutch, German and French.
- Add to Home Screen and Update buttons in Settings.
- Game state, settings and statistics saved automatically in `localStorage`.
- Automatic deployment to GitHub Pages via GitHub Actions.
