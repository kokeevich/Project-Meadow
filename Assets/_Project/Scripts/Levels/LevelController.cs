using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectMeadow.Economy;
using ProjectMeadow.Objectives;
namespace ProjectMeadow.Levels
{
    public sealed class LevelController : MonoBehaviour
    {
        private EconomyService economy;
        private ProjectMeadow.Audio.LevelAudio audioPlayer;
        public SellObjective Objective { get; private set; }
        public string Title { get; private set; }
        public bool Complete => Objective != null && Objective.Complete;
        public float ElapsedSeconds { get; private set; }
        private bool pausedByLevel;
        public void Initialize(LevelData data, EconomyService service, ProjectMeadow.Audio.LevelAudio sound)
        {
            Title = data != null ? data.title : "Первые яйца";
            Objective = new SellObjective(data != null ? Mathf.Max(1, data.eggsToSell) : 10);
            economy = service; audioPlayer = sound;
        }
        private void LateUpdate()
        {
            if (economy == null || Complete) return;
            ElapsedSeconds += Time.deltaTime;
            if (!Objective.Observe(economy.EggsSold)) return;
            audioPlayer.PlayVictory();
            pausedByLevel = true;
            Time.timeScale = 0;
        }
        public void Restart()
        {
            Time.timeScale = 1;
            pausedByLevel = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        private void OnDestroy() { if (pausedByLevel) Time.timeScale = 1; }
    }
}
