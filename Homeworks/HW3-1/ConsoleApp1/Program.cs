class Animal
{
    protected string name;
    protected int age;

    public Animal(string name, int age)
    {
        this.age = age;
        this.name = name;
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public int Age
    {
        get { return age; }
        set { age = value; }
    }

    public virtual void Sound()
    {
        Console.WriteLine($"Животное имени {name} возраста {age} издает звук...");
    }
}

class Dog : Animal
{
    public Dog(string name, int age)
    {
        this.age = age;
        this.name = name;
    }

    public override void Sound()
    {
        Console.WriteLine($"Собака имени {name} возраста {age} гавкает!");
    }
}
class Cat : Animal
{
    public Cat(string name, int age)
    {
        this.age = age;
        this.name = name;
    }

    public override void Sound()
    {
        Console.WriteLine($"Кошка имени {name} возраста {age} гавкает!");
    }
}

