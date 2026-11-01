# Chicken cycle checks

Run with .NET 8 SDK:

```sh
dotnet run --project Tools/ChickenCycleChecks/CycleChecks.csproj
```

Uses the actual ChickenCycle.cs source and checks 26 inventory, economy and production checks without Unity.
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
