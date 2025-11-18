using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace Shared.Tools;

public static class AIToolsFactory
{
    public static IList<AITool> GetToolsFromAttribute(object instance)
    {
        Type type = instance.GetType();
        IEnumerable<MethodInfo> methods = GetMethodsWithAttribute(type);
        AITool[] tools = methods.Select(methodInfo =>
        {
            AiToolDetailsAttribute details = methodInfo.GetCustomAttribute<AiToolDetailsAttribute>()!;
            return AIFunctionFactory.Create(methodInfo, instance, name: details.Name, description: details.Description);
        }).Cast<AITool>().ToArray();
        return tools;
    }

    public static IList<AITool> GetToolsFromMethods(object instance, AIToolsFactoryMethodOptions? options = null)
    {
        options ??= new AIToolsFactoryMethodOptions();
        Type type = instance.GetType();
        IEnumerable<MethodInfo> methods = GetMethods(type, options);
        AITool[] tools1 = methods.Select(methodInfo =>
        {
            string name = options.MakeNamesSnakeCase ? ToSnakeCase(methodInfo.Name) : methodInfo.Name;
            string? description = options.IncludeDescriptions ? methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description : null;
            return AIFunctionFactory.Create(methodInfo, instance, name: name, description: description);
        }).Cast<AITool>().ToArray();
        AITool[] tools = tools1;
        return tools;

        static string ToSnakeCase(string input)
        {
            var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
            return result.ToLower();
        }
    }

    private static IEnumerable<MethodInfo> GetMethods(Type type, AIToolsFactoryMethodOptions options)
    {
        IEnumerable<MethodInfo> methods = [];

        if (options.IncludePublicMethods)
        {
            BindingFlags flags = GetBindingFlags(BindingFlags.Instance | BindingFlags.Public);
            methods = methods.Union(type.GetMethods(flags));
        }

        if (options.IncludeNonPublicMethods)
        {
            BindingFlags flags = GetBindingFlags(BindingFlags.Instance | BindingFlags.NonPublic);
            methods = methods.Union(type.GetMethods(flags));
        }

        if (options.IncludePublicStaticMethods)
        {
            BindingFlags flags = GetBindingFlags(BindingFlags.Static | BindingFlags.Public);
            methods = methods.Union(type.GetMethods(flags));
        }

        if (options.IncludeNonPublicStaticMethods)
        {
            BindingFlags flags = GetBindingFlags(BindingFlags.Static | BindingFlags.NonPublic);
            methods = methods.Union(type.GetMethods(flags));
        }

        return methods;

        BindingFlags GetBindingFlags(BindingFlags flags)
        {
            if (options.DeclaredOnly)
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            else
            {
                flags |= BindingFlags.FlattenHierarchy;
            }

            return flags;
        }
    }

    private static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type type)
    {
        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.FlattenHierarchy);

        return methods.Where(x => x.GetCustomAttribute<AiToolDetailsAttribute>() != null).ToList();
    }
}