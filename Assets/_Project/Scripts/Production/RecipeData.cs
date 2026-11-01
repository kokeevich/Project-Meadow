using UnityEngine;
namespace ProjectMeadow.Production
{
    [CreateAssetMenu(menuName = "Project Meadow/Recipe")]
    public sealed class RecipeData : ScriptableObject
    {
        public string inputId = "grain", outputId = "flour";
        [Min(1)] public int inputCount = 2, outputCount = 1;
        [Min(.1f)] public float seconds = 6;
    }
}
