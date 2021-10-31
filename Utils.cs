using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Project.Classes {
    public static class Utils {
        public static Random Random = new Random();

        public static T Max<T>(params T[] elems) where T: IComparable {
            return elems.Max();
        }

        public static T Min<T>(params T[] elems) where T: IComparable {
            return elems.Min();
        }

        public static bool IsEven(this int num) => num % 2 == 0;
        public static bool IsOdd(this int num) => num % 2 == 1;

        public static List<T> ToList<T>(this T[,] arr) {
            var result = new List<T>();
            var n = arr.GetLength(0);
            var m = arr.GetLength(1);
            for (var i = 0; i < n; i++) {
                for (var j = 0; j < m; j++) {
                    result.Add(arr[i, j]);
                }
            }

            return result;
        }

        public static T GetRandom<T>(this List<T> list) => list[Random.Next(list.Count)];

        public static T[,] DeepCopy<T>(this T[,] obj) where T : ICloneable {
            var n = obj.GetLength(0);
            var m = obj.GetLength(1);
            var result = new T[n, m];
            for (var i = 0; i < n; i++) {
                for (var j = 0; j < m; j++) {
                    result[i, j] = (T) obj[i, j].Clone();
                }
            }

            return result;
        }

        public static T GetNextCycled<T>(this IEnumerator<T> enumerator) {
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }

            enumerator.Reset();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public static Action
            AddHandlerOnIndex(this Action eventHandler, int index, Action newHandler) {
            Action result = null;
            var handlers = eventHandler?.GetInvocationList()
                .OfType<Action>()
                .ToList();
            var handlersLen = handlers?.Count ?? 0;
            if (index >= handlersLen) {
                result = eventHandler;
                result += newHandler;
            }
            else {
                for (var i = 0; i < handlersLen; i++) {
                    if (i == index) {
                        result += newHandler;
                    }

                    result += handlers[i];
                }
            }

            return result;
        }

        public static bool HasSameContent<T>(this List<T> list1, List<T> list2) {
            var list1Count = list1.Count;
            if (list1Count != list2.Count) {
                return false;
            }

            for (var i = 0; i < list1Count; i++) {
                var index = list2.IndexOf(list1[i]);
                if (index == -1) {
                    return false;
                }

                if (list2.Count(elem => elem.Equals(list1[i])) != list1.Count(elem => elem.Equals(list1[i]))) {
                    return false;
                }
            }

            return true;
        }
    }

    public class Pair<T, V> {
        public T TElem { get; private set; }
        public V GElem { get; private set; }

        public Pair(T tElem, V gElem) {
            TElem = tElem;
            GElem = gElem;
        }
    }

    public class PairsList<T, V> : IEnumerable<Pair<T,V>> {
        private List<Pair<T, V>> _pairs = new List<Pair<T, V>>();

        public void AddPair(Pair<T, V> pair) {
            _pairs.Add(pair);
        }

        public void AddPair(T tElem, V vElem) {
            _pairs.Add(new Pair<T, V>(tElem, vElem));
        }

        public Pair<T, V> GetPair(int index) {
            return _pairs[index];
        }

        public IEnumerator<Pair<T, V>> GetEnumerator() {
            return _pairs.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}