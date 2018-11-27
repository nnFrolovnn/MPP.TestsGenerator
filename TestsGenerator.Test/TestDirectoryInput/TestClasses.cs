using System.IO;

namespace TestClasses
{
    class Class3
    {
        public static void Main3(string s)
        {
            string t = "test 3";
            Console.WriteLine(t.ToString());
            Console.ReadKey();
        }
    }
}

namespace Ns1
{
    class Class1
    {
        public void Main(string s)
        {
            Console.WriteLine("test 1");
            Console.ReadKey();
        }
    }
}

namespace Ns1
{
    class Class2
    {
        public void Main2(string s)
        {
            string t = "test 2";
            Console.WriteLine(t.ToString());
        }
		
		public void Main2(string s, int a)
        {
            string t = "test 2";
            Console.WriteLine(t.ToString() + a);
        }
    }
}

