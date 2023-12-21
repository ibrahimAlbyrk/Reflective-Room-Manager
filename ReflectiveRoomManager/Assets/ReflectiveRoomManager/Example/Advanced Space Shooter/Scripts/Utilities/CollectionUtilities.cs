using System;
using System.Collections;
using Mirror;
using System.Linq;
using System.Collections.Generic;

namespace Examples.SpaceShooter.Utilities
{
    public static class CollectionUtilities
    {
        #region List

        public static void RemoveAll<T>(this List<T> list)
        {
            foreach (var element in list.ToList())
            {
                list.Remove(element);
            }
        }

        #endregion

        #region Array

        public static void RemoveAll<T>(this T[] array)
        {
            var list = array.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                list.RemoveAt(i);
            }

            // ReSharper disable once RedundantAssignment
            array = list.ToArray();
        }

        #endregion

        #region IEnumerable

        public static T GetElement<T>(this IEnumerable<T> collection, int atIndex)
        {
            var index = 0;
            foreach (var element in collection)
            {
                if (index == atIndex) return element;

                index++;
            }

            throw new ArgumentOutOfRangeException(nameof(atIndex), $"Expected value less then {index}");
        }

        public static int GetLenght<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case null:
                    throw new ArgumentException(nameof(source));
                case ICollection<T> sources:
                    return sources.Count;
                case ICollection collection:
                    return collection.Count;
                default:
                    var num = 0;
                    using (var enumerator = source.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                            checked
                            {
                                ++num;
                            }
                    }

                    return num;
            }
        }

        public static SyncList<T> ToSyncList<T>(this IEnumerable<T> collection)
        {
            var syncList = new SyncList<T>();

            foreach (var element in collection)
            {
                syncList.Add(element);
            }

            return syncList;
        }

        #endregion
    }
}