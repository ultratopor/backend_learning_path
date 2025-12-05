namespace Decompilation___Refactoring
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        public async Task<int> CalculateAsync(int a, int b)
        {
            int result = a + b;
            await Task.Delay(100);
            return result * 2;
        }
    }
}
