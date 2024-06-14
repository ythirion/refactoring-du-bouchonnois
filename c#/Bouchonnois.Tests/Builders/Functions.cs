namespace Bouchonnois.Tests.Builders
{
    public static class Functions
    {
        public static void Repeat(int times, Action call)
        {
            while (times > 0)
            {
                call();
                times--;
            }
        }
    }
}