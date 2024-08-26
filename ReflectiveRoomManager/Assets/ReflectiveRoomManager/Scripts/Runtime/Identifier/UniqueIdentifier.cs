using System;
using System.Linq;
using System.Collections.Generic;

namespace REFLECTIVE.Runtime.Identifier
{
    /// <summary>
    /// Document: https://reflective-roommanager.gitbook.io/docs/user-manual/identifier/uniqueidentifier
    /// </summary>
    public class UniqueIdentifier
    {
        private readonly Random _random = new();

        private readonly HashSet<uint> _usedIds = new();

        private readonly int _idLength;

        public UniqueIdentifier(int idLength)
        {
            _idLength = idLength;
        }
        
        public bool IsIDUnique(uint ID)
        {
            return !_usedIds.Contains(ID);
        }
        
        public uint CreateID()
        {
            uint newID;

            do
            {
                newID = GenerateUniqueID();
            }
            while(!IsIDUnique(newID));

            _usedIds.Add(newID);

            return newID;
        }

        private uint GenerateUniqueID()
        {
            //Get the all digits to list
            var digits = Enumerable.Range(0, 10).ToArray();

            //Mix the digits with the fisher-yates shuffle algorithm
            var n = digits.Length;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                (digits[k], digits[n]) = (digits[n], digits[k]);
            }

            //Create an id as long as id length
            uint generateUniqueID = 0;
            for (var i = 0; i < _idLength; i++)
            {
                generateUniqueID = generateUniqueID * 10 + (uint)digits[i % _idLength];
            }

            return generateUniqueID;
        }
    }
}