using System.Reflection;

namespace MessengerRando.Utils
{
    public static class ReflectionHelpers
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static T GetPrivateField<T>(this object o, string fieldName)
        {
            var field = o.GetType().GetField(fieldName, Flags);
            if (field != null) return (T)field.GetValue(o);
            return default;
        }

        public static void SetPrivateField(this object o, string fieldName, object value)
        {
            var field = o.GetType().GetField(fieldName, Flags);
            field?.SetValue(o, value);
        }

        public static void InvokeMethod(this object o, string methodName, object[] parameters = null)
        {
            o.GetType().GetMethod(methodName, Flags)?.Invoke(o, parameters ?? []);
        }
    }
}