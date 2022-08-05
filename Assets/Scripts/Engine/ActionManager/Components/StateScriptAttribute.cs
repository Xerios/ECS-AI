using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UtilityAI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StateScriptAttribute : System.Attribute
    {
        public readonly string Name;

        public StateScriptAttribute([CallerMemberName] string name = null)
        {
            this.Name = name;
        }
    }

    public static class StateScriptInteractionsHelper
    {
        public static Dictionary<string, MethodInfo> GetMethods ()
        {
            return System.Reflection.Assembly
                   .GetAssembly(typeof(StateScriptAttribute))
                   .GetTypes()
                   .SelectMany(x => x.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                   .Where(x => x.GetCustomAttributes(typeof(StateScriptAttribute), true).Length == 1)
                   .ToDictionary(method => ((StateScriptAttribute)method.GetCustomAttributes(true).FirstOrDefault()).Name);
        }
    }
}