using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using ProjectMeadow.Farming;

namespace ProjectMeadow.Animals
{
    [MovedFrom(true, sourceNamespace: "ProjectMeadow.Animals", sourceAssembly: "ProjectMeadow.Runtime", sourceClassName: "FarmAnimalController")]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ChickenController : MonoBehaviour
    {
        [SerializeField] private AnimalData data;
        [FormerlySerializedAs("eggData")]
        [SerializeField] private ProjectMeadow.Items.ItemData productData;
        [SerializeField] private bool isCow;
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material beakMaterial;
        [SerializeField] private Material combMaterial, eyeMaterial, wingMaterial;
        [SerializeField] private Material milkMaterial, milkAccent;
        private Transform head, leftFoot, rightFoot;
        private readonly System.Collections.Generic.List<Transform> cowLegs = new System.Collections.Generic.List<Transform>();
        private readonly System.Collections.Generic.List<Vector3> cowLegPositions = new System.Collections.Generic.List<Vector3>();
        private NavMeshAgent agent;
        private ChickenCycle cycle;
        private FoodPlot target;
        private Transform body;
        private float nextSearch;
        private float seekDeadline;
        public string State => cycle == null ? "Idle" : cycle.State != ChickenCycle.Phase.Idle
            ? cycle.State.ToString() : target != null ? "MoveToFood" : "Idle";
        public int ProductsProduced { get; private set; }
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (data == null) { Debug.LogError("Animal needs AnimalData.", this); enabled = false; return; }
            agent.speed = data.moveSpeed;
            agent.radius = isCow ? .45f : .25f;
            agent.height = isCow ? 1.5f : 1f;
            agent.stoppingDistance = 0.1f;
            cycle = new ChickenCycle(data.eatSeconds, data.productionSeconds);
            if (isCow) { BuildCow(); return; }
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
        private void BuildCow()
        {
            body = Part("Cow body", PrimitiveType.Sphere, new Vector3(0, .85f, 0), new Vector3(1, .85f, 1.6f), bodyMaterial);
            head = Part("Cow head", PrimitiveType.Sphere, new Vector3(0, 1.04f, .9f), new Vector3(.63f, .68f, .65f), bodyMaterial);
            var muzzle = Part("Muzzle", PrimitiveType.Sphere, new Vector3(0, .88f, 1.2f), new Vector3(.64f, .36f, .38f), beakMaterial);
            muzzle.SetParent(head, true);
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = Part("Cow eye", PrimitiveType.Sphere, new Vector3(side * .24f, 1.15f, 1.13f), Vector3.one * .09f, eyeMaterial); eye.SetParent(head, true);
                var ear = Part("Cow ear", PrimitiveType.Sphere, new Vector3(side * .4f, 1.2f, .85f), new Vector3(.35f, .14f, .2f), eyeMaterial); ear.SetParent(head, true);
                var horn = Part("Horn", PrimitiveType.Cylinder, new Vector3(side * .23f, 1.42f, .8f), new Vector3(.09f, .14f, .09f), wingMaterial); horn.SetParent(head, true);
                var spot = Part("Coat patch", PrimitiveType.Sphere, new Vector3(side * .45f, .9f, -.2f), new Vector3(.14f, .46f, .65f), eyeMaterial); spot.SetParent(body, true);
                for (int end = -1; end <= 1; end += 2)
                {
                    var leg = Part("Cow leg", PrimitiveType.Capsule, new Vector3(side * .33f, .3f, end * .5f), new Vector3(.17f, .3f, .17f), bodyMaterial);
                    cowLegs.Add(leg); cowLegPositions.Add(leg.localPosition);
                    var hoof = Part("Hoof", PrimitiveType.Sphere, new Vector3(side * .33f, .08f, end * .5f), new Vector3(.23f, .16f, .26f), eyeMaterial); hoof.SetParent(leg, true);
                }
            }
            Part("Tail", PrimitiveType.Cylinder, new Vector3(0, .6f, -.85f), new Vector3(.07f, .35f, .07f), eyeMaterial);
        }
        private void AnimateCow(bool eating)
        {
            float speed = agent.velocity.magnitude;
            body.localPosition = new Vector3(0, .85f + (speed > .1f ? Mathf.Sin(Time.time * 5) * .015f : 0), 0);
            head.localPosition = new Vector3(0, eating ? .8f + Mathf.Sin(Time.time * 4) * .035f : 1.04f, .9f);
            head.localRotation = Quaternion.Euler(eating ? 20 : 0, 0, 0);
            for (int i = 0; i < cowLegs.Count; i++)
            {
                float step = speed > .1f ? Mathf.Sin(Time.time * 5 + (i == 0 || i == 3 ? 0 : Mathf.PI)) * .08f : 0;
                cowLegs[i].localPosition = cowLegPositions[i] + new Vector3(0, Mathf.Max(0, step) * .4f, step);
            }
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
            if (cycle.Tick(Time.deltaTime)) SpawnProduct();
            bool eating = cycle.State == ChickenCycle.Phase.Eat;
            if (isCow) AnimateCow(eating);
            else
            {
            float bob = eating ? Mathf.Sin(Time.time * 7f) * .025f : agent.velocity.magnitude > .1f ? Mathf.Sin(Time.time * 8f) * .02f : 0;
            body.localPosition = new Vector3(0, .5f + bob, 0);
            body.localRotation = Quaternion.Euler(eating ? 6f + Mathf.Sin(Time.time * 7f) * 4f : 0, 0, 0);
            head.localPosition = new Vector3(0, .83f + (eating ? bob * 2 : bob), .36f + (eating ? .07f : 0));
            float step = agent.velocity.magnitude > .1f ? Mathf.Sin(Time.time * 8f) * .045f : 0;
            leftFoot.localPosition = new Vector3(-.16f, .08f + Mathf.Max(0, step), .05f + step);
            rightFoot.localPosition = new Vector3(.16f, .08f + Mathf.Max(0, -step), .05f - step);
            }
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
                if (plot.Kind != (isCow ? FoodPlot.FeedKind.Cattle : FoodPlot.FeedKind.Poultry) || !plot.IsAvailableFor(this)) continue;
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
        private Vector3 ProductPosition()
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            for (int ring = 0; ring < 3; ring++) for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2 / 12;
                var offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (1.1f + ring * .5f);
                if (!NavMesh.SamplePosition(transform.position + offset, out var hit, .5f, filter)) continue;
                if (hit.position.z < -1.9f) continue;
                Vector3 candidate = hit.position + Vector3.up * .4f;
                if (!Physics.CheckSphere(candidate, .28f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    return hit.position + Vector3.up * .08f;
            }
            // The agent itself is on walkable pasture; keep overflow out of the road as well.
            return transform.position + Vector3.up * .08f;
        }
        private void SpawnProduct()
        {
            GameObject product;
            if (isCow)
            {
                product = new GameObject("Milk");
                product.AddComponent<ProjectMeadow.Items.MilkCanVisual>().Build(milkMaterial != null ? milkMaterial : bodyMaterial, milkAccent != null ? milkAccent : wingMaterial);
                product.transform.position = ProductPosition();
            }
            else
            {
                product = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                product.name = "Egg";
                product.transform.position = transform.position - transform.forward * .6f + Vector3.up * .4f;
                product.transform.localScale = new Vector3(.32f, .43f, .32f);
                product.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
            }
            var physics = product.AddComponent<Rigidbody>();
            physics.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (isCow) physics.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            product.AddComponent<ProjectMeadow.Items.WorldItem>().Initialize(productData,
                FindFirstObjectByType<ProjectMeadow.Inventory.InventoryController>());
            ProductsProduced++;
        }
    }
}
