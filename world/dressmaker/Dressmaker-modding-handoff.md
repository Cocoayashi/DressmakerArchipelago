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
- You can't see Dressmaker's code. Anything game-specific comes from what I paste from ILSpy. Say so when you're guessing.
- **Hand-hold me.** I often don't know what to do next. Break work into small numbered steps, each with a **"Check:"** (how I know it worked) and a "Your job:" line. Give me one or two steps at a time, then wait for me to paste results or errors.
- Example code is welcome as long as it's the made-up farming game (or similar), never my actual solution.
- **I know very little Python and nothing about apworlds** (as of 2026-10-28). Explain Python concepts as they come up, comparing them to C# where it helps.
- **I do all git work myself in GitHub Desktop** (the `git` command isn't on PATH). Don't run git; tell me what to commit and with what message.

---

## Current focus (read with `dressmaker_ap_plan.txt`)

The real goal is a **Dressmaker Archipelago** (multiworld randomizer). The full project plan, game facts, research findings and design decisions are in:
`C:\Users\lolli\OneDrive\Desktop\Archipelago\worlds\dressmaker\dressmaker_ap_plan.txt` (a copy may also exist in the mod repo; the one in the Archipelago repo is the up-to-date one)
**Read that file first.** This handoff covers how to teach me and where I am right now.

| Item | Location |
|---|---|
| Mod project (BepInEx plugin, own repo) | `C:\Users\lolli\OneDrive\Desktop\DressmakerArchipelago\` (`Plugin.cs`, `DressmakerArchipelago.csproj`, `.sln`) |
| Mod `.csproj` | Already references `Assembly-CSharp.dll` and `Framework.dll`, and has a post-build copy target to `<game>\BepInEx\plugins` |
| `Plugin.cs` | The data dump plugin. An `OnGUI` button ("Generate APWorld") writes `<game>\BepInEx\plugins\dressmaker_dump.json`. Data classes are in `StoryQuestInfo.cs` |
| Archipelago repo (Python, apworld goes here) | `C:\Users\lolli\OneDrive\Desktop\Archipelago`, branch `DressmakerArchipelago`. `worlds\dressmaker\` (lowercase) holds this handoff, the plan and `dressmaker_dump.json`. No Python files yet |
| git for the mod repo | Initialized (`.git` and `.gitignore` exist as of 2026-10-28) |
| Python IDE | **PyCharm** (paid), with the Archipelago folder opened as the project |
| Python for Archipelago | **3.13.17** in a venv at `Archipelago\.venv` (PyCharm interpreter "Python 3.13 (Archipelago)"). The repo only accepts **3.12.10 to 3.13.x** (checked in `ModuleUpdate.py`); 3.14/3.15 won't run. Python 3.7.9 is also installed but unusable for this. In PyCharm's terminal (Alt+F12, prompt shows `(.venv)`) use plain `python`; outside the venv use `py -3.13` |

**Plugin #2 (gold button) is paused.** Its notes are kept below because the `PlayerProgress.Load` postfix idea is reusable.

### Next task: a "data dump" plugin

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
| Fabrics (`Fabric.AllFabrics`) | **564** in the base game. Only `name` and `prettyName` so far |
| Patterns | **157** = 117 from relationship levels (`character.GetPatternsUnlockedAtLevel(0..5)`) + 40 starters (`PlayerProgress.Current.unlockedPatterns` on a fresh save). Each has `name`, `prettyName`, `PatternType` and `itemTags` (from `garment.additiveTags`, 20 different tags, weights -10 to 25) |

**Dump review (2026-10-28), `worlds\dressmaker\dressmaker_dump.json`:**
- Top-level keys: `storyQuestInfos` (53), `sideQuestInfos` (24), `finalQuestInfo`, `fabricInfos` (564), `patternInfos` (157). Quests have `name`, `prettyName`, `questGiverCharacterName`. Fabrics have `name`, `prettyName`, `itemTags` (tag → weight). Patterns have `PatternType`, `name`, `prettyName`, `itemTags`.
- ✅ No `null`s anywhere, no modded content, none of the 21 cut patterns (so no filter is needed for them). `No Collar` and `No Sleeves` are present and must be dropped.
- Patterns: 59 bodices, 46 skirts, 31 sleeves, 21 collars. Pattern and quest names are all unique. No pattern name matches a fabric name.
- Story commission givers: Prudence 7, Priya 6, everyone else 5. Each character has 2–3 side quests.
- Pattern `name` and `prettyName` often differ (e.g. `Ruffle Bodice` = "Off-Shoulder Begonia Bodice", `V collar scalloped` = "Scalloped Collar"). **Decide** which to use for AP item/location names, and use the same one in the mod and the apworld.
- **70 fabrics have empty `itemTags`** (mostly beaded, brocade, burnout, dupioni and print fabrics). Check in ILSpy whether their tags are stored elsewhere. This matters for the tag-score logic.
- One fabric is named `_CottonVictorianPattern` ("Victorian Cotton Print"). The leading `_` may mean a template or test asset. Check in-game that it's a real shop fabric before putting it in the item pool.
- The dump doesn't record which 40 patterns are the native starters. Add a field if it's needed for removing the native starters on a fresh save.
- Pattern tags (20): Casual, Cool, Cute, Daywear, Eclectic, Elaborate, Elegant, Eveningwear, Flowers, Formal, Glamour, Goth, Playful, Professional, Risque, Romantic, Simple, Uncomfortable, Warm, Workwear. Fabrics have those plus **Patterned** and **Shimmering** (22).
- **To commit:** the dump/plan/handoff in the Archipelago repo, and the dump-plugin fixes in the mod repo (via GitHub Desktop). Unknown whether this was done.

**Facts learned:**
- `FinalQuest` unlocks (knighthood letter, `ShouldPresentKnighthoodLetter()`) once **every** quest in `ActiveCampaign.quests` is completed. Side quests don't count. Completing `FinalQuest` rolls the credits, so it's a clean `main_story` victory event.
- **155 obtainable patterns** = 157 minus `No Collar` and `No Sleeves`. That matches the wiki's 155. The "No" pieces are always available and must **not** be AP items.
- **21 base-game patterns are unobtainable (cut content)** and must not be in the item pool: Black Widow, Carmilla, Cinched Blouse, Elevated Ruffled Square, Empire Waist Princess, Ruffled Square, Sweet Maiden and Widow bodices; Bo Peep, Empire Princess, Evie, Frill Wrap, Handkerchief Hem, Junon, Petal Bow, Spiral, Venus and Widow skirts; Dove, Galadriel and Half Bishop sleeves. (`BodiceDefinition.AllBodiceDefinitions` is the same 67 bodices as in `AllGarmentComponents`, so it doesn't filter anything.)
- Which character and level originally gave a pattern does **not** matter for the apworld: relationship levels are locations ("Rose Level 3"), and patterns are granted with `UnlockPattern(name)`.
- 3 pairs of fabrics share a display name (`Silk Bone`/`Silk Van Dyke Brown`, `Silk Camel`/`Silk Smoke Grey`, `Silk Brocade Pink Pistacchio`/`Silk Pacifc Sunset`). Use `name` as the key, or add a suffix, because AP item names must be unique.

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
From the plan (Phase 2): the dress-complete hook is `CheckAchievements()` (it runs when a dress is completed, related to the achievements), and the current dress is `SingletonBehaviour<GameManager>.Instance.activeDress`, which contains the list of pattern pieces used. Phase 3 plans a Harmony Postfix there.
**Not yet recorded:** which class `CheckAchievements()` is in, whether it takes parameters, and the name of the pattern-piece list field inside `activeDress`. Look these up in ILSpy and add them to the plan.

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

---

## Next step when resuming

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
