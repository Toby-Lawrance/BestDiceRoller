using System;
using System.Security.Cryptography;
using Antlr4.Build.Tasks;

namespace BestDiceRollerBot
{
    public class RandomProducer
    {
        private readonly RNGCryptoServiceProvider _gen;
        private readonly byte genSize = 32;

        public RandomProducer()
        {
            _gen = new RNGCryptoServiceProvider();
        }

        public int Get(int inMin, int exMax)
        {
            var buffer = new byte[genSize];
            lock (_gen)
            {
                _gen.GetBytes(buffer);
            }
            var span = (uint)Math.Abs(exMax - inMin);
            var t = span * (uint)(Math.Floor(Math.Pow(2, genSize)) / span);
            var val = BitConverter.ToUInt32(buffer);
            while (val >= t) //Rare but may happen, doing so prevents any bias
            {
                Console.WriteLine("Re-rolling to prevent bias");
                lock (_gen)
                {
                    _gen.GetBytes(buffer);
                }
                val = BitConverter.ToUInt32(buffer);
            }
            var r = (val % span);
            return (int)((int)r + inMin);
        }
    }
}