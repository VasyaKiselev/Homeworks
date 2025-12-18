using System;

namespace StudentsApp
{
    
    class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Group { get; set; }

        public Student(string name, int age, string group)
        {
            Name = name;
            Age = age;
            Group = group;
        }

        public void Study()
        {
            Console.WriteLine($"Студент по имени {Name}, которому {Age} лет, учится в группе {Group}");
        }
    }

   
    class Magistr : Student
    {
        public Magistr(string name, int age, string group)
            : base(name, age, group)
        {
        }

        public void DefendDiploma()
        {
            Console.WriteLine($"Магистр {Name} защищает диплом.");
        }
    }

   
    class Bachelor : Student
    {
        public Bachelor(string name, int age, string group)
            : base(name, age, group)
        {
        }

        public void TakeExams()
        {
            Console.WriteLine($"Бакалавр {Name} сдаёт экзамены.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Magistr magistr = new Magistr("Иван", 23, "М-21");
            magistr.Study();
            magistr.DefendDiploma();

            Console.WriteLine();

            Bachelor bachelor = new Bachelor("Анна", 19, "Б-11");
            bachelor.Study();
            bachelor.TakeExams();
        }
    }
}
