using UnityEngine;
namespace ProjectMeadow.Animals
{
    [CreateAssetMenu(menuName = "Project Meadow/Animal Data")]
    public sealed class AnimalData : ScriptableObject
    {
        [Min(0.1f)] public float moveSpeed = 1.5f;
        [Min(0.1f)] public float eatSeconds = 2f;
        [Min(0.1f)] public float productionSeconds = 5f;
        [Min(0.1f)] public float searchInterval = 0.5f;
    }
}
