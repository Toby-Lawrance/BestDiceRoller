using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Antlr4.Build.Tasks;

namespace BestDiceRollerBot
{
    public class RandomProducer
    {
        private readonly RNGCryptoServiceProvider _gen;
        private readonly byte genSize = sizeof(uint);

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
            var t = span * (Math.Floor(Math.Pow(2.0, (genSize*8)) / span));
            var val = BitConverter.ToUInt32(buffer);
            while (val >= t) //Rare but may happen, doing so prevents any bias
            {
                Console.WriteLine($"Re-rolling to prevent bias: val{val} t{t}");
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