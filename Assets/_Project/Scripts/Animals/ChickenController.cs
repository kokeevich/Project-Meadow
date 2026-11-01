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
        private NavMeshAgent agent;
        private ChickenCycle cycle;
        private FoodPlot target;
        private Transform body;
        private float nextSearch;
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
            body = Part("Body", PrimitiveType.Sphere, new Vector3(0, .45f, 0), new Vector3(.65f, .7f, .8f), bodyMaterial);
            Part("Head", PrimitiveType.Sphere, new Vector3(0, .8f, .35f), Vector3.one * .4f, bodyMaterial);
            Part("Beak", PrimitiveType.Cube, new Vector3(0, .78f, .61f), new Vector3(.18f, .12f, .23f), beakMaterial);
            Part("Comb", PrimitiveType.Cube, new Vector3(0, 1.04f, .35f), new Vector3(.12f, .18f, .25f), beakMaterial);
        }
        private Transform Part(string label, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(shape);
            part.name = label;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(part.GetComponent<Collider>());
            return part.transform;
        }
        private void Update()
        {
            if (cycle == null || !agent.isOnNavMesh) return;
            if (cycle.Tick(Time.deltaTime)) SpawnEgg();
            bool eating = cycle.State == ChickenCycle.Phase.Eat;
            float bob = eating ? Mathf.Sin(Time.time * 12f) * .06f : agent.velocity.magnitude > .1f ? Mathf.Sin(Time.time * 10f) * .04f : 0;
            body.localPosition = new Vector3(0, .45f + bob, 0);
            body.localRotation = Quaternion.Euler(eating ? 18f + Mathf.Sin(Time.time * 12f) * 12f : 0, 0, 0);
            if (cycle.State != ChickenCycle.Phase.Idle) return;
            if (target != null)
            {
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
                if (plot.State != FoodPlot.GrowthState.Ready) continue;
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
            if (nearest != null && agent.SetDestination(destination)) target = nearest;
        }
        private void StopSeeking()
        {
            target = null;
            agent.ResetPath();
        }
        private void SpawnEgg()
        {
            var egg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            egg.name = "Egg";
            egg.transform.position = transform.position - transform.forward * .6f + Vector3.up * .4f;
            egg.transform.localScale = new Vector3(.25f, .35f, .25f);
            egg.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
            var physics = egg.AddComponent<Rigidbody>();
            physics.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            egg.AddComponent<ProjectMeadow.Items.WorldItem>().Initialize(eggData,
                FindFirstObjectByType<ProjectMeadow.Inventory.InventoryController>());
            EggsProduced++;
        }
    }
}
