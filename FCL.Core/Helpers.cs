using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCL.Core
{
    class Helpers
    {
    }

    public class Null
    {
        private static readonly Null Instance = new Null();

        private Null() { }

        public static Null OrEmpty
        {
            get { return Instance; }
        }

        public static bool operator ==(IEnumerable collection, Null n)
        {
            return collection == null || !collection.Cast<object>().Any();
        }

        public static bool operator !=(IEnumerable collection, Null n)
        {
            return !(collection == n);
        }

        public static bool operator ==(Null n, IEnumerable collection)
        {
            return collection == null || !collection.Cast<object>().Any();
        }

        public static bool operator !=(Null n, IEnumerable collection)
        {
            return !(collection == n);
        }

        public static bool operator ==(string s, Null n)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool operator !=(string s, Null n)
        {
            return !(s == n);
        }

        public static bool operator ==(Null n, string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool operator !=(Null n, string s)
        {
            return !(s == n);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return true;

            var asNull = obj as Null;
            if (asNull != null) return true;

            var asString = obj as string;
            if (asString != null) return asString == Instance;

            var asEnumerable = obj as IEnumerable;
            if (asEnumerable != null) return asEnumerable == Instance;

            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
