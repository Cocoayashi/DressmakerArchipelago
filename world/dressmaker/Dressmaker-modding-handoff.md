# Dressmaker BepInEx Modding: Session Handoff

Paste this into a new conversation to continue where we left off.

---

## How to teach me (IMPORTANT, read first)

- **Do NOT write code for my project.** I write every line myself.
- Lead me step by step, using **examples**.
- Examples must **never be the solution** to what I'm currently learning. They should be *slightly relevant* only: a different made-up game (we've been using a "farming game" with `PlayerWallet`, `ShopUI`, `Crop`), a different Unity type, or a different property.
- When I'm stuck, give hints, trace-through tables, analogies and leading questions. Explaining a bug in plain words is OK when I'm really stuck, but don't write the fixed code.
- Researching the game's code together (me pasting ILSpy snippets) is fine.
- Non-code help (finding files, explaining errors, tooling commands) can be answered directly.
- Game-specific facts come from what I paste from ILSpy, or from reading the game's compiled `Assembly-CSharp.dll` with read-only tools (load `BepInEx\core\Mono.Cecil.dll` from bytes in PowerShell; `Add-Type` fails on it). Say so when you're guessing.
- **Hand-hold me.** I often don't know what to do next. Break work into small numbered steps, each with a **"Check:"** (how I know it worked) and a "Your job:" line. Give me one or two steps at a time, then wait for me to paste results or errors.
- Example code is welcome as long as it's the made-up farming game (or similar), never my actual solution.
- **I know very little Python and nothing about apworlds** (as of 2026-10-28). Explain Python concepts as they come up, comparing them to C# where it helps.
- **I do all git work myself in GitHub Desktop** (the `git` command isn't on PATH). Don't run git; tell me what to commit and with what message.

---

## Current focus (read with `dressmaker_ap_plan.txt`)

The real goal is a **Dressmaker Archipelago** (multiworld randomizer). The full project plan, game facts, research findings and design decisions are in:
`C:\Users\lolli\OneDrive\Desktop\Archipelago\worlds\dressmaker\dressmaker_ap_plan.txt` (the one in the Archipelago repo is the master copy. Copies in the mod repo's root and `world\dressmaker\` were synced on 2026-11-07 but can drift)
**Read that file first.** This handoff covers how to teach me and where I am right now.

| Item | Location |
|---|---|
| Mod project (BepInEx plugin, own repo) | `C:\Users\lolli\OneDrive\Desktop\DressmakerArchipelago\` (`Plugin.cs`, `DressmakerArchipelago.csproj`, `.sln`) |
| Mod `.csproj` | Already references `Assembly-CSharp.dll` and `Framework.dll`, and has a post-build copy target to `<game>\BepInEx\plugins` |
| `Plugin.cs` | Plugin entry point: Harmony setup, `Update` loop, hard-coded `Login()`. `Login()` fills `allLocations`, compares the seed with the slot's file (`isCorrectServer`), and resends every saved check in one call. The "Connect to server" button is commented out. Also holds the `getTags` helpers |
| `PlayerJson.cs` | (added 2026-11-06) The per-save-slot file `BepInEx\plugins\playerArchipelago_<slot>.json`: `goldCount`, `seed`, `allLocationsReached` (a `HashSet<string>` of location names). `filepath` is a static `=>` property built from `Plugin.saveSlot`. `Loadfile()` (static) reads the file or makes a new one; `SaveFile()` writes it |
| `UnlockItems.cs` | Queues received items and applies fabrics, patterns and Gold (`Plugin.playerJson.goldCount` stops repeat Gold payouts) |
| `LocationChecks.cs` | `allLocations` (internal name → location ID), the dress, reputation and friendship checks, and `RememberCheck` (adds a name to `allLocationsReached` and saves the file) |
| `StopUnlockItems.cs` | Prefixes that block native unlocks: starting, prestige, relationship, and `DressmakerMods.FabricAvailability.RestoreEarnedUnlocks`. The prestige and relationship ones also send the reputation and friendship checks |
| `HarmonyClass.cs` | (was `HarmonyTestingClass.cs`) Postfixes: `GameManager.SetScene` (at `FrontDesk`: sets "save loaded", the slot, and loads the slot's file), `Dress.CheckAchievements` (flags a dress for checking), and `PlayerProgress.DeleteSlot` (deletes that slot's file) |
| `GetDump.cs`, `StoryQuestInfo.cs` | Data dump code (button commented out in `Plugin.cs`) and its data classes |
| Archipelago repo (Python, apworld goes here) | `C:\Users\lolli\OneDrive\Desktop\Archipelago`, branch `DressmakerArchipelago`. `worlds\dressmaker\` (lowercase) holds this handoff, the plan, the apworld `.py` files and `data\dressmaker_dump.json` |
| git for the mod repo | Initialized (`.git` and `.gitignore` exist as of 2026-10-28) |
| Python IDE | **PyCharm** (paid), with the Archipelago folder opened as the project |
| Python for Archipelago | **3.13.17** in a venv at `Archipelago\.venv` (PyCharm interpreter "Python 3.13 (Archipelago)"). The repo only accepts **3.12.10 to 3.13.x** (checked in `ModuleUpdate.py`); 3.14/3.15 won't run. Python 3.7.9 is also installed but unusable for this. In PyCharm's terminal (Alt+F12, prompt shows `(.venv)`) use plain `python`; outside the venv use `py -3.13` |

**Plugin #2 (gold button) is paused.** Its notes are kept below because the `PlayerProgress.Load` postfix idea is reusable.

### Data dump plugin (✅ done 2026-10-28; the current work is the apworld, see "Next step when resuming" at the bottom)

**Why:** the apworld (Phase 4) needs real game data: names, counts, slots, tags and requirements. A plugin can't write the whole apworld (options, rules, regions and logic are hand-written Python), but it **can dump the game's data to a JSON file**. The apworld then reads that file (`worlds/dressmaker/data/*.json` via `pkgutil.get_data`, or a script turns it into `.py` lists). The apworld must never need the game installed to generate a seed. Building location names from the same dump keeps the mod and the apworld names matching.

**What to dump:**
1. `QuestDefinition.AllQuestDefinitions`: `.name`, `.PrettyNameUnlocalizedEnglishDefault`, `.questGiverCharacter` (store its name), `IsGiftQuest()`, and the types of `.requirements` and `.completionRequirements`
2. `ActiveCampaign.quests` and `.sideQuests`: names and counts (settles the 53 / 25 question)
3. Every `GarmentComponent` (pattern): name, slot (bodice, skirt, sleeve or collar), tags
4. `FabricType.AllFabricTypes`: name, tags, reputation requirement
5. `CharacterDefinition.AllCharacterDefinitions`
6. `PlayerProgress.Current.unlockedPatterns` on a brand-new save (the native starter patterns)
7. If findable: which patterns each relationship level unlocks

### Dump progress (as of 2026-10-28)

**Done:** the dump plugin works and writes JSON with Newtonsoft (Steps 0-5 are done). The re-dump was reviewed on 2026-10-28 (see "Dump review" below).

| Section | Result |
|---|---|
| Story quests (`ActiveCampaign.quests`) | 53 ✅ |
| Side/pigeon quests (`ActiveCampaign.sideQuests`) | 24 ✅ |
| Final quest (`QuestDefinition.FinalQuest`, static) | `FinalQuest` / "Peerless Dressmaker", giver `Blank`. Stored in its own `finalQuestInfo` field. **53 + 24 + 1 = 78 commissions** ✅ |
| Fabrics (`Fabric.AllFabrics`) | **363** after removing unreleased fabrics (564 in `AllFabrics`, 201 of them unreleased). `name`, `prettyName`, `itemTags` |
| Characters | 10 names in `characterNames` |
| Patterns | **157** = 117 from relationship levels (`character.GetPatternsUnlockedAtLevel(0..5)`) + 40 starters (`PlayerProgress.Current.unlockedPatterns` on a fresh save). Each has `name`, `prettyName`, `PatternType` and `itemTags` (from `garment.additiveTags`, 20 different tags, weights -10 to 25) |

**Dump review (2026-10-28, fabric section updated 2026-10-31), `worlds\dressmaker\data\dressmaker_dump.json`:**
- Top-level keys: `storyQuestInfos` (53), `sideQuestInfos` (24), `finalQuestInfo`, `fabricInfos` (363), `patternInfos` (157), `characterNames` (10). Quests have `name`, `prettyName`, `questGiverCharacterName`. Fabrics have `name`, `prettyName`, `itemTags` (tag → weight). Patterns have `PatternType`, `name`, `prettyName`, `itemTags`.
- ✅ No `null`s anywhere, no modded content, none of the 21 cut patterns (so no filter is needed for them). `No Collar` and `No Sleeves` are present and must be dropped.
- Patterns: 59 bodices, 46 skirts, 31 sleeves, 21 collars. Pattern and quest names are all unique. No pattern name matches a fabric name.
- Story commission givers: Prudence 7, Priya 6, everyone else 5. Each character has 2–3 side quests.
- Pattern `name` and `prettyName` often differ (e.g. `Ruffle Bodice` = "Off-Shoulder Begonia Bodice", `V collar scalloped` = "Scalloped Collar"). **Decided:** AP names use `prettyName/name` (e.g. `Use pattern: <prettyName>/<name>`), and the mod must build the same strings.
- **Unreleased fabrics removed (2026-10-31):** 201 of the 564 fabrics were unreleased and are no longer in the dump, leaving **363**. That also resolved:
  - Empty `itemTags`: 70 fabrics → **1** (`Velvet Crushed Rouge` / "Rouge Crushed Velvet"). Check that one in ILSpy or in-game.
  - `_CottonVictorianPattern`: gone (it was unreleased).
  - Duplicate fabric `prettyName`s: none left.
- The dump doesn't record which 40 patterns are the native starters. Add a field if it's needed for removing the native starters on a fresh save.
- Pattern tags (20): Casual, Cool, Cute, Daywear, Eclectic, Elaborate, Elegant, Eveningwear, Flowers, Formal, Glamour, Goth, Playful, Professional, Risque, Romantic, Simple, Uncomfortable, Warm, Workwear. Fabrics have those plus **Patterned** and **Shimmering** (22).
- **To commit:** the dump/plan/handoff in the Archipelago repo, and the dump-plugin fixes in the mod repo (via GitHub Desktop). Unknown whether this was done.

**Facts learned:**
- `FinalQuest` unlocks (knighthood letter, `ShouldPresentKnighthoodLetter()`) once **every** quest in `ActiveCampaign.quests` is completed. Side quests don't count. Completing `FinalQuest` rolls the credits, so it's a clean `main_story` victory event.
- **155 obtainable patterns** = 157 minus `No Collar` and `No Sleeves`. That matches the wiki's 155. The "No" pieces are always available and must **not** be AP items.
- **21 base-game patterns are unobtainable (cut content)** and must not be in the item pool: Black Widow, Carmilla, Cinched Blouse, Elevated Ruffled Square, Empire Waist Princess, Ruffled Square, Sweet Maiden and Widow bodices; Bo Peep, Empire Princess, Evie, Frill Wrap, Handkerchief Hem, Junon, Petal Bow, Spiral, Venus and Widow skirts; Dove, Galadriel and Half Bishop sleeves. (`BodiceDefinition.AllBodiceDefinitions` is the same 67 bodices as in `AllGarmentComponents`, so it doesn't filter anything.)
- Which character and level originally gave a pattern does **not** matter for the apworld: relationship levels are locations ("Rose Level 3"), and patterns are granted with `UnlockPattern(name)`.
- ~~3 pairs of fabrics share a display name~~: resolved. Those fabrics were all unreleased and are gone from the 363. AP names include `name` anyway, so they stay unique.

**Dumping rules (to keep modded content out):**
- Close the game and move the other mods out of `BepInEx\plugins` first, **especially `DressmakerMods.dll`**. It loads the modded patterns `brandy.layered-tutu-skirt` (Layered Tutu Skirt) and `tiered_sleeves_long` from `BepInEx\config\DressmakerMods\garment-cache\` and also adds them to a fresh save's `unlockedPatterns`.
- Use a **fresh save**. An old save that used modded fabrics creates 280 `Missing Fabric <id>` placeholders when the mods are gone.
- After building, check that the DLL in `plugins` is newer than the last save of `Plugin.cs`. A stale DLL caused confusing results twice.

### 📌 PINNED: unlock/access logic (deferred, but MUST be done before release)

Decided 2026-10-28: put the "what can be unlocked when" logic on hold and come back to it later. The apworld can start with simple or no access rules, but it **cannot be released** until this is done:
- [ ] **Fabric data in the dump:** fabric tags (like the pattern `additiveTags`) and each fabric's **reputation requirement** (research the `Fabric` class in ILSpy)
- [ ] **Commission requirements:** dump each quest's `.requirements` (what unlocks it) and `.completionRequirements` (what finishes it: `FabricCompositionRequirement`, `FabricTypeRequirement`, `TagScoreRequirement` etc.), then work out which fabric and pattern items each commission needs
- [ ] **Character availability:** characters unlock through the story, so relationship-level locations depend on story progress. Also need `CampaignDefinition.giftingUnlockRequirement` (gifting gates relationship levels)
- [ ] **Story gating:** `FinalQuest` needs all 53 story quests. Check how story quests unlock each other
- [ ] **Tutorial:** check whether the tutorial quest needs a specific pattern (affects random starting patterns)
- [ ] **Colour requirements (found 2026-11-01, first test seed):** a seed's 23 random starting fabrics had no fabric named "black". If an early commission or the tutorial needs a black (or other specific colour/type) fabric, the player can get stuck. The dump has no fabric colour data yet. Either guarantee one matching fabric in the starting set (quick fix) or dump colour/type and write commission rules (proper fix)
- [ ] Turn all of this into access rules in `rules.py` and test that generation never creates impossible seeds

### Original step plan (from 2026-10-26, kept for reference)
- **Step 0:** `git init`, `dotnet new gitignore`, first commit in the mod folder. *Check:* `git log` shows a commit.
- **Step 1:** a `GUI.Button` in `OnGUI` that logs `QuestDefinition.AllQuestDefinitions`' count. (Use a button, since I already know it. Unity 6 may use the new Input System, so `Input.GetKeyDown` might not work; unverified.) First check in ILSpy whether it's a List (`.Count`), an array (`.Length`) or something else, and which namespace/`using` it needs. *Check:* the log shows ~96. Also compare main menu vs. after loading a save.
- **Step 2:** loop over the quests and log `.name` and the pretty name. Then log `ActiveCampaign.quests.Count` / `.sideQuests.Count` via `SingletonBehaviour<GameManager>.Instance.ActiveCampaign`. It's probably null on the main menu, so null-check it.
- **Step 3:** write to a file instead of the log: `File.WriteAllLines(Path.Combine(Paths.ConfigPath, "..."), lines)` (`using System.IO;`, `System.Collections.Generic;`). *Check:* the file appears in `<game>\BepInEx\config\`.
- **Step 4:** the same for patterns, fabrics and characters.
- **Step 5:** switch to JSON with Newtonsoft (details below).
- **Step 6:** start the apworld in Python, reading the JSON.

**Step 5 details (JSON):**
- The game ships `Newtonsoft.Json.dll` in `Dressmaker_Data\Managed`. Reference it like `Framework.dll`, with `<Private>false</Private>`, because the game already loads its own copy. Then `using Newtonsoft.Json;`.
- **Don't serialize game objects** (ScriptableObjects reference each other, which causes a "self referencing loop" error or a huge file). Make small plain data classes (public fields: strings, ints, bools, lists) plus one outer "dump" class, and copy values in. For references to other objects, store `.name`.
- `JsonConvert.SerializeObject(dump, Formatting.Indented)` → `File.WriteAllText(...)`.
- Check: `python -c "import json; d=json.load(open('dressmaker_dump.json')); print(len(d['quests']))"`
- Pitfalls: private fields aren't written; nulls mean the data was read before a save loaded; enums come out as numbers (use `.ToString()` for names).
- **The planned first move for Step 5:** I write my data classes (quests only at first) and paste them for review *before* writing the fill loop.

### Dress-complete / achievements note
The dress-complete hook is `Dress.CheckAchievements` (Postfix in `HarmonyTestingClass.cs`). The pieces are `SingletonBehaviour<GameManager>.Instance.activeDress.state.collarDefinition`, `.sleeveDefinition`, `.bodiceDefinition` and `.skirtDefinition`. Verified working in the 2026-11-04 test.
**Not yet recorded:** when `activeDress` or its `state` gets cleared. The old `LocationCheck` errors started right after the first send, so something in that chain went null within moments. The flag is now reset before the checks run, so a failure should only happen once per dress (not retested yet).

**Fabrics on a dress:** `activeDress.state.panelStates` is a `List<PanelState>`, and each `PanelState` has a `fabric`. From the game's compiled code:
- `Dress.GetTagScores` null-checks `fabric`, so it may be null.
- `DressState.GetFabricCost` stops at whichever is shorter, `panelStates` or the current pattern panels (built from the four definitions and their variants). So `panelStates` might hold leftovers from an earlier dress. **Unconfirmed**; see the two-dress test under "Next step when resuming".
- `PanelState.placedOnMannequin` exists, but it's unconfirmed whether it's still accurate when the dress is submitted. Log it before filtering on it.

### Commission hook (found 2026-11-04 in the compiled game code, not yet used by the mod)
- `PlayerProgress.RecordCompletedQuest(QuestState qs)` adds the quest to `PlayerProgress.completedQuests`. It's called from `FrontDesk.StoreCompletedQuest` (used by `FrontDesk.CompleteQuest` and `PlayEnding`) and from `PhotoScene.ConfirmSell`.
- `qs.questDefinition.name` is the internal name, which is the `allLocations` key for story and pigeon quests (e.g. `RoseWork`).
- `ConfirmSell` also records quests, and those might not be locations, so the lookup needs `TryGetValue`.
- **The final quest's name doesn't match:** the game calls it `FinalQuest`, but its location is `Complete Final Commission` (ID 8000). Compare against `QuestDefinition.FinalQuest` and handle it separately.
- `qs.outcome` can be `NotCompleted`, `Failed`, `Succeeded` or `Excelled`. The plan already decided that any accepted dress counts, so don't filter on it.
- **Suggested design:** same as the dress check. A Postfix sets a flag, a method called from `Update` loops over the whole `completedQuests` list, and the flag is also set after login so earlier quests get sent. Repeats are harmless because the server ignores them.

---

## My setup (verified facts)

| Item | Value |
|---|---|
| Game | Dressmaker, by **Cozy Lives** |
| Game folder | `C:\Program Files (x86)\Steam\steamapps\common\Dressmaker` |
| Unity version | 6000.3.4 |
| Scripting backend | **Mono** (has `Assembly-CSharp.dll` in `<Game>_Data\Managed\`) |
| BepInEx | **BepInEx 5**, installed in the game folder and working |
| Plugin target framework | **`netstandard2.1`** (NOT 2.0, see "Lessons learned") |
| Project template | BepInEx template (`bepinex5plugin`) |
| First project name | `MyFristPlugin` (typo in the project name; the plugin's display name is "My first plugin", GUID/name MyFirstPlugin) |
| Decompiler | ILSpy |
| Save folder | `C:\Users\lolli\AppData\LocalLow\Cozy Lives\Dressmaker\` |
| Main save | `player.json` (~3.6 MB). Also has `player.json.lastgood` (game's backup) and a second slot `player1.json`. There's also `player.json.moddata.json`, so the game appears to store mod data separately. Steam Cloud syncs saves (`steam_autocloud.vdf`). |
| BepInEx console | Enabled via `BepInEx\config\BepInEx.cfg` → `[Logging.Console] Enabled = true` (suggested; may or may not have been turned on) |

Tip I learned: Unity saves usually live in `%USERPROFILE%\AppData\LocalLow\<Company>\<Game>\`. The company name is the first line of `<Game>_Data\app.info`.

---

## Plugin #1: "my first plugin works!" notification ✅ DONE

What I built:
1. Built the template plugin and confirmed it loads. Log output:
   ```
   [Info   :   BepInEx] Loading [My first plugin 1.0.0]
   [Info   :My first plugin] Plugin MyFirstPlugin is loaded!
   ```
2. Added `OnGUI()` with `GUI.Box` / `GUI.Label` and a `Rect` to show "my first plugin works!" on screen. ✅ Worked.
3. Made it disappear after 15 seconds. ✅ Worked.
   - Stored `Time.time` in a field (`awakeTime`) **once in `Awake()`**.
   - In `OnGUI`, early `return` when `Time.time - awakeTime > 15f`.
   - My bug along the way: I was reassigning `awakeTime = Time.time` inside `OnGUI` every frame ("restarting the kitchen timer"). Fixed by removing that line.

### Concepts I've learned
- `[BepInPlugin(...)]` attribute, `BaseUnityPlugin`, `Logger.LogInfo`
- Unity calls methods by name: `Awake()` (once), `Update()` (every frame), `OnGUI()` (every frame, for IMGUI drawing)
- `GUI.*` calls only work inside `OnGUI`
- IMGUI is **immediate mode**: there's no "erase". You stop drawing something by not calling the draw line that frame.
- `Rect(x, y, width, height)` measured from the top-left. `Screen.width` / `Screen.height`.
- `GUI.skin.box.fontSize` changes the style of **every** box (it's shared).
- Fields persist between frames; local variables don't.
- `Time.time` (affected by time scale) vs `Time.realtimeSinceStartup`
- Debugging by drawing values on screen in a debug box
- The early `return` pattern

---

## Plugin #2: set in-game gold with a button ⏸️ PAUSED (focus moved to the Archipelago dump plugin)

**Goal:** an on-screen button that sets my gold to a specific amount, shown **only when a save is actually loaded** (not on the main menu, since there are multiple saves).

### What I found in ILSpy
- `PlayerProgress.Current` is **static** (confirmed). `PlayerProgress.Current.gold` is a **public `int`**.
- **A default `PlayerProgress.Current` exists even on the main menu**, so `Current != null` can't tell me whether a save is loaded.
- The real save is loaded by **`PlayerProgress.Load`**. (Not yet checked: whether it's public/private, and whether it has overloads.)
- `GameManager.AddGold(int)` adds gold (no set method) and calls `RefreshUI()`. It is an **instance method**, so it needs a `GameManager` object.
- `GameManager` inherits from **`SingletonBehaviour<GameManager>`**, which lives in **`Framework.dll`** (in the same `Managed` folder).

### Chosen approach
- Read the current gold from `PlayerProgress.Current.gold`, then call `AddGold(target - current)` so the UI refreshes. Writing the field directly would skip `RefreshUI()`.
- Use a **Harmony Postfix on `PlayerProgress.Load`** to set a `public static bool` flag on my plugin meaning "a real save is loaded".
- `OnGUI`: early `return` if the flag is false. Otherwise draw `GUI.Button`, and in its `if` block do the gold logic. (`GUI.Button` returns `true` on the frame it's clicked.)
- Null-check everything (`GameManager`, `PlayerProgress.Current`).

### Where I left off / open items
1. **Two compile errors I was fixing:**
   - `Cannot convert method group 'FindAnyObjectByType' to non-delegate type 'GameManager'`: missing `()` on the call.
   - `The type 'SingletonBehaviour<>' is defined in an assembly that is not referenced... 'Framework'`: need to add a `<Reference>` to `Framework.dll` (with `<Private>false</Private>`), the same way I referenced `Assembly-CSharp.dll`.
2. **Investigate `SingletonBehaviour<T>` in `Framework.dll`** (File → Open in ILSpy). The suspicion, **not yet verified**, is that it has a static member (like `Instance`) that returns `T`, meaning `GameManager` can be reached statically and `FindAnyObjectByType` may not be needed. Also use ILSpy **Analyze → Used By** on `AddGold` to see how the game itself reaches `GameManager`.
3. **Verify the `Load` Postfix** by logging first. Does `Load` also fire on the main menu (for example, to show save-slot info)? Once or several times? Patch classes are static and can't use the plugin's `Logger` directly, so I need a static way to reach a logger.
4. **Later:** decide what sets the "save loaded" flag back to `false` when quitting to the main menu.
5. Test plan: back up the save folder first (game closed), click the button, check that the UI updates immediately, then save, quit and reload to check that it stuck.

### Concepts covered for plugin #2 (explained, being applied)
- Referencing game DLLs in the `.csproj` (`<Reference Include="..."><HintPath>...</HintPath><Private>false</Private></Reference>`)
- `ClassName.Member` access means the member is static
- `UnityEngine.Object.FindAnyObjectByType<T>()` (Unity 6 name; `FindObjectOfType` is deprecated). It returns null if nothing is found, and it's slow, so don't call it every frame.
- `Update()` for logic, `OnGUI()` for drawing, and a field to share state between them
- **Harmony:** `using HarmonyLib;`, `new Harmony("id").PatchAll();` in `Awake`, `[HarmonyPatch(typeof(X), nameof(X.Method))]` on a separate static class, `Prefix` / `Postfix` found by name, patch methods are static, and a `public static` field passes info back to the plugin. Use a string name instead of `nameof` for private methods. Overloads need disambiguation (not yet covered).
- **Replacing a method:** a `bool Prefix` that returns `false` skips the original. Magic parameters: `__instance`, `ref __result` (type must match the return type), and original parameters by exact name. Caveats: you skip all the original's side effects, a Postfix that changes `__result` is often safer, and you can return false conditionally. (I asked about this generally; no specific target method chosen yet.)

---

## Tooling / workflow notes

- **Auto-copy DLL after build:** explained an MSBuild `<Target Name="..." AfterTargets="Build">` with a `<Copy SourceFiles="..." DestinationFolder="..." />` task, using built-in variables (`$(TargetPath)`, `$(TargetFileName)`, `$(OutDir)`, `$(ProjectDir)`), plus an optional custom `<PropertyGroup>` variable for the game path. Close the game before building, because a running game locks the DLL. Possible "Access denied" because the game is under Program Files. (I may or may not have finished setting this up.)
- **New plugin from template:**
  ```
  dotnet new bepinex5plugin -n YourPluginName -T netstandard2.1 -U <unity version>
  ```
  Use the same `UnityEngine.Modules` version that's in my first project's `.csproj` for `-U`. Then run `dotnet restore`. Helpers: `dotnet new list bepinex`, `dotnet new bepinex5plugin -h`. A new project needs the `Assembly-CSharp` and `Framework` references and the post-build copy target added again.
- Built DLL output: `bin\Debug\netstandard2.1\<Name>.dll` → copy to `<game>\BepInEx\plugins\`
- Logs: `<game>\BepInEx\LogOutput.log` (or the BepInEx console window)

---

## Lessons learned (errors hit and why)

| Error | Cause | Fix |
|---|---|---|
| `...uses 'netstandard, Version=2.1.0.0' which has a higher version than referenced assembly 'netstandard' 2.0.0.0` | Template targeted netstandard2.0, but the Unity 6 module package needs 2.1 | Changed `<TargetFramework>` in the `.csproj` to `netstandard2.1` |
| Notification never disappeared | Reassigned the start time every frame in `OnGUI` | Set it once in `Awake` only |
| `Cannot convert method group...` | Called a method without `()` | Add `()` |
| `SingletonBehaviour<> is defined in an assembly that is not referenced` | `GameManager`'s base class lives in `Framework.dll` | Reference `Framework.dll` |
| Dump button did nothing; log stopped after "53 Story Quests" | `NullReferenceException` in `OnGUI` (the `activeDress` chain is null on a fresh save), so `File.WriteAllText` never ran | Commented out `activeDress` (it's Phase 3 data anyway). **Exceptions from `OnGUI`/`Update` show up in `AppData\LocalLow\Cozy Lives\Dressmaker\Player.log`, not in BepInEx's log.** Use breadcrumb `LogInfo("1")`, `("2")`... to find the line |
| Dump didn't reflect my code changes | The game ran an older DLL (edited `Plugin.cs` but didn't rebuild, or dumped before the rebuild) | Compare the timestamps of the DLL in `plugins` and `Plugin.cs` |
| Extra fabrics and patterns in the dump | Other mods (Dressmaker Mods fabric/garment packs) were loaded | Move the other mods out of `plugins` before dumping |
| python.org offered only 3.15; 3.13 "can't be downloaded" | Security-only Python versions get no new installers | Got a 3.13.x installer from the older-releases list on python.org/downloads/windows |
| `AttributeError: module 'importlib._bootstrap_external' has no attribute '_w_long'` (also `SRE module mismatch`) | Windows env vars `PYTHONHOME` (System) and `PYTHONPATH` (User + System) pointed at Python 3.7, so 3.13 loaded 3.7's standard library | Deleted those variables (keep `Path`!) and restarted Windows |
| "Environment location directory is not empty" | An old 3.7 `.venv` already existed | Deleted the old `.venv` and created a new one with Python 3.13 as the base |
| PyCharm interpreter `[Invalid]` | PyCharm was still running with the old env vars | Restarted Windows |
| `x: []` didn't create a list (`UnboundLocalError` on `+=`) | `:` is a type annotation, `=` assigns | Use `=` |
| Pattern names in `PATTERNS_BY_TYPE` didn't match item names | Stored `pattern['name']` instead of the full `"Pattern: <prettyName>/<name>"` | Lists must hold exactly the same strings as the dict keys |
| `.append(...)` inside the `[ ]` | Bracket closed in the wrong place, so `.append` was called on a string | Close the `[ ]` first, then `.append` |
| `get_location(...)` would fail | Prefix capitals didn't match (`"Use Pattern: "` vs `"Use pattern: "`); also once passed the region name `"workshop"` instead of the event location name | Strings must match `locations.py` exactly |
| `python -c "import worlds.dressmaker"` warnings `_speedups not available` / `pkg_resources is deprecated` | Harmless AP warnings | Ignore |
| Finished a dress and only 2 things unlocked (2026-11-04) | Not a bug: 4 checks were sent and held 2 Gold + 2 fabrics. Gold is ignored by `UnlockQueue`, so it looks like nothing happened | Compare `Archipelago\logs\Server_*.txt`, which lists every check and its item, against the game log |
| 3,861 `NullReferenceException`s in `LocationCheck` (2026-11-04) | The "needs checking" flag was never reset, so the check ran every frame | Reset the flag **before** the risky code, so a failure can only happen once |
| Game "crashed" on load (2026-11-04) | The server wasn't running. `TryConnectAndLogin` blocks the main thread and `Update` retried every frame, so Windows closed the frozen game | Start the server first. Real fix still to do: connect from a button or retry on a timer |
| 2 Gold paid only 100 (2026-11-04) | The note was written as a bare number but read with `JObject.Parse`, so the second Gold threw and was dropped | Read it with `JsonConvert.DeserializeObject<int>`. The write and read formats must match |
| Only one location with no `/` in its name could be stored (2026-11-04) | `itemName` stayed `""`, so the second such location threw on `Dictionary.Add`, and the rest of the list was never added | Fall back to the full name. Check: the log should say `696 Locations` |
| Reputation Prefix would throw during save load (2026-11-04, caught before running) | The prestige and relationship Prefixes also run during load, before login, when `allLocations` is empty and `session` is null | Guard with `UnlockItems.isLoaded` and use `TryGetValue` |
| Game closed while loading the save, no error logged, server never saw a connection (2026-11-07) | Endless loop: the `PlayerJson` constructor called `Loadfile()`, and `JsonConvert.DeserializeObject<PlayerJson>` calls the constructor again before filling the fields. A stack overflow closes the game instantly with no log | Load in a `static` method that returns what `DeserializeObject` gives back. Never load the file from the constructor |
| `goldCount` reset to 0 on every load, and Gold got paid again (2026-11-07) | `new PlayerJson()` never read the file, then `SaveFile()` after login wrote the 0 over the real number | Read the file back (`Loadfile`) |
| `static filepath = ...` would use the wrong slot after switching saves | A static field's initializer runs once, the first time the class is used | Use a `=>` property, which is worked out each time |
| Checks saved but empty again after restart | `allLocationsReached` was a `=>` property. Newtonsoft writes it but can't load into it (no setter) | Make it a normal field: `public HashSet<string> allLocationsReached = new HashSet<string>();` |
| 132 extra Gold (13,200) paid in the wrong-seed test (2026-11-07) | On a seed mismatch `Login()` returned without stopping, so it logged in again every frame. Each new session resent all items, `allItemsReceivedList` was never emptied, so the Gold count kept rising, and `UnlockQueue` still ran | `isCorrectServer` stops `Update`. Still to do: empty `allItemsReceivedList` per session. The test save has the extra gold |

---

## Next step when resuming

**Status as of 2026-11-07 (checked against the code, the server logs and the game DLL).** Test seed `AP_05499520754356937898`, slot `Coco`, game save slot 1 (`playerArchipelago_1.json`). ✅ = confirmed by a log, 🟡 = written but not confirmed, ❌ = not done. The test save has about 13,200 extra gold from the wrong-seed bug (see "Lessons learned").

| Area | State | Evidence |
|---|---|---|
| Connect, 696 location lookup | ✅ | Log |
| Receive fabrics and patterns | ✅ | Log |
| Gold, no repeat payouts across restarts | ✅ | Log: `Gold skipped` ×3, `goldCount` stays 3 |
| Gold, no repeat payouts across reconnects | ❌ | `allItemsReceivedList` is never emptied (see connection plan, step C1) |
| Per-slot JSON file, load and save | ✅ | `goldCount` and `seed` survive restarts |
| Seed check | ✅ | Wrong-seed test: one error, no Gold paid |
| Send all saved checks after login | 🟡 | `Plugin.cs:95-107`, not run yet |
| Delete JSON file when a save is deleted | 🟡 | `HarmonyClass.cs:35-44`, not run yet |
| Pattern checks | ✅ | Server log 2026-11-04 |
| Friendship checks | ✅ | Server log 2026-11-05: `Reach friendship: 1 with Prudence` |
| Fabric checks | 🟡 | Never in a server log |
| Reputation checks | 🟡 | Never in a server log |
| Story, pigeon and final commission checks | ❌ | Hook found: see "Commission hook" above |
| Mod reads slot_data, checks the goal, tells the server | ❌ | The apworld already sends the options (`world.py:81`) |
| No freezing, reconnecting, connection settings | ❌ | See connection plan |
| Native starter patterns and fabrics locked | 🟡 | The DLL shows they come only from `ApplyStartingUnlocks` and `ApplyUnlocksForPrestigeLevel`, which are both blocked. Not checked in the shop on a fresh save |
| Other unlock paths | ❌ | `UnlockHiddenItems` (fabrics), `YarnBridge.UnlockFabric` and `YarnBridge.UnlockPattern` (dialogue) add unlocks and aren't blocked. When the game calls them is unknown |

**When testing, use patterns and fabrics that haven't been used on this seed.** The server only logs a check the first time, so reused ones look like nothing happened.

### Connection plan (current focus, 2026-11-07)
Design decided 2026-11-06: see the plan's "PHASE 3b". Do these in order. Each one has a "Your job" and a "Check". Teach them one or two at a time, with farming-game examples only.

**C1. Empty the item list on each new session.**
- *Why:* every new session resends all items. `allItemsReceivedList` keeps the old ones, so Gold gets counted (and paid) twice. Fabrics and patterns are safe because they check "already unlocked".
- *Your job:* a small `internal static` method in `UnlockItems` that empties the list, called in `Login()` right after `CreateSession`. Alternative: count Gold from `session.Items.AllItemsReceived`, which belongs to the session and starts empty with each new one.
- *Check:* login logs `Gold skipped` ×3 and no `Gold given`. The real test is C6.

**C2. Test sending all saved checks.**
- *Your job:* close the game, add one unused location name (internal name, exactly as `RememberCheck` saves it) to `allLocationsReached` in the JSON file, then load.
- *Check:* `Added check for …` in the log, and the check in the server log without finishing a dress. Then finish a dress, restart, and its pieces should still be in the file and get resent.
- Small extra: `Added check for` is logged even when the name wasn't found. Log a warning for unknown names instead.

**C3. Test the DeleteSlot postfix.**
- *Your job:* start a new game in an empty slot (not slot 1!), load it once so its JSON file appears, go back to the title screen, and delete that save.
- *Check:* that slot's `playerArchipelago_<n>.json` is gone, and `playerArchipelago_1.json` is still there.

**C4. Switching saves.**
- *Why:* going back to the title screen resets `isLoaded`, but not `isConnected`, and the session stays open. Loading another save (maybe another seed) keeps the old connection, so the seed check never runs for the new save.
- *Your job:* on the title screen, disconnect the session, set `isConnected` back to `false`, and empty the item queue and list. Only do it once, not every frame.
- *Check:* load slot 1, go back to the title screen, load slot 1 again. The log should show a new `Connection Success!` and `Gold skipped` ×3, with no `Gold given`.

**C5. No freezing when the server is down.**
- *Why:* `TryConnectAndLogin` blocks the main thread until it gives up, and `Update` calls `Login()` every frame, so the game freezes and Windows closes it.
- *Your job, part 1 (retry delay):* only start an attempt when you're not connected, an attempt isn't already running (an "is connecting" bool), and a few seconds have passed since the last try. You already know the timer idea from plugin #1 (`Time.time` and a stored start time). `Time.realtimeSinceStartup` ignores game pauses.
- *Your job, part 2 (don't block):* the library has async versions, `session.ConnectAsync()` then `session.LoginAsync(...)`, checked in the MultiClient.Net 6.7.1 docs. `Login()` is already `async void`, so you can `await` them. In Unity, code after an `await` started on the main thread continues on the main thread, so `ShowNotification` stays safe. Not yet tested here.
- *Also:* each attempt makes a new session and adds `ItemReceived` again. Make sure only one session is ever hooked up (remove the handler from the old one, or reuse the session).
- *Check:* with the server off, load your save. The game stays playable, and `Failed` is logged every few seconds, not every frame. Start the server and it should connect within a few seconds.

**C6. Reconnect after a drop.**
- *Why:* nothing notices when the connection drops. `isConnected` stays `true`, and sends start failing.
- *Your job:* subscribe to `session.Socket.SocketClosed`. It runs on a background thread, so the handler should only set a flag (for example `isConnected = false`), with no Unity calls. C5's retry then reconnects on its own. Also:
  - The wrong-seed disconnect will also fire `SocketClosed`. Make sure it doesn't start retrying (`isCorrectServer` stays `false`).
  - Sends while disconnected throw `ArchipelagoSocketClosedException`. The check is already saved, so it's safe, but wrap sends in `try/catch` and log a warning, so the error isn't silently lost.
- *Check:* while playing, close the server window. The log says it disconnected, and the game keeps running. Finish a dress with new pieces (`First time reached` and the file updates). Restart the server: the mod reconnects by itself, the server log shows the offline checks, and you see `Gold skipped` with no `Gold given`.

**C7. Connection settings.**
- *Why:* host, port, slot name and password are hard-coded (`localhost`, `38281`, `Coco`, no password). archipelago.gg rooms also change port when they restart, so players need to edit them.
- *Your job (simplest first):* BepInEx config entries (`Config.Bind(...)` in `Awake`, giving `ConfigEntry<string>`/`<int>`). BepInEx writes `BepInEx\config\<GUID>.cfg`, which players edit. `TryConnectAndLogin`/`LoginAsync` take a `password` argument. Later, maybe in-game text fields with `GUI.TextField` and the Connect button.
- *Check:* set a wrong slot name in the `.cfg`, and the log shows a failed login without freezing. Set the right one and it connects.

**After the connection plan:** confirm fabric and reputation checks → two-dress test (temporary logging in `CheckFabrics` of fabric name, `placedOnMannequin` and `panelStates.Count`) → make dress checks go through the saved list only (and fix `CheckPatterns`' `allLocations[...]` and missing bodice/skirt null checks) → commission checks → goal.

**Other known mod issues:**
1. Gold is counted as paid when it's added, not when the game saves. Quitting without saving loses it. Accepted for now because it's only filler.
2. `CheckPatterns` still uses `allLocations[...]`, so one unknown name stops the fabric check for that dress. It also doesn't null-check the bodice and skirt.
3. `CheckFabrics` sends one message per panel, which is harmless but wasteful.
4. Cleanup: unused `givenGoldCount` (`UnlockItems.cs:19`) and `Plugin.seed` (`Plugin.cs:20`). Unused `using`s (`Newtonsoft.Json.Linq` and `UnityEngine.UIElements` in `UnlockItems.cs`, `UnityEngine.TextCore.Text` in `LocationChecks.cs`).
5. **Unexplained game errors:** `Sketchbook.UpdateTags` threw `KeyNotFoundException` for `Professional (ItemTag)`, and `Sketchbook.RecreateTexture` threw a null reference. Cause unknown. Not in the 2026-11-07 `Player.log`, but that only covers one run.
6. `[BepInDependency("yuu.dressmaker.mods")]` makes Dressmaker Mods required. Decide if that's wanted for release.
7. `world\dressmaker` in the mod repo still has stale `locations.py`, `world.py` and `data\dressmaker_dump.json`, so that copy won't load. Use `Archipelago\worlds\dressmaker`.
8. Fixed 2026-11-06/07: the `itemList` thread race (it's now only touched on the main thread in `UnlockQueue`), and the Gold file not being tied to a seed (now one file per slot with the seed inside).

**Not a bug:** "No Collar" / "No Sleeves" leave a `0` in the ID array, but the server ignores unknown IDs (`MultiServer.py:1222`), so it's only untidy. Sent checks don't need tracking per save either, because the server ignores repeats (`MultiServer.py:1221`).

**`StopUnlockMods` (purpose inferred, unconfirmed in ILSpy):** it appears to stop Dressmaker Mods from restoring earned fabrics, which would bypass the Archipelago fabric locks. The log shows the prefix runs during save load.

**To decide:** `StopUnlockFabrics` currently unlocks every accessory at once (`accessoryLevel = 50`), but the plan says accessories stay gated by reputation.

**🔀 Focus changed 2026-11-01 (later session): mod first, beatability logic later.** The black fabric task and the open apworld bugs below are parked.

**Mod roadmap (updated 2026-11-07):** 1. connect ✅ → 2. receive items ✅ (adds directly to `unlockedFabrics` / `unlockedPatterns`) → 3. send checks 🔧 (patterns ✅, friendship ✅; fabric and reputation written but never reached the server; commissions not started) → 4. goal ❌ → 5. connection handling 🔧 (**current focus**, moved ahead of 3 and 4 on 2026-11-06; see "Connection plan" above).

**Mod status (2026-11-01):**
- `Archipelago.MultiClient.Net` NuGet package added. `CopyLocalLockFileAssemblies` is on, and the post-build copy also copies `Archipelago.MultiClient.Net.dll` to `plugins`. Lesson: MSBuild target `Name`s can't contain `.`, and two targets with the same name override each other. The game's Newtonsoft is 13.0.2, which works with MultiClient.Net (don't ship a second Newtonsoft).
- ✅ A "Connect" button in `Plugin.cs` (host `localhost`, port `38281`, game `"Dressmaker"`, slot name all hard-coded) connects and logs in. The session is stored in a field. Test server: `python MultiServer.py` with the generated seed.
- ✅ (by 2026-11-04) Receiving items works: `session.Items.ItemReceived` fills a queue, which `Update` empties on the main thread once a save is loaded. See the 2026-11-04 status above for what's next.

**Status as of 2026-11-01: 🎉 the apworld generates seeds.** `python -c "import worlds.dressmaker"` loads cleanly, and `python Generate.py` with a Dressmaker YAML in `Players\` made a seed. The spoiler showed the right starting items (6/7/4/3 patterns + 23 fabrics). Suggested commit (unconfirmed whether done): `First generating Dressmaker apworld: items, locations, rules, options` (don't commit `output\` or `Players\`).

**Files in `worlds\dressmaker\` (all written by me):**

| File | State |
|---|---|
| `archipelago.json` | Done |
| `__init__.py` | Imports `DressmakerWorld` (AP loads the world) |
| `world.py` | Imports items/locations/regions/rules. `create_item` returns `items.DressmakerItem`. `fill_slot_data` sends `goal_choice`, `fabric_goal_percentage`, `pattern_goal_percentage` to the mod. Docstring/comments may still mention APQuest |
| `regions.py` | One region, `workshop` (`origin_region_name = "workshop"`) |
| `locations.py` | **696 locations** = 53 story + 24 pigeon + 155 "Use pattern: …" + 50 "Reach reputation level: N" + 50 "Reach friendship: N with <char>" + 363 "Use fabric: …" + 1 "Complete Final Commission". ID blocks: story 1000, pigeon 2000, pattern 3000, reputation 4000, friendship 5000, fabric 6000, final 8000. Each group is its own dict (`STORY_QUEST_LOCATIONS` etc.), unioned into `LOCATION_NAME_TO_ID`. All go in `workshop`. Victory event = `"Dressmaker goal reached"` → item `"Victory"`. Note: "Complete Final Commission" is inside `STORY_QUEST_LOCATIONS` (54 entries) |
| `items.py` | `PATTERN_ITEMS` (155, "Pattern: <prettyName>/<name>", IDs 1000+), `FABRIC_ITEMS` (363, "Fabric: <prettyName>/<name>", IDs 2000+), `FILLER_ITEMS = {"Gold": 50}`. `PATTERNS_BY_TYPE` (Bodice 59 / Skirt 46 / Sleeve 30 / Collar 20). Borrows `DRESSMAKER_DATA` from `.locations`. Patterns + fabrics = progression, Gold = filler. `create_all_items` precollects random starting patterns per type and starting fabrics from the options, pools the rest, fills with Gold |
| `rules.py` | Each "Use pattern: X" needs "Pattern: X", each "Use fabric: X" needs "Fabric: X" (built with `.replace` on item names; stops an item landing on its own use-location). Goal event needs `HasFromList(*PATTERN_ITEMS, count=…)`. Completion = `Has("Victory")`. Only the pattern goal has a rule; `goal_choice` is not read yet |
| `options.py` | `goal_choice` (pattern / fabric / main_story / all_commissions, default pattern), `pattern_goal_percentage` and `fabric_goal_percentage` (0–100, default 50), `starting_bodices` (3–30, 6), `starting_skirts` (3–30, 7), `starting_sleeves` (3–15, 4), `starting_collars` (3–10, 3), `starting_fabrics` (23–100, 23). Option groups "Goal Options" / "Starting Options". No presets (decided: not needed) |

**Open bugs / loose ends (fix first next session):**
1. ✅ **Fixed (checked 2026-11-07):** `rules.py` line 28 now computes `int(155 * (pct / 100))`. Dividing a `Range` option works (`Options.py:364`). `pattern_goal_percentage: 0` no longer crashes, but means "0 patterns needed" (an instant win). Still decide whether the range should start at 1.
2. `items.py` line 57 passes `world.options.starting_fabrics` without `.value`. It works (Range options act like ints), but `.value` is clearer and matches the pattern lines.
3. `goal_choice` isn't used in `rules.py` yet. Only the pattern goal works. Next: an `if` on the goal for `fabric` (same as patterns but with `FABRIC_ITEMS`). `main_story` / `all_commissions` have no rule yet (they need the pinned commission logic). Consider commenting them out of the Choice until then.
4. Small: `rules.py` imports `HasAll`/`Rule` unused. Leftover APQuest docstring and comments in `world.py` and other files. (The `StartingCollars` docstring is fixed.)
5. ~~Debug `print` in `locations.py`~~: gone (checked 2026-11-04).

**Parked task: the black fabric problem.** An early commission needs a black fabric, but the 23 random starting fabrics may not include one (the first test seed had none). Plan:
- **Step 0 (ILSpy, waiting on me):** find which commission it is, and its `completionRequirements`. What requirement type is it, and how does the game decide a fabric is "black" (a colour field on `Fabric`, a fabric list, a tag)? Names are unreliable: about 25 fabrics have black-sounding names (e.g. Black Cotton, Onyx Black Silk, Midnight Black Velvet, Ink Black Wool, Black Corduroy, Ebony Lace, Black Sequins) plus unclear ones (Charcoal Cotton, `WoolArgyleBlack` = "Grey Argyle", Black & White Polka Linen, Houndstooth Black).
- **Step 1 (quick fix):** in `items.py`, make a `BLACK_FABRICS` list, then in `create_all_items` sample 1 from it first, then sample `starting_fabrics - 1` from the fabrics not already picked.
- **Proper fix (later, pinned logic):** a rule on that commission's location, `HasAny(<black fabrics>)`, plus dumping fabric colours.

**Explained to me 2026-11-06/07 (don't re-explain unless asked):** how other Archipelago clients handle being offline (the server keeps items and resends them all on connect; the client keeps its checks and resends them all); `HashSet` and `.Add` returning `bool`; `static` vs instance methods; a constructor can't hand back a different object; `JsonConvert.DeserializeObject<T>` calls the constructor first; static field initializers run once vs `=>` properties; `=>` properties are saved but never loaded by Newtonsoft; `params long[]` and `List.ToArray()`; why one call with 400 checks is fast; why reconnecting double-counts Gold.

**Explained to me earlier (don't re-explain unless asked):** items = locations count; classifications (progression / useful / filler); `push_precollected` and taking starting items out of the pool; `world.random` (never Python's `random`); dict of lists; `.items()`, `+=` vs `.append`, `list(dict)`, `*` spreading, `in` / `not in`, `.replace`; `:` vs `=`; why use-locations need their item (an item can't sit on its own unlock); AP logic is "possible", not "how long", and AP handles chains (spheres) automatically, but almost no chains are modelled yet; slot_data is what the mod receives on connect; the mod (not the apworld) checks "used X%" and sends the goal.

**Decisions made this session:** gold is the only filler (reputation filler = nice to have); patterns/fabrics are NOT toggleable (no patternsanity/fabricsanity); no option presets; starting fabrics are random AP starting items (game's native starter fabrics get locked by the mod); starting patterns 6/7/4/3 by type.

**After the black fabric:** fix the open bugs above → fabric goal rule → maybe a `WebWorld` (so option groups show on the website) → a `test\` folder like APQuest's → then back to Phase 2 research (hooks) and the mod (Phase 3/5).

(Older plan below, from 2026-10-28. Steps 1–5 are effectively done.)

(Updated 2026-10-28, end of session.) The dump is finished and reviewed. Python 3.13 and the PyCharm venv are set up and working. **Now starting Step 6: the apworld.** I've never written an apworld and know little Python, so start from the basics.

**Already explained to me:** what an apworld is (items, locations, regions, rules, options, goal; it runs only at generation and never touches the game; the mod is the other half), using the farming game as the example. APQuest (`worlds\apquest\`) is the teaching example to copy the structure from: `items.py`, `locations.py`, `regions.py`, `rules.py`, `options.py`, `world.py`, `__init__.py`, `archipelago.json`, `test\`.

**Steps:**
1. **Confirm the Launcher runs:** in PyCharm's terminal (`(.venv)` showing), run `python Launcher.py` and press Enter to install modules if asked. *Check:* the Launcher window opens. (Unconfirmed whether I've done this yet. Ask me.)
2. **Commit** (GitHub Desktop) the `worlds\dressmaker` files on branch `DressmakerArchipelago`, plus the dump-plugin changes in the mod repo, if not done yet.
3. **Generate a test seed with APQuest** to see what generation does: Launcher → "Generate Template Options", copy APQuest's YAML into `Players\`, run `python Generate.py`, look at the output in `output\` (the spoiler log especially).
4. **Read APQuest together:** `!READ_FIRST!.txt` → `__init__.py` → `world.py` → `items.py` / `locations.py` → `test\__init__.py`. Explain the Python as we go.
5. **Start `worlds\dressmaker\`:** decide where the JSON lives (e.g. `worlds\dressmaker\data\`, read with `pkgutil.get_data`), then write the smallest possible world: patterns as items, one location type, no rules, `pattern_percentage` goal. Then grow it.
   - Item pool = 155 patterns (drop "No Collar"/"No Sleeves") + fabrics (unique names, see "Dump review").
   - Locations = commissions, relationship levels, reputation levels.
   - Access logic is **pinned** (see above). Keep it simple for now, but finish it before release.

Guide me one or two steps at a time. Ask me what I've already done, since I may have started some steps after this doc was written.

(Plugin #2, the gold button, is paused. If I come back to it, resume from its "Where I left off" item 1.)
