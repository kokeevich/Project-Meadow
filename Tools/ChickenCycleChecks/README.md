# Chicken cycle checks

Run with .NET 8 SDK:

```sh
dotnet run --project Tools/ChickenCycleChecks/CycleChecks.csproj
```

Uses the actual ChickenCycle.cs source and checks seven production invariants without Unity.
Unity EditMode tests also cover the cycle and single-serving food consumption.

## Unity smoke check

Open MVP_Farm and enter Play mode. Two procedural placeholder chickens should appear.
Click the brown Food Plot; after five seconds it becomes ready. One chicken should approach,
consume the serving, eat for two seconds and produce one egg after another five seconds.
Plant again to repeat. Both chickens must not consume the same serving. The HUD shows states
and egg counts. The temporary Consume button intentionally removes ready food manually.
Egg collection belongs to Phase 4. Confirm Console has no errors, and re-enter Play mode
once to check NavMesh cleanup. NavMesh and visuals require Unity and are not tested by the CLI.
