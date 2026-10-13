# Spider Solitaire

The classic **Spider** card game for the browser, iPad, iPhone, Android, Windows and Mac. After the first visit it also works **offline**.

## ▶️ Play now

**https://paulsinnema.github.io/SpiderBlazor/**

Open the link and play. No download, no sign-up.

## 📲 Install as an app

Add the game to your home screen and it opens like a regular app (full screen, works offline).

### iPad and iPhone (Safari)
1. Open **https://paulsinnema.github.io/SpiderBlazor/** in **Safari**.
2. Tap the **Share** button (square with an arrow pointing up).
3. Choose **Add to Home Screen** and tap **Add**.
4. Open the game once from the new icon while online. After that it also works offline.

### Android (Chrome)
1. Open the link in **Chrome**.
2. Tap **⋮** (menu) and choose **Install app** or **Add to Home screen**.

### Windows and Mac (Chrome or Edge)
1. Open the link.
2. Click the **install icon** on the right side of the address bar, or choose **Install app** from the menu.

> Tip: the game itself has a **Settings → Add to Home Screen** button that helps you with this.

## 🃏 Rules in short

- Played with 104 cards (two decks) in 10 columns.
- Place a card on a card exactly one rank higher, regardless of suit.
- A descending sequence of the **same suit** can be moved as a whole.
- A complete **King to Ace** sequence of one suit is moved to the top automatically.
- Tap the stock (top right) to deal 10 new cards (only when no column is empty).
- You win when all 8 sequences are complete.

## 🎮 Controls

| Action | How |
|---|---|
| Move card(s) | Drag, or tap to move to the best spot automatically |
| Open/close menu | Tap an empty spot next to the cards, or the small bar at the bottom |
| Inspect a card in a long column | Tap and hold; release to put it back |
| Deal | Tap the stock in the top right |

**Menu:** New · Restart (same deal from the start) · Undo · Hint · Settings

**Settings:**
- **Difficulty:** Easy (1 suit), Medium (2 suits), Hard (4 suits)
- **Highlight movable cards:** non-movable cards are shown slightly darker
- **Language:** English, Nederlands, Deutsch, Français
- **Add to Home Screen** and **Update** (fetch the latest version)

Your game, settings and number of wins are saved automatically on your device.

## 🛠️ For developers

- **Tech:** C# / .NET 10, Blazor WebAssembly as a Progressive Web App (PWA). No server required.
- **Storage:** browser `localStorage`.
- **Run locally:** open `SpiderBlazor.sln` in Visual Studio and press Ctrl+F5, or:
  ```
  dotnet run
  ```
- **Deploy:** every push to `master` builds and publishes to GitHub Pages via `.github/workflows/deploy.yml`.

### Project structure

| Path | Contents |
|---|---|
| `Game/` | Game logic (`SpiderGame`), game state (`GameState`), card drawings (`CardSvg`) |
| `Localization/Text.cs` | Translations (EN, NL, DE, FR) |
| `Pages/Home.razor` | Table, menu and dialogs |
| `wwwroot/js/spider.js` | Dragging, animations, install and update |
| `wwwroot/css/app.css` | Styling |
