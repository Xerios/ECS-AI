using System;
using Unity.Entities;

namespace UtilityAI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsiderationDataAttribute : System.Attribute
    {
        public readonly Type[] Types;

        public ConsiderationDataAttribute(params Type[] types)
        {
            this.Types = types;
        }
    }
}