using System;

namespace UtilityAI
{
    [Flags]
    public enum ParametersType : short {
        None = 1,
        Range = 1 << 1,
        Value = 1 << 2,
        Property = 1 << 3,
        Boolean = 1 << 4,
        Ability = 1 << 5
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ConsiderationAttribute : System.Attribute
    {
        public readonly string Name;
        public readonly int Order;
        public readonly ParametersType Parameters = ParametersType.None;
        public readonly bool Cache = false;

        public ConsiderationAttribute(int order, string name, ParametersType parameters, bool cache = false)
        {
            this.Name = name;
            this.Order = order;
            this.Parameters = parameters;
            this.Cache = cache;
        }
    }
}