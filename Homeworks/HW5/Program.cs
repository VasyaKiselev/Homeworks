using System;

namespace GenshinCharacters
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }

    public interface IHealable
    {
        void Heal(int amount);
    }

    public abstract class Character : IDamageable
    {
        public string Name { get; set; }
        public int Health { get; set; }

        public Character(string name, int health)
        {
            Name = name;
            Health = health;
        }

        public void Move()
        {
            Console.WriteLine($"{Name} moves.");
        }

        public abstract void Attack();

        public void TakeDamage(int damage)
        {
            Health -= damage;
            Console.WriteLine($"{Name} takes {damage} damage. HP: {Health}");
        }
    }

    public class Warrior : Character
    {
        public Warrior(string name, int health) : base(name, health) { }

        public override void Attack()
        {
            Console.WriteLine($"{Name} attacks with a sword.");
        }
    }

    public class Mage : Character
    {
        public Mage(string name, int health) : base(name, health) { }

        public override void Attack()
        {
            Console.WriteLine($"{Name} casts a spell.");
        }
    }

    public class Priest : Character, IHealable
    {
        public Priest(string name, int health) : base(name, health) { }

        public override void Attack()
        {
            Console.WriteLine($"{Name} uses elemental power.");
        }

        public void Heal(int amount)
        {
            Health += amount;
            Console.WriteLine($"{Name} heals for {amount}. HP: {Health}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Character[] characters = new Character[3];
            characters[0] = new Warrior("Ayaka", 120);
            characters[1] = new Mage("Klee", 90);
            characters[2] = new Priest("Barbara", 100);

            Console.WriteLine("Characters attack:");
            foreach (Character c in characters)
            {
                c.Attack();
            }

            Console.WriteLine("\nDamage:");
            characters[0].TakeDamage(30);

            Console.WriteLine("\nHealing:");
            if (characters[2] is IHealable healer)
            {
                healer.Heal(40);
            }
        }
    }
}
