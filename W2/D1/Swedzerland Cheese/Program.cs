namespace Swedzerland_Cheese
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var lohFragmentation = new LohFragmentation();
            lohFragmentation.Run();

            Console.WriteLine("\n\n");

            var intOrBytes = new IntOrBytes();
            intOrBytes.IntValue = 42;
            Console.WriteLine($"Byte0 = {intOrBytes.Byte0}");
            intOrBytes.IntValue = 256;
            Console.WriteLine($"Byte0 = {intOrBytes.Byte0}, Byte1 = {intOrBytes.Byte1}");
            intOrBytes.IntValue = -1;
            Console.WriteLine($"Byte0 = {intOrBytes.Byte0}, Byte1 = {intOrBytes.Byte1}," +
                $" Byte2 = {intOrBytes.Byte2}");
            Console.WriteLine("\n\n");

            string s1 = "Hello";
            string s2 = "World";
            object o = new object();

            // Вывод адресов в HEX формате
            Console.WriteLine(s1.GetType().TypeHandle.Value.ToString("X"));
            Console.WriteLine(s2.GetType().TypeHandle.Value.ToString("X"));
            Console.WriteLine(o.GetType().TypeHandle.Value.ToString("X"));
            Console.ReadKey();
        }
    }
}
