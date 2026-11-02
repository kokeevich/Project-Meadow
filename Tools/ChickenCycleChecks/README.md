# Chicken cycle checks

Run with .NET 8 SDK:

```sh
dotnet run --project Tools/ChickenCycleChecks/CycleChecks.csproj
```

Uses the actual ChickenCycle.cs source and checks 63 animal, dairy, production, route, objective, inventory and economy checks without Unity.
Unity EditMode tests also cover the cycle and single-serving food consumption.

## Unity smoke check

Open MVP_Farm and enter Play mode. Two procedural placeholder chickens should appear.
Click the brown Food Plot; after five seconds it becomes ready. One chicken should approach,
consume the serving, eat for two seconds and produce one egg after another five seconds.
Plant again to repeat. Both chickens must not consume the same serving. The HUD shows states
and egg counts. The temporary Consume button intentionally removes ready food manually.
Click an egg: its counter increases, a short chime plays, and it shrinks upwards. At 10 stored items, further clicks must leave eggs in the world. Repeated clicks during collection must not duplicate items. Confirm Console has no errors, and re-enter Play mode
once to check NavMesh cleanup. NavMesh and visuals require Unity and are not tested by the CLI.

## Asset availability

Kenney Farm Kit and Food Kit are not included. On 2026-11-01 their official pages returned HTTP 403 after a successful proxy connection. No third-party archives or licenses were downloaded. The collection chime is synthesized by project code.

## Economy smoke check

Collect five eggs, open Market or click the truck, select five eggs and send it.
Storage frees immediately; coins stay at zero until the five-second trip completes, then become 25.
Buy a chicken: the balance becomes zero and a third chicken joins the farm.
Repeat up to the explicit limit of 12 chickens. Invalid sales and purchases must be disabled.
Visual redesign is the next milestone before Phase 6.

## First level smoke check (Unity)

Play MVP_Farm. Follow the garden/collection/truck hints; hide them if desired.
Collect and deliver ten eggs (one full truck or multiple partial trips).
The objective must advance only on truck return. Victory should display once,
freeze the farm, stop the background melody and play the success sound.
Click Play Again: storage, coins, sales, objective, tutorial and chickens reset;
NavMesh and input must still work and music must restart. Mute should persist.
Repeat with sound disabled. Inspect Console throughout.

The CLI tests cannot validate scene reload, Unity audio or the victory UI.

## Production and expanded map smoke check (Unity)

- Move right using WASD/arrows/middle mouse or two fingers. Bounds must stop the camera.
- Tap the wheat patch by the mill, wait five seconds, tap to harvest two grain.
- Open the mill through its model or bottom button. Queue one flour (six seconds).
- Queue three jobs; verify the fourth is rejected without consuming more grain.
- Fill storage while milling: completed flour must remain pending until a sale frees space.
- Use the market tabs for grain/flour; check prices 2/14 and that egg goal does not advance.
- Upgrade for 20 and 40 coins: subsequent jobs take 4.8 and 4 seconds; an active job keeps its duration.
- Watch the truck leave, disappear during delivery and return facing the farm.
- Replant chicken food and watch both hens over several cycles. One reserves each serving.
- Check facial features follow the head, and gestures over UI do not move the world or plant crops.
- Restart after victory; all inventory, queues, upgrades and navigation must reset.

Scene navigation, input, animation and sound still require Unity; the CLI tests do not cover them.

## Cow and dairy smoke check (Unity)

- Start the scene: two chickens and one cow appear, and HUD counts show 2 / 1.
- Plant the separately labelled cattle grass between the food patch and wheat patch.
- Grass grows for eight seconds. Only the cow should approach it; chickens keep their own food.
- After eating for three seconds and waiting ten seconds, the cow leaves a milk can.
- Click milk to collect it. Repeat to obtain two milk.
- Open Production, select the cheese maker, queue cheese and wait eight seconds.
- Mill and dairy should run at the same time, with independent queue/upgrade state.
- Sell milk (8) or cheese (24). Neither should advance the egg objective.
- Check labels and five market tabs at 1024x576 and 1920x1080.
- Restart: cow, feed, products and both production queues reset.

The imported prefab reference test is supplied in DairyTests for Unity EditMode.
CLI tests validate the cycle/economy, not animal navigation, visual animation or Unity import.
