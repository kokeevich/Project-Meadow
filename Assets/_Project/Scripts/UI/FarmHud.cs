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
        private Rect Market => new Rect(Width - 358, 112, 334, 380);
        public void Initialize(InventoryController storage, EconomyService trading, ChickenPrototype flock, FoodPlot food, Truck vehicle)
        { warehouse = storage; economy = trading; chickens = flock; plot = food; truck = vehicle; }
        public bool BlocksScreenPoint(Vector2 point)
        {
            Rect safe = Screen.safeArea;
            if (!safe.Contains(point)) return true;
            Vector2 p = new Vector2((point.x - safe.x) / Scale, (safe.yMax - point.y) / Scale);
            return new Rect(20, 20, Width - 40, 72).Contains(p)
                || new Rect(20, Height - 102, Width - 40, 82).Contains(p)
                || (truck != null && truck.Selected && Market.Contains(p));
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
            var inventory = warehouse.Inventory;
            Panel(new Rect(20, 20, Width - 40, 72), paper);
            GUI.Label(new Rect(42, 26, 230, 35), "Луговая ферма", title);
            GUI.Label(new Rect(43, 60, 240, 22), "УЮТ В КАЖДОМ ДНЕ", small);
            float left = Width - 720;
            Fill(new Rect(left - 20, 36, 1, 40), new Color(.8f, .82f, .72f));
            GUI.Label(new Rect(left, 28, 150, 24), "МОНЕТЫ", small);
            GUI.Label(new Rect(left, 51, 160, 32), economy.Currency.Balance.ToString(), number);
            GUI.Label(new Rect(left + 170, 28, 160, 24), "ЯЙЦА НА СКЛАДЕ", small);
            GUI.Label(new Rect(left + 170, 51, 170, 32), inventory.Count("egg") + " / " + inventory.Capacity, number);
            GUI.Label(new Rect(left + 365, 28, 120, 24), "КУРИЦЫ", small);
            GUI.Label(new Rect(left + 365, 51, 130, 32), chickens.ChickenCount.ToString(), number);
            if (Action(new Rect(Width - 190, 34, 150, 44), muted ? "Звук: выкл." : "Звук: вкл.", true, green))
            { muted = !muted; warehouse.GetComponent<AudioSource>().mute = muted; }

            Panel(new Rect(20, Height - 102, Width - 40, 82), paper);
            string foodTitle = plot == null ? "Добро пожаловать" : plot.State == FoodPlot.GrowthState.Empty ? "Посадите корм" : plot.State == FoodPlot.GrowthState.Growing ? "Корм подрастает" : "Угощение готово";
            string foodHint = plot == null ? "Ваша маленькая ферма" : plot.State == FoodPlot.GrowthState.Empty ? "Нажмите на грядку — курицы ждут угощение" : plot.State == FoodPlot.GrowthState.Growing ? "Скоро курицы смогут поесть" : "Курицы сами найдут корм. Собирайте яйца нажатием.";
            FitLabel(new Rect(42, Height - 94, Width - 690, 30), foodTitle, title);
            FitLabel(new Rect(42, Height - 61, Width - 690, 25), foodHint, small);
            if (plot != null && plot.State == FoodPlot.GrowthState.Growing)
            {
                Fill(new Rect(42, Height - 29, 240, 4), new Color(.85f, .86f, .77f));
                Fill(new Rect(42, Height - 29, 240 * plot.Progress, 4), green);
            }
            if (Action(new Rect(Width - 590, Height - 84, 270, 48), "Купить курицу · 25", economy.Currency.Balance >= EconomyService.ChickenPrice && chickens.ChickenCount < ChickenPrototype.MaximumChickens, green))
                Notify(economy.TryBuyChicken(chickens.TrySpawnPurchasedChicken) ? "На ферме новая курица!" : "Не удалось разместить курицу");
            if (Action(new Rect(Width - 300, Height - 84, 258, 48), economy.Busy ? "Грузовик в пути…" : "Продать урожай", true, new Color(.66f, .42f, .24f))) truck.Selected = !truck.Selected;
            if (truck.Selected) DrawMarket();
            if (warehouse.FullFeedback || Time.unscaledTime < toastUntil)
            {
                var box = new Rect((Width - 520) / 2, Height - 163, 520, 44);
                Panel(box, green);
                var text = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
                text.normal.textColor = Color.white;
                GUI.Label(box, warehouse.FullFeedback ? "Склад заполнен — отправьте яйца на рынок" : toast, text);
            }
        }
        private void DrawMarket()
        {
            Rect r = Market; Panel(r, paper); float x = r.x + 24, y = r.y;
            FitLabel(new Rect(x, y + 18, r.width - 94, 38), "Фермерский рынок", title);
            if (Action(new Rect(r.xMax - 58, y + 16, 40, 40), "×", true, green)) truck.Selected = false;
            FitLabel(new Rect(x, y + 64, 285, 26), "Свежие яйца · 5 монет за штуку", body);
            GUI.Label(new Rect(x, y + 99, 285, 24), "На складе: " + warehouse.Inventory.Count("egg"), small);
            if (economy.Busy)
            {
                GUI.Label(new Rect(x, y + 143, 285, 44), "Везём на рынок: " + economy.Cargo, title);
                GUI.Label(new Rect(x, y + 197, 285, 30), "Возвращение через " + Mathf.CeilToInt(economy.Remaining) + " сек.", body);
                Fill(new Rect(x, y + 250, 285, 8), new Color(.83f, .85f, .76f));
                Fill(new Rect(x, y + 250, 285 * (1 - economy.Remaining / EconomyService.TripSeconds), 8), green);
                GUI.Label(new Rect(x, y + 294, 285, 55), "Деньги поступят, когда грузовик вернётся.", small);
                return;
            }
            int available = warehouse.Inventory.Count("egg");
            quantity = Mathf.Clamp(quantity, 1, Mathf.Max(1, available));
            if (Action(new Rect(x, y + 143, 54, 48), "−", quantity > 1, green)) quantity--;
            var centered = new GUIStyle(number) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(x + 64, y + 143, 150, 48), quantity.ToString(), centered);
            if (Action(new Rect(x + 230, y + 143, 54, 48), "+", quantity < available, green)) quantity++;
            if (Action(new Rect(x, y + 207, 285, 44), "Выбрать все", available > 0, green)) quantity = available;
            if (Action(new Rect(x, y + 270, 285, 54), "Отправить · +" + quantity * EconomyService.EggPrice + " монет", available >= quantity, new Color(.66f, .42f, .24f)))
            { if (economy.TryDispatch("egg", quantity)) Notify("Грузовик отправился на рынок"); }
            GUI.Label(new Rect(x, y + 334, 285, 28), "Поездка занимает 5 секунд", small);
        }
        private void OnDestroy() { if (rounded != null) Destroy(rounded); }
    }
}
