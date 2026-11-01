using UnityEngine;
namespace ProjectMeadow.Items
{
    [CreateAssetMenu(menuName = "Project Meadow/Item Data")]
    public sealed class ItemData : ScriptableObject
    {
        public string id = "egg";
        public string displayName = "Egg";
    }
}
