using UnityEngine;
using UnityEngine.AI;
using ProjectMeadow.Farming;

namespace ProjectMeadow.Animals
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ChickenController : MonoBehaviour
    {
        [SerializeField] private AnimalData data;
        [SerializeField] private ProjectMeadow.Items.ItemData eggData;
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material beakMaterial;
        [SerializeField] private Material combMaterial, eyeMaterial, wingMaterial;
        private Transform head, leftFoot, rightFoot;
        private NavMeshAgent agent;
        private ChickenCycle cycle;
        private FoodPlot target;
        private Transform body;
        private float nextSearch;
        private float seekDeadline;
        public string State => cycle == null ? "Idle" : cycle.State != ChickenCycle.Phase.Idle
            ? cycle.State.ToString() : target != null ? "MoveToFood" : "Idle";
        public int EggsProduced { get; private set; }
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (data == null) { Debug.LogError("Chicken needs AnimalData.", this); enabled = false; return; }
            agent.speed = data.moveSpeed;
            agent.radius = 0.25f;
            agent.height = 1f;
            agent.stoppingDistance = 0.1f;
            cycle = new ChickenCycle(data.eatSeconds, data.productionSeconds);
            body = Part("Body", PrimitiveType.Sphere, new Vector3(0, .5f, 0), new Vector3(.7f, .7f, .85f), bodyMaterial);
            head = Part("Head", PrimitiveType.Sphere, new Vector3(0, .83f, .36f), Vector3.one * .43f, bodyMaterial);
            Part("Beak", PrimitiveType.Sphere, new Vector3(0, .79f, .62f), new Vector3(.19f, .13f, .25f), beakMaterial);
            for (int i = 0; i < 3; i++)
                Part("Comb", PrimitiveType.Sphere, new Vector3(0, 1.06f, .23f + i * .11f), new Vector3(.13f, .2f, .14f), combMaterial);
            Part("Wattle", PrimitiveType.Sphere, new Vector3(0, .66f, .54f), new Vector3(.12f, .19f, .12f), combMaterial);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Eye", PrimitiveType.Sphere, new Vector3(side * .19f, .88f, .47f), Vector3.one * .07f, eyeMaterial);
                Part("Eye glint", PrimitiveType.Sphere, new Vector3(side * .215f, .9f, .49f), Vector3.one * .024f, bodyMaterial);
                Part("Wing", PrimitiveType.Sphere, new Vector3(side * .31f, .5f, -.03f), new Vector3(.16f, .38f, .53f), wingMaterial);
                var foot = Part("Foot", PrimitiveType.Cube, new Vector3(side * .16f, .08f, .05f), new Vector3(.13f, .1f, .28f), beakMaterial);
                if (side < 0) leftFoot = foot; else rightFoot = foot;
            }
            var tail = Part("Tail feathers", PrimitiveType.Sphere, new Vector3(0, .68f, -.39f), new Vector3(.35f, .49f, .2f), wingMaterial);
            tail.localRotation = Quaternion.Euler(-32, 0, 0);
        }
        private Transform Part(string label, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(shape);
            part.name = label;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            if ((label == "Beak" || label == "Comb" || label == "Wattle" || label == "Eye" || label == "Eye glint") && head != null)
                part.transform.SetParent(head, true);
            if ((label == "Wing" || label == "Tail feathers") && body != null)
                part.transform.SetParent(body, true);
            return part.transform;
        }
        private void Update()
        {
            if (cycle == null || !agent.isOnNavMesh) return;
            if (cycle.Tick(Time.deltaTime)) SpawnEgg();
            bool eating = cycle.State == ChickenCycle.Phase.Eat;
            float bob = eating ? Mathf.Sin(Time.time * 7f) * .025f : agent.velocity.magnitude > .1f ? Mathf.Sin(Time.time * 8f) * .02f : 0;
            body.localPosition = new Vector3(0, .5f + bob, 0);
            body.localRotation = Quaternion.Euler(eating ? 6f + Mathf.Sin(Time.time * 7f) * 4f : 0, 0, 0);
            head.localPosition = new Vector3(0, .83f + (eating ? bob * 2 : bob), .36f + (eating ? .07f : 0));
            float step = agent.velocity.magnitude > .1f ? Mathf.Sin(Time.time * 8f) * .045f : 0;
            leftFoot.localPosition = new Vector3(-.16f, .08f + Mathf.Max(0, step), .05f + step);
            rightFoot.localPosition = new Vector3(.16f, .08f + Mathf.Max(0, -step), .05f - step);
            if (cycle.State != ChickenCycle.Phase.Idle) return;
            if (target != null)
            {
                if (Time.time > seekDeadline) { StopSeeking(); return; }
                if (target.State != FoodPlot.GrowthState.Ready) { StopSeeking(); return; }
                if (agent.pathPending) return;
                if (agent.pathStatus != NavMeshPathStatus.PathComplete) { StopSeeking(); return; }
                if (agent.remainingDistance <= agent.stoppingDistance + .15f)
                {
                    // TryConsume is atomic on Unity's main thread: two chickens cannot eat one serving.
                    if (target.TryConsume()) cycle.BeginMeal();
                    StopSeeking();
                }
                return;
            }
            if (Time.time < nextSearch) return;
            nextSearch = Time.time + Mathf.Max(.1f, data.searchInterval);
            FoodPlot nearest = null;
            Vector3 destination = default;
            float best = float.PositiveInfinity;
            foreach (var plot in FindObjectsByType<FoodPlot>(FindObjectsSortMode.None))
            {
                if (!plot.IsAvailableFor(this)) continue;
                var collider = plot.GetComponent<Collider>();
                if (collider == null) continue;
                Vector3 point = collider.ClosestPoint(transform.position);
                point.y = transform.position.y;
                if (!NavMesh.SamplePosition(point, out var hit, 1f, agent.areaMask)) continue;
                float distance = (hit.position - transform.position).sqrMagnitude;
                if (distance >= best) continue;
                var path = new NavMeshPath();
                if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                nearest = plot; destination = hit.position; best = distance;
            }
            if (nearest != null && nearest.TryReserve(this))
            {
                if (agent.SetDestination(destination)) { target = nearest; seekDeadline = Time.time + Mathf.Max(8, Vector3.Distance(transform.position, destination) / Mathf.Max(.1f, agent.speed) + 4); }
                else nearest.Release(this);
            }
        }
        private void StopSeeking()
        {
            if (target != null) target.Release(this);
            target = null;
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.ResetPath();
        }
        private void OnDisable() => StopSeeking();
        private void SpawnEgg()
        {
            var egg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            egg.name = "Egg";
            egg.transform.position = transform.position - transform.forward * .6f + Vector3.up * .4f;
            egg.transform.localScale = new Vector3(.32f, .43f, .32f);
            egg.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
            var physics = egg.AddComponent<Rigidbody>();
            physics.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            egg.AddComponent<ProjectMeadow.Items.WorldItem>().Initialize(eggData,
                FindFirstObjectByType<ProjectMeadow.Inventory.InventoryController>());
            EggsProduced++;
        }
    }
}
