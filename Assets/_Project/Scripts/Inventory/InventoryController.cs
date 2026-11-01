using UnityEngine;
namespace ProjectMeadow.Inventory
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class InventoryController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 10;
        public InventoryService Inventory { get; private set; }
        public bool FullFeedback => Time.unscaledTime < feedbackUntil;
        private float feedbackUntil;
        private AudioSource source;
        private AudioClip collectSound;
        private void Awake()
        {
            Inventory = new InventoryService(Mathf.Max(1, capacity));
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            const int rate = 22050;
            var samples = new float[4410];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                samples[i] = Mathf.Sin(2 * Mathf.PI * (650 * t + 600 * t * t))
                    * Mathf.Sin(Mathf.PI * i / samples.Length) * .15f;
            }
            collectSound = AudioClip.Create("Original collection chime", samples.Length, 1, rate, false);
            collectSound.SetData(samples, 0);
        }
        public bool TryCollect(string id)
        {
            if (!Inventory.TryAdd(id)) { feedbackUntil = Time.unscaledTime + 2; return false; }
            source.PlayOneShot(collectSound);
            return true;
        }
        private void OnDestroy() { if (collectSound != null) Destroy(collectSound); }
    }
}
