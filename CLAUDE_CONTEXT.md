---
# Yes Chef! — Unity Dev Test

## Mission
Tentworks Interactive dev test. Overcooked-lite 3D top-down kitchen game, 4–6 hour scope. 
Graded on clean architecture, scalable design, and proper Unity practices — NOT visual polish.

## Architecture Contract
- Data-driven: IngredientDefinition and StationDefinition SOs drive all gameplay data.
- State machines for GameManager, IngredientInstance, OrderInstance, preparation stations.
- IInteractable interface is the ONLY way the player communicates with stations.
- Proximity auto-interact: on trigger enter, evaluate (player hand state, station state) 
  and execute the legal action. No key press required for station interactions.
- Event-driven UI via ScriptableObject event channels (e.g., ScoreChangedChannel, 
  OrderCompletedChannel). UI subscribes; gameplay raises. No direct references from 
  gameplay code to UI.
- HighScoreService wraps PlayerPrefs behind an interface for future swap.
- Singletons avoided. GameManager is the one bootstrap; everything else is injected or 
  listens to channels.

## Coding Conventions
- Namespaces: YesChef.Core, YesChef.Gameplay, YesChef.Data, YesChef.UI, YesChef.Services
- One class per file, PascalCase file name.
- Private fields: _camelCase with [SerializeField] for inspector exposure.
- Public mutable fields forbidden — use properties, events, or SO channels.
- No magic numbers in gameplay code — pull from SOs or named consts.

## Unity Setup
- Unity 2022 LTS, URP.
- New Input System (Unity.InputSystem) with a single PlayerInput.inputactions asset.
- Player: Rigidbody (isKinematic=false, freezeRotation on X,Y,Z), capsule collider.
- Camera: perspective, tilted top-down, orthographic optional. Static.

## Scope Locked
- Ingredients: Vegetable (chop 2s, value 20), Cheese (no prep, 10), Meat (cook 6s, 30).
- Orders: 50/50 chance 2 vs 3 ingredients, fully random with duplicates allowed.
- Score per delivered order: sum(ingredient values) - floor(elapsed seconds since order 
  opened). Can go negative.
- Max 4 concurrent orders, 5 seconds respawn on empty window.
- Game length: 180 seconds.

## Polish Targets (ship if time allows, skip if squeezed)
- Ingredient visible in player's hand socket (child transform).
- In-range station highlighted via material emission.
- World-space canvas timer bars above Table and Stove slots.
- Score "+N" / "-N" floaters above completed/expired windows.
- Start panel listing controls.
- Pause menu, Quit button.
- "New High Score!" celebration on game over.

## Folder Discipline
Assets/_Project/ contains ALL project assets. Nothing in Assets/ root except _Project/ 
and default Unity folders.

## Testing Discipline
Each phase completes only after its manual test script passes.
---
