using UnityEngine;
using ProjectMeadow.Economy;
using ProjectMeadow.Animals;
using ProjectMeadow.Farming;
using ProjectMeadow.Inventory;

namespace ProjectMeadow.UI
{
    public sealed class FarmHud : MonoBehaviour
    {
        private InventoryController warehouse;
        private EconomyService economy;
        private ChickenPrototype chickens;
        private FoodPlot plot;
        private Truck truck;
        private ProjectMeadow.Levels.LevelController level;
        private ProjectMeadow.Audio.LevelAudio audioPlayer;
        private ProjectMeadow.Production.ProductionBuilding mill;
        private ProjectMeadow.Production.ProductionBuilding[] buildings;
        private ProjectMeadow.Farming.GrainPlot grain;
        private int marketItem;
        private readonly string[] itemIds = { "egg", "grain", "flour", "milk", "cheese" };
        private readonly string[] itemNames = { "Яйца", "Зерно", "Мука", "Молоко", "Сыр" };
        public bool WorldBlocked => level != null && level.Complete;
        private bool tutorialHidden;
        private int tutorialStep;
        private int quantity = 1;
        private bool muted;
        private string toast;
        private float toastUntil;
        private Texture2D rounded;
        private GUIStyle title, body, small, number, button, panel;
        private readonly Color ink = new Color(.19f, .27f, .23f);
        private readonly Color mutedInk = new Color(.43f, .48f, .39f);
        private readonly Color paper = new Color(.98f, .96f, .88f);
        private readonly Color green = new Color(.28f, .43f, .33f);
        private float Scale => Mathf.Max(.1f, Mathf.Min(Screen.safeArea.width / 1152f, Screen.safeArea.height / 648f));
        private float Width => Screen.safeArea.width / Scale;
        private float Height => Screen.safeArea.height / Scale;
        private Rect Market => new Rect(Width - 358, 112, 334, 420);
        public void Initialize(InventoryController storage, EconomyService trading, ChickenPrototype flock, FoodPlot food, Truck vehicle, ProjectMeadow.Levels.LevelController currentLevel, ProjectMeadow.Audio.LevelAudio sound, ProjectMeadow.Production.ProductionBuilding[] production, ProjectMeadow.Farming.GrainPlot wheat)
        {
            warehouse = storage; economy = trading; chickens = flock; plot = food; truck = vehicle;
            buildings = production; grain = wheat;
            foreach (var building in buildings) if (building.Kind == ProjectMeadow.Production.ProductionBuilding.BuildingKind.Mill) mill = building;
            level = currentLevel; audioPlayer = sound; muted = sound.Muted;
            warehouse.GetComponent<AudioSource>().mute = muted;
        }
        public bool BlocksScreenPoint(Vector2 point)
        {
            if (level != null && level.Complete) return true;
            if (buildings != null) foreach (var building in buildings) if (building.Selected) mill = building;
            Rect safe = Screen.safeArea;
            if (!safe.Contains(point)) return true;
            Vector2 p = new Vector2((point.x - safe.x) / Scale, (safe.yMax - point.y) / Scale);
            return new Rect(20, 20, Width - 40, 72).Contains(p)
                || new Rect(20, Height - 102, Width - 40, 82).Contains(p)
                || new Rect(20, 112, 320, tutorialHidden ? 105 : 220).Contains(p)
                || (((truck != null && truck.Selected) || (mill != null && mill.Selected)) && Market.Contains(p));
        }
        private void SetupStyles()
        {
            if (rounded != null) return;
            rounded = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            rounded.name = "Original rounded UI surface";
            rounded.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float dx = Mathf.Max(7 - x, x - 24, 0), dy = Mathf.Max(7 - y, y - 24, 0);
                rounded.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(8 - Mathf.Sqrt(dx * dx + dy * dy))));
            }
            rounded.Apply();
            title = Text(25, FontStyle.Bold, ink);
            body = Text(18, FontStyle.Normal, ink);
            small = Text(16, FontStyle.Normal, mutedInk);
            number = Text(26, FontStyle.Bold, ink);
            button = Text(18, FontStyle.Bold, Color.white);
            button.alignment = TextAnchor.MiddleCenter;
            button.normal.background = rounded;
            button.hover.background = rounded;
            button.active.background = rounded;
            button.border = new RectOffset(8, 8, 8, 8);
            button.hover.textColor = Color.white;
            button.active.textColor = Color.white;
            panel = new GUIStyle { border = new RectOffset(8, 8, 8, 8) };
            panel.normal.background = rounded;
        }
        private GUIStyle Text(int size, FontStyle weight, Color color)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            style.normal.textColor = color; return style;
        }
        // Fit single-line labels using the actual font metrics, not character counts.
        private void FitLabel(Rect rect, string text, GUIStyle source)
        {
            var fitted = new GUIStyle(source) { wordWrap = false, clipping = TextClipping.Clip };
            var content = new GUIContent(text);
            while (fitted.fontSize > 12 && fitted.CalcSize(content).x > rect.width - 4)
                fitted.fontSize--;
            GUI.Label(rect, content, fitted);
        }
        private void Fill(Rect rect, Color color)
        {
            Color saved = GUI.color; GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = saved;
        }
        private void Panel(Rect rect, Color color)
        {

            Color saved = GUI.color;
            GUI.color = new Color(.12f, .2f, .16f, .12f);
            GUI.Box(new Rect(rect.x, rect.y + 4, rect.width, rect.height), GUIContent.none, panel);
            GUI.color = color; GUI.Box(rect, GUIContent.none, panel); GUI.color = saved;
        }
        private bool Action(Rect rect, string text, bool enabled, Color color)
        {
            bool savedEnabled = GUI.enabled; Color saved = GUI.backgroundColor;
            GUI.enabled = enabled;
            GUI.backgroundColor = enabled ? color : new Color(.65f, .69f, .61f);
            bool clicked = GUI.Button(rect, text, button);
            GUI.backgroundColor = saved; GUI.enabled = savedEnabled; return clicked;
        }
        private void Notify(string text) { toast = text; toastUntil = Time.unscaledTime + 3; }
        private void OnGUI()
        {
            if (economy == null || warehouse == null || chickens == null) return;
            SetupStyles();
            Matrix4x4 saved = GUI.matrix;
            Rect safe = Screen.safeArea;
            GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x, Screen.height - safe.yMax, 0), Quaternion.identity, Vector3.one * Scale);
            try { Draw(); } finally { GUI.matrix = saved; }
        }
        private void Draw()
        {
            if (level != null && level.Complete) { DrawComplete(); return; }
            DrawObjective();
            var inventory = warehouse.Inventory;
            Panel(new Rect(20, 20, Width - 40, 72), paper);
            GUI.Label(new Rect(42, 26, 230, 35), "Луговая ферма", title);
            FitLabel(new Rect(43, 60, 240, 22), "КАМЕРА: WASD / СКМ / ДВА ПАЛЬЦА", small);
            float left = Width - 720;
            Fill(new Rect(left - 20, 36, 1, 40), new Color(.8f, .82f, .72f));
            GUI.Label(new Rect(left, 28, 150, 24), "МОНЕТЫ", small);
            GUI.Label(new Rect(left, 51, 160, 32), economy.Currency.Balance.ToString(), number);
            GUI.Label(new Rect(left + 170, 28, 160, 24), "СКЛАД", small);
            GUI.Label(new Rect(left + 170, 51, 170, 32), inventory.Used + " / " + inventory.Capacity, number);
            FitLabel(new Rect(left + 365, 28, 120, 24), "КУРЫ / КОРОВЫ", small);
            GUI.Label(new Rect(left + 365, 51, 130, 32), chickens.ChickenCount + " / " + chickens.CowCount, number);
            if (Action(new Rect(Width - 190, 34, 150, 44), muted ? "Звук: выкл." : "Звук: вкл.", true, green))
            { muted = !muted; warehouse.GetComponent<AudioSource>().mute = muted; audioPlayer.SetMuted(muted); }

            Panel(new Rect(20, Height - 102, Width - 40, 82), paper);
            string foodTitle = plot == null ? "Добро пожаловать" : plot.State == FoodPlot.GrowthState.Empty ? "Посадите корм" : plot.State == FoodPlot.GrowthState.Growing ? "Корм подрастает" : "Угощение готово";
            string foodHint = plot == null ? "Ваша маленькая ферма" : plot.State == FoodPlot.GrowthState.Empty ? "Нажмите на грядку — курицы ждут угощение" : plot.State == FoodPlot.GrowthState.Growing ? "Скоро курицы смогут поесть" : "Курицы сами найдут корм. Собирайте яйца нажатием.";
            FitLabel(new Rect(42, Height - 94, Width - 740, 30), foodTitle, title);
            FitLabel(new Rect(42, Height - 61, Width - 740, 25), foodHint, small);
            if (plot != null && plot.State == FoodPlot.GrowthState.Growing)
            {
                Fill(new Rect(42, Height - 29, 240, 4), new Color(.85f, .86f, .77f));
                Fill(new Rect(42, Height - 29, 240 * plot.Progress, 4), green);
            }
            if (Action(new Rect(Width - 650, Height - 84, 210, 48), "Курица · 25 монет", economy.Currency.Balance >= EconomyService.ChickenPrice && chickens.ChickenCount < ChickenPrototype.MaximumChickens, green))
                Notify(economy.TryBuyChicken(chickens.TrySpawnPurchasedChicken) ? "На ферме новая курица!" : "Не удалось разместить курицу");
            if (Action(new Rect(Width - 230, Height - 84, 188, 48), economy.Busy ? "Грузовик в пути…" : "Продать урожай", true, new Color(.66f, .42f, .24f))) { truck.Selected = !truck.Selected; if (buildings != null) foreach (var building in buildings) building.Selected = false; }
            if (mill != null && Action(new Rect(Width - 425, Height - 84, 180, 48), "Производство", true, green)) mill.Selected = !mill.Selected;
            if (buildings != null) foreach (var building in buildings) if (building.Selected) mill = building;
            if (mill != null && mill.Selected) { truck.Selected = false; DrawMill(); }
            else if (truck.Selected) DrawMarket();
            if (warehouse.FullFeedback || Time.unscaledTime < toastUntil)
            {
                var box = new Rect((Width - 520) / 2, Height - 163, 520, 44);
                Panel(box, green);
                var text = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
                text.normal.textColor = Color.white;
                GUI.Label(box, warehouse.FullFeedback ? "Склад заполнен — продайте товары на рынке" : toast, text);
            }
        }
        private void DrawObjective()
        {
            if (level == null || level.Objective == null) return;
            var objective = level.Objective;
            Panel(new Rect(20, 112, 320, tutorialHidden ? 105 : 220), paper);
            FitLabel(new Rect(38, 121, 205, 28), level.Title, title);
            if (Action(new Rect(250, 122, 72, 30), "Заново", true, green)) level.Restart();
            FitLabel(new Rect(38, 159, 282, 26), "Продайте яйца: " + objective.Progress + " / " + objective.Target, body);
            Fill(new Rect(38, 194, 282, 6), new Color(.83f, .85f, .76f));
            Fill(new Rect(38, 194, 282f * objective.Progress / objective.Target, 6), green);
            if (tutorialHidden) return;
            if (plot != null && plot.State != FoodPlot.GrowthState.Empty) tutorialStep = Mathf.Max(tutorialStep, 1);
            if (warehouse.Inventory.Count("egg") > 0) tutorialStep = Mathf.Max(tutorialStep, 2);
            if (economy.Busy || economy.EggsSold > 0) tutorialStep = 3;
            string[] tips = {
                "1. Нажмите на грядку, чтобы вырастить корм.",
                "2. Курицы поедят и снесут яйца. Нажмите на яйцо, чтобы собрать его.",
                "3. Откройте рынок, выберите яйца и отправьте грузовик.",
                "4. Дождитесь возвращения грузовика. Продажи засчитываются после доставки."
            };
            GUI.Label(new Rect(38, 214, 282, 66), tips[tutorialStep], small);
            if (Action(new Rect(38, 286, 282, 30), "Понятно, скрыть подсказки", true, green)) tutorialHidden = true;
        }
        private void DrawComplete()
        {
            Fill(new Rect(0, 0, Width, Height), new Color(.12f, .2f, .16f, .65f));
            Rect card = new Rect((Width - 520) / 2, (Height - 330) / 2, 520, 330);
            Panel(card, paper);
            var centeredTitle = new GUIStyle(title) { alignment = TextAnchor.MiddleCenter };
            var centeredBody = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(card.x + 30, card.y + 28, 460, 48), "Первый урожай продан!", centeredTitle);
            GUI.Label(new Rect(card.x + 40, card.y + 92, 440, 60), "Уровень завершён. Ваша ферма сделала первые шаги!", centeredBody);
            GUI.Label(new Rect(card.x + 40, card.y + 166, 440, 35), "Продано яиц: " + economy.EggsSold + "   ·   Монеты: " + economy.Currency.Balance, centeredBody);
            if (Action(new Rect(card.x + 60, card.y + 244, 400, 54), "Сыграть ещё раз", true, green)) level.Restart();
        }
        private void DrawMill()
        {
            if (mill.Service == null) return;
            var service = mill.Service;
            bool dairy = mill.Kind == ProjectMeadow.Production.ProductionBuilding.BuildingKind.CheeseMaker;
            Rect r = Market; Panel(r, paper); float x = r.x + 24, y = r.y;
            FitLabel(new Rect(x, y + 18, r.width - 94, 38), dairy ? "Сыроварня" : "Мельница", title);
            if (Action(new Rect(r.xMax - 58, y + 16, 40, 40), "×", true, green)) mill.Selected = false;
            for (int i = 0; i < buildings.Length; i++)
            {
                bool isDairy = buildings[i].Kind == ProjectMeadow.Production.ProductionBuilding.BuildingKind.CheeseMaker;
                if (Action(new Rect(x + (isDairy ? 147 : 0), y + 65, 138, 32), isDairy ? "Сыроварня" : "Мельница", true, green))
                {
                    foreach (var building in buildings) building.Selected = false;
                    mill = buildings[i]; mill.Selected = true; return;
                }
            }
            FitLabel(new Rect(x, y + 105, 285, 28), (dairy ? "2 молока → 1 сыр · " : "2 зерна → 1 мука · ") + service.NextDuration.ToString("0.#") + " сек.", body);
            FitLabel(new Rect(x, y + 140, 285, 28), dairy ? "Молоко: " + warehouse.Inventory.Count("milk") + "    Сыр: " + warehouse.Inventory.Count("cheese") : "Зерно: " + warehouse.Inventory.Count("grain") + "    Мука: " + warehouse.Inventory.Count("flour"), body);
            GUI.Label(new Rect(x, y + 175, 285, 28), "В очереди: " + service.Queued + " / 3", body);
            Fill(new Rect(x, y + 212, 285, 8), new Color(.83f, .85f, .76f));
            Fill(new Rect(x, y + 212, 285 * service.Progress, 8), green);
            if (Action(new Rect(x, y + 239, 285, 48), dairy ? "Приготовить сыр" : "Смолоть зерно", service.Queued < 3 && warehouse.Inventory.Count(dairy ? "milk" : "grain") >= 2, green)) service.TryQueue();
            if (Action(new Rect(x, y + 302, 285, 48), service.UpgradeLevel >= 2 ? "Здание улучшено" : "Улучшить · " + service.UpgradeCost + " монет", service.UpgradeLevel < 2 && economy.Currency.Balance >= service.UpgradeCost, green)) service.TryUpgrade();
            string hint = service.OutputBlocked ? "Освободите место на складе для продукта" : dairy ? "Посадите траву для коровы. Соберите молоко." : grain != null && grain.Ready ? "Пшеница созрела. Нажмите на неё для сбора." : "Посадите пшеницу на грядке у мельницы.";
            GUI.Label(new Rect(x, y + 364, 285, 42), hint, small);
        }
        private void DrawMarket()
        {
            Rect r = Market; Panel(r, paper); float x = r.x + 24, y = r.y;
            FitLabel(new Rect(x, y + 18, r.width - 94, 38), "Фермерский рынок", title);
            if (Action(new Rect(r.xMax - 58, y + 16, 40, 40), "×", true, green)) truck.Selected = false;
            FitLabel(new Rect(x, y + 64, 285, 26), itemNames[marketItem] + " · " + EconomyService.Price(itemIds[marketItem]) + " монет · В наличии: " + warehouse.Inventory.Count(itemIds[marketItem]), body);
            for (int i = 0; i < itemIds.Length; i++)
                if (Action(new Rect(x + (i % 3) * 96, y + 99 + (i / 3) * 34, 90, 30), itemNames[i], !economy.Busy, i == marketItem ? green : new Color(.6f, .62f, .5f))) { marketItem = i; quantity = 1; }
            if (economy.Busy)
            {
                GUI.Label(new Rect(x, y + 177, 285, 44), "Везём на рынок: " + economy.Cargo, title);
                GUI.Label(new Rect(x, y + 231, 285, 30), "Возвращение через " + Mathf.CeilToInt(economy.Remaining) + " сек.", body);
                Fill(new Rect(x, y + 284, 285, 8), new Color(.83f, .85f, .76f));
                Fill(new Rect(x, y + 284, 285 * (1 - economy.Remaining / EconomyService.TripSeconds), 8), green);
                GUI.Label(new Rect(x, y + 328, 285, 55), "Деньги поступят, когда грузовик вернётся.", small);
                return;
            }
            int available = warehouse.Inventory.Count(itemIds[marketItem]);
            quantity = Mathf.Clamp(quantity, 1, Mathf.Max(1, available));
            if (Action(new Rect(x, y + 177, 54, 48), "−", quantity > 1, green)) quantity--;
            var centered = new GUIStyle(number) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x + 64, y + 177, 150, 48), quantity.ToString(), centered);
            if (Action(new Rect(x + 230, y + 177, 54, 48), "+", quantity < available, green)) quantity++;
            if (Action(new Rect(x, y + 241, 285, 44), "Выбрать все", available > 0, green)) quantity = available;
            if (Action(new Rect(x, y + 304, 285, 54), "Отправить · +" + quantity * EconomyService.Price(itemIds[marketItem]) + " монет", available >= quantity, new Color(.66f, .42f, .24f)))
            { if (economy.TryDispatch(itemIds[marketItem], quantity)) Notify("Грузовик отправился на рынок"); }
            GUI.Label(new Rect(x, y + 368, 285, 28), "Поездка занимает 5 секунд", small);
        }
        private void OnDestroy() { if (rounded != null) Destroy(rounded); }
    }
}
