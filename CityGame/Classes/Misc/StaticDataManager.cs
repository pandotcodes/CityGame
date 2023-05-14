using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityGame.Classes.Misc
{
    public class StaticData
    {
        protected object obj;
        static List<StaticData> list = new List<StaticData>();
        public static void Reset()
        {
            list.ForEach(x => x.obj = null);
        }
    }
    public class StaticData<T> : StaticData
    {
        public StaticData(T obj)
        {
            this.obj = obj;
        }
        public T Value { get => (T)obj; }
        public static implicit operator StaticData<T>(T obj)
        {
            return new StaticData<T>(obj);
        }
        public static implicit operator T(StaticData<T> d)
        {
            return d.Value;
        }
    }
}
