﻿using System;
using System.Security.Cryptography;

namespace BestDiceRollerBot
{
    public class RandomProducer
    {
        private readonly RandomNumberGenerator _gen = RandomNumberGenerator.Create();
        private const byte GenSize = sizeof(uint);

        public int Get(int inMin, int exMax)
        {
            var buffer = new byte[GenSize];
            lock (_gen)
            {
                _gen.GetBytes(buffer);
            }
            var span = (uint)Math.Abs(exMax - inMin);
            var t = span * (Math.Floor(Math.Pow(2.0, (GenSize*8)) / span));
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
            return (int)r + inMin;
        }
    }
}