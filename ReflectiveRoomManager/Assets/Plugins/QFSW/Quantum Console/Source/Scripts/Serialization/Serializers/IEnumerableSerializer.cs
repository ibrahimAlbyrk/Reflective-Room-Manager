using System;
using System.Collections;
using System.Text;

namespace QFSW.QC.Serializers
{
    public class IEnumerableSerializer : IEnumerableSerializer<IEnumerable>
    {
        public override int Priority => base.Priority - 1000;

        protected override IEnumerable GetObjectStream(IEnumerable value)
        {
            return value;
        }
    }

    public abstract class IEnumerableSerializer<T> : PolymorphicQcSerializer<T> where T : class, IEnumerable
    {
        private readonly Pool<StringBuilder> _builderPool = new Pool<StringBuilder>();

        public override string SerializeFormatted(T value, QuantumTheme theme)
        {
            Type type = value.GetType();
            StringBuilder builder = _builderPool.GetObject();

            string left = "[";
            string seperator = ",";
            string right = "]";
            if (theme)
            {
                theme.GetCollectionFormatting(type, out left, out seperator, out right);
            }

            builder.Clear();
            builder.Append(left);

            bool firstIteration = true;
            foreach (object item in GetObjectStream(value))
            {
                if (firstIteration)
                {
                    firstIteration = false;
                }
                else
                {
                    builder.Append(seperator);
                }

                builder.Append(SerializeRecursive(item, theme));
            }

            builder.Append(right);

            string result = builder.ToString();
            _builderPool.Release(builder);
            return result;
        }

        protected abstract IEnumerable GetObjectStream(T value);
    }
}
