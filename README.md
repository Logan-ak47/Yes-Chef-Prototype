# Yes Chef!

Top-down 3D kitchen prototype. Built as a dev test for Tentworks Interactive.

## Running
- Unity 2022 LTS, URP.
- Open `Assets/_Project/Scenes/Kitchen.unity` and press Play.

## Controls
- WASD / Arrow keys / Left stick: move.
- Walk into stations to interact automatically.
- Esc: pause.

## Design notes
- I used ScriptableObjects for ingredient, station, and game settings so tuning stays out of gameplay scripts.
- `IInteractable` is the single interaction contract between the player and stations.
- ScriptableObject event channels carry score, timer, order, and state changes to the UI.
- `GameManager` owns the top-level flow for menu, running, paused, and game over.
- High score persistence sits behind `IHighScoreStore`, with `PlayerPrefs` as the current store.

## Adding a new ingredient
1. Create a new `IngredientDefinition` asset in `Assets/_Project/Data/Ingredients/`.
2. Assign its icon, raw/prepared visuals, score value, prep type, and prep duration.
3. Add it to `DefaultOrderSettings.orderableIngredients`.
4. Duplicate a refrigerator slot in `Kitchen` and assign the new ingredient asset.
5. Press Play and verify it appears in orders and can be delivered.

## Adding a new station
1. Create or update a `StationDefinition` asset for the station type and slot count.
2. Add a new `MonoBehaviour` that implements `IInteractable`.
3. Use `IngredientDefinition.RequiredPreparation` and station data instead of ingredient-specific checks.
4. Place the station in `Kitchen` with its collider, visuals, and any timer or highlight objects it needs.
5. Wire any scene references the same way the existing table and stove are wired.

## Known limitations
- Score floaters are spawned per event and are not pooled.
- The highlight feedback is simple.
- The project uses a single playable scene and a short manual tutorial flow.

## Time
Approx 8 hours.
