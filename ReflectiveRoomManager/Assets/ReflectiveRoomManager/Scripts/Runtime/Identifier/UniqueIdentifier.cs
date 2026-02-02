using System;

namespace REFLECTIVE.Runtime.Identifier
{
    public class UniqueIdentifier
    {
        private uint _nextId;
        private readonly uint _xorKey;

        public UniqueIdentifier()
        {
            var random = new Random();
            _xorKey = (uint)random.Next(1, int.MaxValue);
        }

        public uint CreateID() => ++_nextId ^ _xorKey;
    }
}
