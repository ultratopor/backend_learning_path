using ObjectLayoutInspector;

namespace Memory_Exploper
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var emptyClass = new EmptyClass();
            var badStruct = new BadStruct();
            var goodStruct = new GoodStruct();
            var complexClass = new ComplexClass();
            //Console.WriteLine($"EmptyClass Layout: {TypeLayout.PrintLayout<EmptyClass>()}");
            TypeLayout.PrintLayout<EmptyClass>();
            TypeLayout.PrintLayout<BadStruct>();
            TypeLayout.PrintLayout<GoodStruct>();
            TypeLayout.PrintLayout<ComplexClass>();
            Console.ReadKey();

            var leakyApp = new LeakyApp();
            leakyApp.Run();
            Console.ReadKey();
        }
    }
}
