using System;
using System.Security.Cryptography;
using Antlr4.Build.Tasks;

namespace BestDiceRollerBot
{
    public class RandomProducer
    {
        private readonly RNGCryptoServiceProvider _gen;
        private readonly byte genSize = 64;

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
            var span = (ulong)Math.Abs(exMax - inMin);
            var t = span * (ulong)(Math.Floor(Math.Pow(2, genSize)) / span);
            var val = BitConverter.ToUInt64(buffer);
            while (val >= t) //Rare but may happen, doing so prevents any bias
            {
                lock (_gen)
                {
                    _gen.GetBytes(buffer);
                }
                val = BitConverter.ToUInt64(buffer);
            }
            var r = (val % span);
            return (int)((int)r + inMin);
        }
    }
}