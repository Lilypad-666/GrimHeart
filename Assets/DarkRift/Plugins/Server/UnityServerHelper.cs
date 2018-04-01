using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
///     Various shared methods for use in UnityServer and UnityServerEditor.
/// </summary>
public static class UnityServerHelper
{
    /// <summary>
    ///     Searches the app domain for plugin types. 
    /// </summary>
    /// <returns>The plugin types in the app domain.</returns>
    public static IEnumerable<Type> SearchForPlugins()
    {
        //Omit DarkRift server assembly so internal plugins aren't loaded twice
        Assembly[] omit = new Assembly[]
        {
            Assembly.GetAssembly(typeof(DarkRiftServer))
        };

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Except(omit))
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Plugin)))
                    yield return type;
            }
        }
    }
}
