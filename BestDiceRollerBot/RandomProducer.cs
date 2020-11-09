using System;
using System.Security.Cryptography;
using Antlr4.Build.Tasks;

namespace BestDiceRollerBot
{
    public class RandomProducer
    {
        private readonly RNGCryptoServiceProvider _gen;

        public RandomProducer()
        {
            _gen = new RNGCryptoServiceProvider();
        }

        public int Get(int inMin, int exMax)
        {
            var buffer = new byte[32];
            lock (_gen)
            {
                _gen.GetBytes(buffer);
            }
            var span = exMax - inMin;
            var val = BitConverter.ToUInt32(buffer);
            return (int)((val % span) + inMin);
        }
    }
}