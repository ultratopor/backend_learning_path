namespace BlackHole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int counter = 1000000;
            for(int i =0; i<counter; i++)
            {
                string s = "User" + Guid.NewGuid().ToString();
                string.IsInterned(s);
                string.Intern(s);
                string.IsInterned(s);
            }
        }
    }
}
