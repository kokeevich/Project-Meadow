using UnityEngine;
namespace ProjectMeadow.Levels
{
    [CreateAssetMenu(menuName = "Project Meadow/Level Data")]
    public sealed class LevelData : ScriptableObject
    {
        public string title = "Первые яйца";
        [Min(1)] public int eggsToSell = 10;
    }
}
