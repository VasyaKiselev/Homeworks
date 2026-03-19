using System;

public interface ICommand
{
    void Execute(); // выполнить команду
    bool CanExecute(); // можно ли выполнить
    string GetDesc(); // описание команды
}

public interface ICondition
{
    bool IsMet(GameState gs); // проверка условия
    string GetDesc(); // описание условия
}

public interface IEffect
{
    void Apply(GameState gs); // применить эффект
    string GetDesc(); // описание эффекта
}

public interface IGameEvent
{
    ICondition TriggerCond { get; } // условие запуска
    IEffect[] Effects { get; } // список эффектов
    bool IsOneTime { get; } // один раз или нет
    bool HasTriggered { get; } // уже сработало
    void CheckAndTrigger(GameState gs); // проверить и запустить
}

public interface IInteractable
{
    string Id { get; } // номер объекта
    string Name { get; } // название
    bool IsAvailable(); // доступен ли
    void Interact(Character ch, GameState gs); // взаимодействие
}

public interface IKiller
{
    void Patrol(); // патрулировать
    int Attack(Character target); // атаковать
    Location CurrLoc { get; set; } // текущее место
    bool IsActive { get; } // активен ли
}

public class ExitLink
{
    public string Dir; // направление
    public Location Target; // куда ведет
}

public class WorldFlag
{
    public string Name; // имя флага
    public bool Value; // значение
}

public abstract class CommandBase : ICommand
{
    protected GameState GS;
    protected Character Player;
    protected CommandBase(GameState gs, Character p) { GS = gs; Player = p; }
    public abstract void Execute(); // выполнить
    public abstract bool CanExecute(); // проверка доступности
    public abstract string GetDesc(); // текст справки
}

public abstract class ConditionBase : ICondition
{
    public abstract bool IsMet(GameState gs); // проверить
    public abstract string GetDesc(); // описание
}

public class AndCondition : ConditionBase
{
    ICondition[] list;
    public AndCondition(params ICondition[] c) { list = c; }
    public override bool IsMet(GameState gs)
    {
        if (list == null) return true;
        for (int i = 0; i < list.Length; i++)
            if (!list[i].IsMet(gs)) return false; // если одно нет - false
        return true;
    }
    public override string GetDesc()
    {
        string s = "Все: ";
        if (list != null)
            for (int i = 0; i < list.Length; i++) s += list[i].GetDesc() + ", ";
        return s;
    }
}

public class OrCondition : ConditionBase
{
    ICondition[] list;
    public OrCondition(params ICondition[] c) { list = c; }
    public override bool IsMet(GameState gs)
    {
        if (list == null) return false;
        for (int i = 0; i < list.Length; i++)
            if (list[i].IsMet(gs)) return true; // если одно да - true
        return false;
    }
    public override string GetDesc()
    {
        string s = "Любое: ";
        if (list != null)
            for (int i = 0; i < list.Length; i++) s += list[i].GetDesc() + ", ";
        return s;
    }
}

public class NotCondition : ConditionBase
{
    ICondition c;
    public NotCondition(ICondition cond) { c = cond; }
    public override bool IsMet(GameState gs) => !c.IsMet(gs); // инверсия
    public override string GetDesc() => "НЕ " + c.GetDesc();
}

public abstract class EffectBase : IEffect
{
    public abstract void Apply(GameState gs); // применить
    public abstract string GetDesc(); // описание
}

public class AddItemEffect : EffectBase
{
    string id; int count;
    public AddItemEffect(string i, int c = 1) { id = i; count = c; }
    public override void Apply(GameState gs) 
    { 
        for (int i = 0; i < count; i++) Player.AddItem(id); // добавить предмет
    }
    public override string GetDesc() => $"Дать {id}";
}

public class DamageEffect : EffectBase
{
    int amount;
    public DamageEffect(int a) { amount = a; }
    public override void Apply(GameState gs) => Player.TakeDamage(amount); // нанести урон
    public override string GetDesc() => $"Урон {amount}";
}

public class SetFlagEffect : EffectBase
{
    string flag; bool val;
    public SetFlagEffect(string f, bool v) { flag = f; val = v; }
    public override void Apply(GameState gs) => gs.SetFlag(flag, val); // установить флаг
    public override string GetDesc() => $"Флаг {flag}={val}";
}

public class LogEffect : EffectBase
{
    string msg;
    public LogEffect(string m) { msg = m; }
    public override void Apply(GameState gs) => gs.Log(msg); // запись в лог
    public override string GetDesc() => $"Лог: {msg}";
}

public class GameOverEffect : EffectBase
{
    string reason; bool win;
    public GameOverEffect(string r, bool w = false) { reason = r; win = w; }
    public override void Apply(GameState gs) => gs.EndGame(reason, win); // конец игры
    public override string GetDesc() => $"Конец: {reason}";
}

public abstract class GameEventBase : IGameEvent
{
    public ICondition TriggerCond { get; set; }
    public IEffect[] Effects { get; protected set; }
    public bool IsOneTime { get; set; }
    public bool HasTriggered { get; private set; }
    
    protected GameEventBase() { Effects = new IEffect[0]; }
    protected void SetEffects(params IEffect[] effs) { Effects = effs; }
    
    public void CheckAndTrigger(GameState gs)
    {
        if (HasTriggered && IsOneTime) return; // если уже было - выход
        if (TriggerCond != null && TriggerCond.IsMet(gs))
        {
            HasTriggered = true;
            for (int i = 0; i < Effects.Length; i++) Effects[i].Apply(gs); // запуск эффектов
        }
    }
}

public abstract class QuestBase
{
    public string Name { get; set; }
    public ICondition CompleteCond { get; set; }
    public bool IsDone { get; private set; }
    public bool Active { get; private set; }
    
    public void Start() { if (!IsDone) Active = true; // начать квест
    }
    public void Check(GameState gs)
    {
        if (Active && !IsDone && CompleteCond != null && CompleteCond.IsMet(gs))
        {
            IsDone = true; Active = false;
            gs.Log($"Квест выполнен: {Name}"); // завершение
        }
    }
}

public class Character
{
    public string Name { get; set; }
    public int Health { get; set; }
    public Location CurrLoc { get; set; }
    public string[] Inv { get; set; }
    public int InvCount { get; private set; }
    public bool Alive => Health > 0;
    
    public Character(string n, int hp) 
    { 
        Name = n; Health = hp; 
        Inv = new string[20]; 
        InvCount = 0;
    }
    
    public void AddItem(string id) 
    { 
        if (!HasItem(id) && InvCount < Inv.Length) 
        { 
            Inv[InvCount++] = id; // положить в инвентарь
        } 
    }
    
    public bool RemoveItem(string id) 
    {
        for (int i = 0; i < InvCount; i++)
        {
            if (Inv[i] == id)
            {
                Inv[i] = Inv[--InvCount]; // убрать из инвентаря
                return true;
            }
        }
        return false;
    }
    
    public bool HasItem(string id) 
    {
        for (int i = 0; i < InvCount; i++)
            if (Inv[i] == id) return true; // поиск предмета
        return false;
    }
    
    public void TakeDamage(int amt) 
    { 
        Health -= amt; 
        if (Health < 0) Health = 0; // отнять здоровье
    }
    
    public void MoveTo(Location loc) => CurrLoc = loc; // переместить
}

public class Enemy : Character, IKiller
{
    public Location[] Route { get; set; }
    public bool IsActive { get; set; } = true;
    int step = 0;
    
    public Enemy(string n, int hp) : base(n, hp) { Route = new Location[0]; }
    
    public void Patrol()
    {
        if (!IsActive || Route.Length == 0) return;
        step = (step + 1) % Route.Length; // следующая точка
        CurrLoc = Route[step];
    }
    
    public int Attack(Character target)
    {
        if (!IsActive || !target.Alive) return 0;
        target.TakeDamage(10); // ударить игрока
        return 10;
    }
}

public class Location
{
    public string Name { get; set; }
    public string Desc { get; set; }
    public ExitLink[] Exits { get; set; }
    public IInteractable[] Objects { get; set; }
    public IGameEvent[] Events { get; set; }
    
    public Location(string n, string d) 
    { 
        Name = n; Desc = d; 
        Exits = new ExitLink[0]; 
        Objects = new IInteractable[0];
        Events = new IGameEvent[0];
    }
    
    public void AddExit(string dir, Location loc) 
    {
        ExitLink[] newExits = new ExitLink[Exits.Length + 1];
        for (int i = 0; i < Exits.Length; i++) newExits[i] = Exits[i];
        newExits[Exits.Length] = new ExitLink { Dir = dir, Target = loc };
        Exits = newExits; // добавить выход
    }
    
    public Location GetExit(string dir)
    {
        for (int i = 0; i < Exits.Length; i++)
            if (Exits[i].Dir == dir) return Exits[i].Target; // найти выход
        return null;
    }
    
    public void AddObj(IInteractable obj) 
    {
        IInteractable[] newObjs = new IInteractable[Objects.Length + 1];
        for (int i = 0; i < Objects.Length; i++) newObjs[i] = Objects[i];
        newObjs[Objects.Length] = obj; // добавить объект
        Objects = newObjs;
    }
    
    public void AddEvent(IGameEvent ev) 
    {
        IGameEvent[] newEvents = new IGameEvent[Events.Length + 1];
        for (int i = 0; i < Events.Length; i++) newEvents[i] = Events[i];
        newEvents[Events.Length] = ev; // добавить событие
        Events = newEvents;
    }
    
    public void CheckEvents(GameState gs) 
    { 
        for (int i = 0; i < Events.Length; i++) 
            Events[i].CheckAndTrigger(gs); // проверка событий
    }
}

public class GameState
{
    public Character Player { get; }
    public WorldFlag[] Flags { get; set; }
    public int FlagCount { get; private set; }
    public string[] Log { get; set; }
    public int LogCount { get; private set; }
    public QuestBase[] Quests { get; set; }
    public int QuestCount { get; private set; }
    public int Turn { get; private set; }
    public bool Over { get; private set; }
    public string EndMsg { get; private set; }
    public bool Win { get; private set; }
    
    public GameState(Character player)
    {
        Player = player;
        Flags = new WorldFlag[20];
        FlagCount = 0;
        Log = new string[100];
        LogCount = 0;
        Quests = new QuestBase[10];
        QuestCount = 0;
        Turn = 0;
    }
    
    public void SetFlag(string name, bool val)
    {
        for (int i = 0; i < FlagCount; i++)
        {
            if (Flags[i].Name == name)
            {
                Flags[i].Value = val; // изменить флаг
                return;
            }
        }
        if (FlagCount < Flags.Length)
        {
            Flags[FlagCount++] = new WorldFlag { Name = name, Value = val }; // создать флаг
        }
    }
    
    public bool GetFlag(string name)
    {
        for (int i = 0; i < FlagCount; i++)
            if (Flags[i].Name == name) return Flags[i].Value; // найти флаг
        return false;
    }
    
    public void Log(string msg) 
    { 
        if (LogCount < Log.Length) Log[LogCount++] = $"[Ход {Turn}] {msg}"; // запись в журнал
    }
    
    public void AddQuest(QuestBase q) 
    { 
        if (QuestCount < Quests.Length) 
        { 
            Quests[QuestCount++] = q; 
            q.Start(); 
        } 
    }
    
    public void CheckQuests() 
    { 
        for (int i = 0; i < QuestCount; i++) 
            Quests[i].Check(this); // проверка квестов
    }
    
    public void EndGame(string msg, bool victory = false)
    {
        if (!Over) { Over = true; Win = victory; EndMsg = msg; Log($"Игра окончена: {msg}"); } // завершение
    }
    
    public void NextTurn() => Turn++; // следующий ход
}

public class HasItemCondition : ConditionBase
{
    string id;
    public HasItemCondition(string itemId) { id = itemId; }
    public override bool IsMet(GameState gs) => gs.Player.HasItem(id); // есть предмет
    public override string GetDesc() => $"Есть {id}";
}

public class FlagCondition : ConditionBase
{
    string flag; bool expect;
    public FlagCondition(string f, bool e = true) { flag = f; expect = e; }
    public override bool IsMet(GameState gs) => gs.GetFlag(flag) == expect; // проверка флага
    public override string GetDesc() => $"Флаг {flag}={expect}";
}

public class HealthCondition : ConditionBase
{
    int min, max;
    public HealthCondition(int mn = 0, int mx = 1000) { min = mn; max = mx; }
    public override bool IsMet(GameState gs)
    {
        int h = gs.Player.Health;
        return h >= min && h <= max; // проверка здоровья
    }
    public override string GetDesc() => $"Здоровье [{min}, {max}]";
}

public class LocationCondition : ConditionBase
{
    Location target;
    public LocationCondition(Location t) { target = t; }
    public override bool IsMet(GameState gs) => gs.Player.CurrLoc == target; // проверка места
    public override string GetDesc() => $"В {target?.Name}";
}