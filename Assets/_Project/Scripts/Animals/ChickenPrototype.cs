using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace ProjectMeadow.Animals
{
    public sealed class ChickenPrototype : MonoBehaviour
    {
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private GameObject cowPrefab;
        public int CowCount { get; private set; }
        public int ChickenCount { get; private set; }
        public const int MaximumChickens = 12;
        private int currentAgentType;
        private NavMeshData mesh;
        private NavMeshDataInstance instance;
        private void Start()
        {
            if (chickenPrefab == null) { Debug.LogError("Chicken prefab missing.", this); return; }
            var ground = GameObject.Find("Prototype Ground");
            if (ground == null || NavMesh.GetSettingsCount() == 0) { Debug.LogError("Ground or NavMesh settings missing.", this); return; }
            var sources = new List<NavMeshBuildSource>();
            foreach (var box in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
            {
                if (!box.enabled || box.isTrigger) continue;
                sources.Add(new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = box.transform.localToWorldMatrix * Matrix4x4.Translate(box.center),
                    size = box.size,
                    area = box.gameObject == ground ? 0 : 1
                });
            }
            // Reserve the road and a shoulder wide enough for the cow's body.
            sources.Add(new NavMeshBuildSource {
                shape = NavMeshBuildSourceShape.Box,
                transform = Matrix4x4.TRS(new Vector3(3, 1, -5.1f), Quaternion.identity, Vector3.one),
                size = new Vector3(24, 4, 4.5f), area = 1
            });
            var settings = NavMesh.GetSettingsByIndex(0);
            mesh = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(new Vector3(3, 0, 0), new Vector3(24, 8, 18)), Vector3.zero, Quaternion.identity);
            if (mesh == null) { Debug.LogError("Cannot build prototype NavMesh.", this); return; }
            instance = NavMesh.AddNavMeshData(mesh);
            currentAgentType = settings.agentTypeID;
            Spawn(new Vector3(2, 0, -.8f), settings.agentTypeID);
            Spawn(new Vector3(4, 0, -2.2f), settings.agentTypeID);
            var filter = new NavMeshQueryFilter { agentTypeID = settings.agentTypeID, areaMask = NavMesh.AllAreas };
            if (cowPrefab != null && NavMesh.SamplePosition(new Vector3(0, 0, .15f), out var cowSpawn, 2f, filter))
            {
                var cow = Instantiate(cowPrefab, cowSpawn.position, FacingFeed(cowSpawn.position, ProjectMeadow.Farming.FoodPlot.FeedKind.Cattle), transform);
                cow.GetComponent<NavMeshAgent>().agentTypeID = settings.agentTypeID;
                CowCount = 1;
            }
            else Debug.LogError("Cow prefab or valid spawn missing.", this);
        }
        public bool TrySpawnPurchasedChicken()
        {
            if (!instance.valid || chickenPrefab == null || ChickenCount >= MaximumChickens) return false;
            // Alternate between the open sides of the hen-house garden.
            bool right = ChickenCount % 2 != 0;
            float row = (ChickenCount / 2) % 3;
            return Spawn(new Vector3(right ? 5.9f : 2f, 0, (right ? .65f : -.8f) + row * .9f), currentAgentType);
        }
        private bool Spawn(Vector3 position, int agentType)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agentType, areaMask = NavMesh.AllAreas };
            if (!NavMesh.SamplePosition(position, out var hit, 2f, filter)) { Debug.LogError("Chicken spawn is outside NavMesh.", this); return false; }
            var chicken = Instantiate(chickenPrefab, hit.position, FacingFeed(hit.position, ProjectMeadow.Farming.FoodPlot.FeedKind.Poultry), transform);
            chicken.GetComponent<NavMeshAgent>().agentTypeID = agentType;
            ChickenCount++;
            return true;
        }
        private static Quaternion FacingFeed(Vector3 position, ProjectMeadow.Farming.FoodPlot.FeedKind kind)
        {
            foreach (var plot in FindObjectsByType<ProjectMeadow.Farming.FoodPlot>(FindObjectsSortMode.None))
            {
                if (plot.Kind != kind) continue;
                var direction = plot.transform.position - position;
                direction.y = 0;
                if (direction.sqrMagnitude > .001f) return Quaternion.LookRotation(direction);
            }
            return Quaternion.identity;
        }
        private void OnDestroy()
        {
            if (instance.valid) instance.Remove();
            if (mesh != null) Destroy(mesh);
        }
    }
}
