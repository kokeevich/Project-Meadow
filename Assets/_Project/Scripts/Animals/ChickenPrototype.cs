using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace ProjectMeadow.Animals
{
    public sealed class ChickenPrototype : MonoBehaviour
    {
        [SerializeField] private GameObject chickenPrefab;
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
            var settings = NavMesh.GetSettingsByIndex(0);
            mesh = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(Vector3.zero, new Vector3(18, 8, 18)), Vector3.zero, Quaternion.identity);
            if (mesh == null) { Debug.LogError("Cannot build prototype NavMesh.", this); return; }
            instance = NavMesh.AddNavMeshData(mesh);
            currentAgentType = settings.agentTypeID;
            Spawn(new Vector3(-2, 0, -2), settings.agentTypeID);
            Spawn(new Vector3(2, 0, -2), settings.agentTypeID);
        }
        public bool TrySpawnPurchasedChicken()
        {
            if (!instance.valid || chickenPrefab == null || ChickenCount >= MaximumChickens) return false;
            return Spawn(new Vector3(-3 + ChickenCount % 6, 0, -3), currentAgentType);
        }
        private bool Spawn(Vector3 position, int agentType)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agentType, areaMask = NavMesh.AllAreas };
            if (!NavMesh.SamplePosition(position, out var hit, 2f, filter)) { Debug.LogError("Chicken spawn is outside NavMesh.", this); return false; }
            var chicken = Instantiate(chickenPrefab, hit.position, Quaternion.identity, transform);
            chicken.GetComponent<NavMeshAgent>().agentTypeID = agentType;
            ChickenCount++;
            return true;
        }
        private void OnDestroy()
        {
            if (instance.valid) instance.Remove();
            if (mesh != null) Destroy(mesh);
        }
    }
}
