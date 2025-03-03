namespace API_LapinCouvert.Services
{
    public class RandomService
    {
        private static readonly Random Random = new Random();

        public virtual int Next(int min, int max)
        {
            return Random.Next(min, max);
        }
    }
}
