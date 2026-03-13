interface Icommand
{
    virtual void Exicute;
}   

interface IInteractable
{
    
} 

interface IEffect
{
    virtual void NewLocation;
} 

interface IGameEvent 
{

} 

interface IGameGuest 
{
    virtual void StartGuest;
} 

interface ICondition 
{
    virtual void IsCondition;
} 


interface IKiller  
{
    virtual void Movement;
    virtual void Attack;
    virtual void WorldChange;
} 

abstract class Character : ICommand
{
    string name;
    int health;
    double speed;
    // public Character (string name, int health)
    // {
    //     this.name = name;
    //     this.health = health;
    // }
    string Name
    {
        get
        {
            value = name;
        }
        set
        {
            return value;
        }
    }
    int Health
    {
        set
        {
            return 75;
        }
    }
    Double Speed
    {
        get
        {
            value = speed;
        }
        set
        {
            return value;
        }
    }
}

abstract class Enemy : IKiller
{
    double speed;
    double Speed
    {
        get
        {
            value = speed;
        }
        set
        {
            return value;
        }
    }
    public override int Attack()
    {
        //Character.Health -= 15;
        //return Character.Health; Тут должен быть объект классла
    }
    
        
    }


abstract class CommandBase : ICommand
{
    virtual void Help;
    virtual void Look;
    virtual void Go;
    virtual void Interact;
    virtual void Inv;
    virtual void Status;
}

abstract class ConditionBase : ICondition
{
    virtual void GameOver;
    virtual bool IsThereAnItem;
    virtual bool FlagState;
    virtual bool And;
    virtual bool Or;
    virtual bool Not;
}

abstract class EffectBase : IEffect
{
    public override void GameEnd()
    {
        // if (Character.Health <= 0) Тут должен быть объект класса персонажа
        // {
        //     Console.WriteLine("Игра окончена");
        // }

    }
}

abstract class GameEventBase : IGameEvent
{
    
}

abstract class QuestBase : IGameGuest
{
    
}

abstract class Location  
{
    
}

abstract class GameState 
{
    
}