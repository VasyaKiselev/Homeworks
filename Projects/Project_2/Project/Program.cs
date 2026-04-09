
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

public abstract class EffectBase : IEffect
{
    public abstract void Apply(GameState gs); // применить
    public abstract string GetDesc(); // описание
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
    
    public void Start() { if (!IsDone) Active = true; } // начать квест
    public void Check(GameState gs)
    {
        if (Active && !IsDone && CompleteCond != null && CompleteCond.IsMet(gs))
        {
            IsDone = true; Active = false;
            gs.Log($"Квест выполнен: {Name}"); // завершение
        }
    }
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


public class AddItemEffect : EffectBase
{
    string id; int count;
    public AddItemEffect(string i, int c = 1) { id = i; count = c; }
    public override void Apply(GameState gs) 
    { 
        for (int i = 0; i < count; i++) gs.Player.AddItem(id); // добавить предмет
    }
    public override string GetDesc() => $"Дать {id}";
}

public class DamageEffect : EffectBase
{
    int amount;
    public DamageEffect(int a) { amount = a; }
    public override void Apply(GameState gs) => gs.Player.TakeDamage(amount); // нанести урон
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

public class UnlockExitEffect : EffectBase
{
    Location loc; string dir;
    public UnlockExitEffect(Location l, string d) { loc = l; dir = d; }
    public override void Apply(GameState gs) 
    { 
        loc.AddExit(dir, gs.Player.CurrLoc); // открыть переход
        gs.Log($"Открыт путь: {dir}"); // запись в лог
    }
    public override string GetDesc() => $"Открыть {dir}"; // описание
}

public class HealEffect : EffectBase
{
    int amount;
    public HealEffect(int a) { amount = a; }
    public override void Apply(GameState gs) 
    { 
        gs.Player.Health = Math.Min(100, gs.Player.Health + amount); // лечение
        gs.Log($"Здоровье +{amount}"); // запись в лог
    }
    public override string GetDesc() => $"Лечение {amount}"; // описание
}

public class RemoveItemEffect : EffectBase
{
    string id;
    public RemoveItemEffect(string i) { id = i; }
    public override void Apply(GameState gs) 
    { 
        gs.Player.RemoveItem(id); // удалить предмет
        gs.Log($"Удалено: {id}"); // запись в лог
    }
    public override string GetDesc() => $"Убрать {id}"; // описание
}


public class EntryEvent : GameEventBase
{
    public EntryEvent(ICondition cond, params IEffect[] effs) 
    { 
        TriggerCond = cond; 
        SetEffects(effs); 
        IsOneTime = false; // не одноразовое
    }
}

public class OneTimeEvent : GameEventBase
{
    public OneTimeEvent(ICondition cond, params IEffect[] effs) 
    { 
        TriggerCond = cond; 
        SetEffects(effs); 
        IsOneTime = true; // одноразовое
    }
}

public class TurnEvent : GameEventBase
{
    public TurnEvent(ICondition cond, params IEffect[] effs) 
    { 
        TriggerCond = cond; 
        SetEffects(effs); 
        IsOneTime = false; // каждый ход
    }
}


public class GeneratorQuest : QuestBase
{
    public GeneratorQuest() 
    { 
        Name = "Включить генератор"; // название
        CompleteCond = new FlagCondition("generator_on", true); // условие: флаг включён
    }
}

public class ExitQuest : QuestBase
{
    public ExitQuest() 
    { 
        Name = "Добраться до выхода"; // название
        // условие будет установлено при создании мира
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

// ============================================================================
// ОБЪЕКТЫ ВЗАИМОДЕЙСТВИЯ
// ============================================================================

public class Container : IInteractable
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public string[] Items { get; set; }
    public int ItemCount { get; private set; }
    public bool Opened { get; private set; }
    
    public Container(string id, string name, string desc) 
    { 
        Id = id; Name = name; Desc = desc; 
        Items = new string[5]; 
        ItemCount = 0;
        Opened = false;
    }
    
    public void AddLoot(string itemId) 
    { 
        if (ItemCount < Items.Length) Items[ItemCount++] = itemId; // положить лут
    }
    
    public bool IsAvailable() => !Opened; // доступен если не открыт
    
    public void Interact(Character ch, GameState gs)
    {
        if (Opened) { gs.Log($"{Name} уже открыт."); } // уже открыт
        else 
        { 
            Opened = true; // открыть
            gs.Log($"Вы открыли {Name}."); // запись в лог
            for (int i = 0; i < ItemCount; i++) 
            { 
                ch.AddItem(Items[i]); // забрать предметы
                gs.Log($"Получено: {Items[i]}"); // запись в лог
            }
        }
    }
}

public class Door : IInteractable
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string RequiredItem { get; set; }
    public bool Opened { get; private set; }
    public string OpenDesc { get; set; }
    public string LockedDesc { get; set; }
    
    public Door(string id, string name, string reqItem = null) 
    { 
        Id = id; Name = name; RequiredItem = reqItem; 
        Opened = false;
        LockedDesc = $"{Name} заперта."; // описание запертой
        OpenDesc = $"{Name} открыта."; // описание открытой
    }
    
    public bool IsAvailable() => !Opened; // доступна если не открыта
    
    public void Interact(Character ch, GameState gs)
    {
        if (Opened) { gs.Log(OpenDesc); } // уже открыта
        else if (RequiredItem == null) // нет требования
        { 
            Opened = true; 
            gs.Log($"Вы открыли {Name}."); // запись в лог
        }
        else if (ch.HasItem(RequiredItem)) // есть ключ
        { 
            Opened = true; 
            gs.Log($"Вы использовали {RequiredItem} и открыли {Name}."); // запись в лог
        }
        else { gs.Log(LockedDesc); } // заперта
    }
}

public class NPC : IInteractable
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string[] Dialogues { get; set; }
    public int DialogueCount { get; private set; }
    public ICondition[] ReplyConds { get; set; }
    public IEffect[] ReplyEffects { get; set; }
    public int CurrDialogue { get; private set; }
    
    public NPC(string id, string name) 
    { 
        Id = id; Name = name; 
        Dialogues = new string[10]; 
        DialogueCount = 0;
        ReplyConds = new ICondition[0];
        ReplyEffects = new IEffect[0];
        CurrDialogue = 0;
    }
    
    public void AddDialogue(string text) 
    { 
        if (DialogueCount < Dialogues.Length) Dialogues[DialogueCount++] = text; // добавить реплику
    }
    
    public void SetReply(ICondition cond, IEffect eff) 
    {
        ICondition[] newConds = new ICondition[ReplyConds.Length + 1];
        IEffect[] newEffects = new IEffect[ReplyEffects.Length + 1];
        for (int i = 0; i < ReplyConds.Length; i++) newConds[i] = ReplyConds[i];
        for (int i = 0; i < ReplyEffects.Length; i++) newEffects[i] = ReplyEffects[i];
        newConds[ReplyConds.Length] = cond;
        newEffects[ReplyEffects.Length] = eff;
        ReplyConds = newConds;
        ReplyEffects = newEffects; // добавить условие и эффект
    }
    
    public bool IsAvailable() => true; // всегда доступен
    
    public void Interact(Character ch, GameState gs)
    {
        if (CurrDialogue < DialogueCount) 
        { 
            gs.Log($"{Name}: {Dialogues[CurrDialogue]}"); // показать реплику
            CurrDialogue++; // следующая
        }
        else
        {
            for (int i = 0; i < ReplyConds.Length; i++)
            {
                if (ReplyConds[i].IsMet(gs)) 
                { 
                    ReplyEffects[i].Apply(gs); // применить эффект
                    break;
                }
            }
            gs.Log($"{Name} молчит."); // конец диалога
        }
    }
}

public class Trap : IInteractable
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Damage { get; set; }
    public bool Triggered { get; private set; }
    public ICondition DetectCond { get; set; }
    
    public Trap(string id, string name, int dmg) 
    { 
        Id = id; Name = name; Damage = dmg; 
        Triggered = false;
    }
    
    public bool IsAvailable() => !Triggered; // доступна если не сработала
    
    public void Interact(Character ch, GameState gs)
    {
        if (Triggered) { gs.Log($"{Name} уже сработала."); } // уже сработала
        else if (DetectCond != null && DetectCond.IsMet(gs)) // обнаружена
        { 
            Triggered = true; 
            gs.Log($"Вы обезвредили {Name}!"); // запись в лог
        }
        else 
        { 
            Triggered = true; 
            ch.TakeDamage(Damage); // получить урон
            gs.Log($"{Name}! Урон {Damage}!"); // запись в лог
        }
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


public class HelpCommand : CommandBase
{
    public HelpCommand(GameState gs, Character p) : base(gs, p) { }
    public override void Execute() 
    { 
        GS.Log("Команды: help, look, go [dir], interact [id], inv, status, quit"); // справка
    }
    public override bool CanExecute() => true; // всегда доступна
    public override string GetDesc() => "help - список команд"; // описание
}

public class LookCommand : CommandBase
{
    public LookCommand(GameState gs, Character p) : base(gs, p) { }
    public override void Execute() 
    { 
        Location loc = Player.CurrLoc;
        GS.Log($"=== {loc.Name} ==="); // название локации
        GS.Log(loc.Desc); // описание
        if (loc.Exits.Length > 0) 
        { 
            string exits = "Выходы: ";
            for (int i = 0; i < loc.Exits.Length; i++) exits += loc.Exits[i].Dir + " ";
            GS.Log(exits); // показать выходы
        }
        if (loc.Objects.Length > 0) 
        { 
            string objs = "Объекты: ";
            for (int i = 0; i < loc.Objects.Length; i++) 
                if (loc.Objects[i].IsAvailable()) objs += loc.Objects[i].Name + " ";
            GS.Log(objs); // показать объекты
        }
    }
    public override bool CanExecute() => true; // всегда доступна
    public override string GetDesc() => "look - осмотреться"; // описание
}

public class GoCommand : CommandBase
{
    string direction;
    public GoCommand(GameState gs, Character p, string dir) : base(gs, p) { direction = dir; }
    public override void Execute() 
    { 
        Location next = Player.CurrLoc.GetExit(direction);
        if (next != null) 
        { 
            Player.MoveTo(next); // переместить
            GS.Log($"Вы идёте на {direction}."); // запись в лог
            next.CheckEvents(GS); // проверить события
            GS.CheckQuests(); // проверить квесты
        }
        else { GS.Log($"Нет пути на {direction}."); } // нет выхода
    }
    public override bool CanExecute() => Player.Alive && !GS.Over; // если жив и игра не окончена
    public override string GetDesc() => $"go {direction} - идти {direction}"; // описание
}

public class InteractCommand : CommandBase
{
    string objId;
    public InteractCommand(GameState gs, Character p, string id) : base(gs, p) { objId = id; }
    public override void Execute() 
    { 
        IInteractable target = null;
        for (int i = 0; i < Player.CurrLoc.Objects.Length; i++)
        {
            if (Player.CurrLoc.Objects[i].Id == objId) 
            { 
                target = Player.CurrLoc.Objects[i]; 
                break;
            }
        }
        if (target != null && target.IsAvailable()) 
        { 
            target.Interact(Player, GS); // взаимодействие
            GS.CheckQuests(); // проверить квесты
        }
        else { GS.Log($"Нет объекта '{objId}' или он недоступен."); } // не найдено
    }
    public override bool CanExecute() => Player.Alive && !GS.Over; // если жив и игра не окончена
    public override string GetDesc() => $"interact {objId} - взаимодействовать"; // описание
}

public class InvCommand : CommandBase
{
    public InvCommand(GameState gs, Character p) : base(gs, p) { }
    public override void Execute() 
    { 
        if (Player.InvCount == 0) { GS.Log("Инвентарь пуст."); } // пусто
        else 
        { 
            string items = "Инвентарь: ";
            for (int i = 0; i < Player.InvCount; i++) items += Player.Inv[i] + " ";
            GS.Log(items); // показать предметы
        }
    }
    public override bool CanExecute() => true; // всегда доступна
    public override string GetDesc() => "inv - инвентарь"; // описание
}

public class StatusCommand : CommandBase
{
    public StatusCommand(GameState gs, Character p) : base(gs, p) { }
    public override void Execute() 
    { 
        GS.Log($"Здоровье: {Player.Health}/100"); // показать здоровье
        GS.Log($"Локация: {Player.CurrLoc.Name}"); // показать локацию
        GS.Log($"Ход: {GS.Turn}"); // показать ход
    }
    public override bool CanExecute() => true; // всегда доступна
    public override string GetDesc() => "status - состояние игрока"; // описание
}

public class QuitCommand : CommandBase
{
    public QuitCommand(GameState gs, Character p) : base(gs, p) { }
    public override void Execute() => GS.EndGame("Игрок вышел."); // завершить игру
    public override bool CanExecute() => true; // всегда доступна
    public override string GetDesc() => "quit - выход"; // описание
}

public class CommandFactory
{
    public static ICommand Create(string input, GameState gs, Character p)
    {
        string[] parts = input.ToLower().Split(' ');
        if (parts.Length == 0) return null;
        switch (parts[0]) // разбор команды
        {
            case "help": return new HelpCommand(gs, p);
            case "look": return new LookCommand(gs, p);
            case "go": return parts.Length > 1 ? new GoCommand(gs, p, parts[1]) : null;
            case "interact": return parts.Length > 1 ? new InteractCommand(gs, p, parts[1]) : null;
            case "inv": return new InvCommand(gs, p);
            case "status": return new StatusCommand(gs, p);
            case "quit": return new QuitCommand(gs, p);
            default: return null;
        }
    }
}

public class QuestEngine
{
    public static void BuildWorld(GameState gs, Character player)
    {
        // Создаём локации
        Location hall = new Location("Hall", "Большой зал с высокими потолками. Пахнет пылью и старым металлом.");
        Location storage = new Location("Storage", "Тесный склад. Повсюду ящики и инструменты.");
        Location darkCorridor = new Location("DarkCorridor", "Тёмный узкий коридор. Слышны странные звуки.");
        Location generatorRoom = new Location("GeneratorRoom", "Комната с огромным генератором. Гудит электричество.");
        Location exit = new Location("Exit", "Дверь на свободу! Но она заперта электронным замком.");
        
        // Связываем локации
        hall.AddExit("north", storage);
        hall.AddExit("east", darkCorridor);
        storage.AddExit("south", hall);
        darkCorridor.AddExit("west", hall);
        darkCorridor.AddExit("north", generatorRoom);
        generatorRoom.AddExit("south", darkCorridor);
        generatorRoom.AddExit("east", exit); // откроется после включения генератора
        exit.AddExit("west", generatorRoom);
        
        // Создаём предметы
        Container chest = new Container("chest", "Старый сундук", "Деревянный сундук с железными углами.");
        chest.AddLoot("Key");
        chest.AddLoot("Torch");
        hall.AddObj(chest);
        
        Container toolbox = new Container("toolbox", "Ящик с инструментами", "Металлический ящик.");
        toolbox.AddLoot("Wrench");
        toolbox.AddLoot("Fuse");
        storage.AddObj(toolbox);
        
        // Создаём дверь в Exit
        Door exitDoor = new Door("exitdoor", "Электронная дверь", "Fuse");
        exit.AddObj(exitDoor);
        
        // Создаём ловушку в коридоре
        Trap corridorTrap = new Trap("trap", "Лазерная сетка", 10);
        corridorTrap.DetectCond = new HasItemCondition("Torch"); // можно обнаружить с факелом
        darkCorridor.AddObj(corridorTrap);
        
        // Создаём терминал в комнате генератора
        NPC terminal = new NPC("terminal", "Терминал управления");
        terminal.AddDialogue("Система: генератор отключен.");
        terminal.AddDialogue("Система: требуется предохранитель.");
        terminal.SetReply(
            new HasItemCondition("Fuse"), 
            new SetFlagEffect("generator_on", true)
        );
        terminal.SetReply(
            new HasItemCondition("Fuse"),
            new LogEffect("Генератор включён! Питание восстановлено.")
        );
        terminal.SetReply(
            new HasItemCondition("Fuse"),
            new UnlockExitEffect(generatorRoom, "east") // открыть путь к выходу
        );
        generatorRoom.AddObj(terminal);
        
        // Событие: урон в тёмном коридоре
        darkCorridor.AddEvent(new EntryEvent(
            new AndCondition(
                new LocationCondition(darkCorridor),
                new NotCondition(new HasItemCondition("Torch"))
            ),
            new DamageEffect(5),
            new LogEffect("Темнота наносит вам урон!")
        ));
        
        // Событие: победа при выходе
        exit.AddEvent(new OneTimeEvent(
            new AndCondition(
                new LocationCondition(exit),
                new FlagCondition("generator_on", true)
            ),
            new GameOverEffect("Вы выбрались! Победа!", true)
        ));
        
        // Квесты
        GeneratorQuest genQuest = new GeneratorQuest();
        ExitQuest exitQuest = new ExitQuest();
        exitQuest.CompleteCond = new AndCondition(
            new LocationCondition(exit),
            new FlagCondition("generator_on", true)
        );
        gs.AddQuest(genQuest);
        gs.AddQuest(exitQuest);
        
        // Устанавливаем начальную локацию
        player.CurrLoc = hall;
        hall.CheckEvents(gs);
    }
}

public class GameEngine
{
    GameState gs;
    Character player;
    
    public GameEngine() 
    { 
        player = new Character("Исследователь", 100); // создать игрока
        gs = new GameState(player); // создать состояние
        QuestEngine.BuildWorld(gs, player); // построить мир
    }
    
    public void Run()
    {
        Console.WriteLine("=== КВЕСТ: ПОБЕГ ИЗ ЛАБОРАТОРИИ ==="); // приветствие
        Console.WriteLine("Введите 'help' для списка команд.");
        
        while (!gs.Over && player.Alive) // игровой цикл
        {
            Console.Write("\n> ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;
            
            ICommand cmd = CommandFactory.Create(input, gs, player);
            if (cmd == null) { gs.Log("Неизвестная команда."); } // неизвестная
            else if (cmd.CanExecute()) { cmd.Execute(); } // выполнить
            else { gs.Log("Нельзя выполнить эту команду."); } // нельзя
            
            if (player.Health <= 0 && !gs.Over) 
            { 
                gs.EndGame("Вы погибли.", false); // смерть игрока
            }
            
            gs.NextTurn(); // следующий ход
            gs.CheckQuests(); // проверить квесты
            
            // Показать последние сообщения
            if (gs.LogCount > 0) 
            { 
                Console.WriteLine(gs.Log[gs.LogCount - 1]); // последнее сообщение
            }
        }
        
        // Конец игры
        Console.WriteLine($"\n=== {gs.EndMsg} ===");
        if (gs.Win) Console.WriteLine("ПОБЕДА!");
        else Console.WriteLine("ПОРАЖЕНИЕ.");
        Console.WriteLine($"Всего ходов: {gs.Turn}");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        GameEngine game = new GameEngine(); // создать игру
        game.Run(); // запустить цикл
    }
}
