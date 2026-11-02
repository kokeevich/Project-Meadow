using UnityEngine;
using ProjectMeadow.Interaction;

namespace ProjectMeadow.Farming
{
    [RequireComponent(typeof(Renderer))]
    public sealed class FoodPlot : MonoBehaviour, IInteractable
    {
        public enum FeedKind { Poultry, Cattle }
        [SerializeField] private FeedKind kind;
        public FeedKind Kind => kind;
        public enum GrowthState { Empty, Growing, Ready }
        [SerializeField, Min(0.1f)] private float growthSeconds = 5f;
        public GrowthState State { get; private set; }
        public float Progress { get; private set; }
        private Object reservedBy;
        public bool IsAvailableFor(Object animal) => State == GrowthState.Ready && (reservedBy == null || reservedBy == animal);
        public bool TryReserve(Object animal)
        {
            if (animal == null || !IsAvailableFor(animal)) return false;
            reservedBy = animal; return true;
        }
        public void Release(Object animal) { if (reservedBy == animal) reservedBy = null; }
        private Renderer surface;
        private MaterialPropertyBlock properties;
        private void Awake()
        {
            surface = GetComponent<Renderer>();
            properties = new MaterialPropertyBlock();
            Refresh();
        }
        public void Interact()
        {
            if (State != GrowthState.Empty) return;
            State = GrowthState.Growing;
            Refresh();
        }
        private void Update() => AdvanceGrowth(Time.deltaTime);
        public void AdvanceGrowth(float seconds)
        {
            if (State != GrowthState.Growing || seconds <= 0f) return;
            Progress = Mathf.Clamp01(Progress + seconds / Mathf.Max(0.1f, growthSeconds));
            if (Progress >= 1f) State = GrowthState.Ready;
            Refresh();
        }
        public bool TryConsume()
        {
            if (State != GrowthState.Ready) return false;
            reservedBy = null;
            State = GrowthState.Empty;
            Progress = 0f;
            Refresh();
            return true;
        }
        private void Refresh()
        {
            if (surface == null) return;
            Color color = new Color(.38f, .25f, .17f);
            properties.SetColor("_BaseColor", color);
            surface.SetPropertyBlock(properties);
        }
    }
}
