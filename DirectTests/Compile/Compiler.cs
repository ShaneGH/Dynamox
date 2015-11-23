﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Mocks;
using Microsoft.CSharp;

namespace DirectTests.Compile
{
    public class Compiler
    {
        private readonly Dictionary<Type, Type> Built = new Dictionary<Type, Type>();
        const string _ObjectBase = "_ObjectBase";

        private Compiler()
        {
        }

        Type _Compile(Type baseType)
        {
            if (!Built.ContainsKey(baseType))
                CompileAndCache(baseType);

            return Built[baseType];
        }

        static readonly ConcurrentDictionary<Type, Tuple<string, IEnumerable<Assembly>>> NameCache = new ConcurrentDictionary<Type, Tuple<string, IEnumerable<Assembly>>>();
        static string GetFullTypeName(Type type, out IEnumerable<Assembly> assemblies)
        {
            if (!NameCache.ContainsKey(type))
            {
                if (!type.FullName.Contains("`"))
                {
                    NameCache.TryAdd(type, new Tuple<string, IEnumerable<Assembly>>(
                        "global::" + type.FullName.Replace("+", "."),
                        Array.AsReadOnly<Assembly>(type.Assembly == null ? new Assembly[] { } : new[] { type.Assembly })));
                }
                else
                {
                    IEnumerable<Assembly> dummy;
                    var ass = new List<Assembly>();
                    var generics = new List<string>();
                    var output = type.FullName.Substring(0, type.FullName.IndexOf("`"));
                    foreach (var generic in type.GetGenericArguments())
                    {
                        generics.Add(GetFullTypeName(generic, out dummy));
                        ass.AddRange(dummy);
                    }

                    NameCache.TryAdd(type, new Tuple<string, IEnumerable<Assembly>>(
                        "global::" + output.Replace("+", ".") + "<" + string.Join(", ", generics) + ">",
                        Array.AsReadOnly<Assembly>(ass.Where(a => a != null).Distinct().ToArray())));
                }
            }

            Tuple<string, IEnumerable<Assembly>> result;
            NameCache.TryGetValue(type, out result);
            assemblies = result.Item2;
            return result.Item1;
        }

        void CompileAndCache(Type baseType)
        {
            if (baseType == null)
                baseType = typeof(object);

            if (baseType.IsSealed)
                throw new InvalidOperationException("Cannot mock sealed");  //TODO

            IEnumerable<Assembly> dummy;
            var _assemblies = Enumerable.Empty<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies = _assemblies.Union(dummy);
                return typeName;
            };

            var @class = new StringBuilder("namespace DirectTests.Proxy." + baseType.Namespace);//TODO: no namespace
            @class.AppendLine("{");
            @class.AppendLine("public class " + baseType.Name + ": " + getFullTypeName(baseType));  //TODO: sealed class, generic parameters
            @class.AppendLine("{");

            @class.AppendLine("readonly " + getFullTypeName(typeof(ObjectBase)) + " " + _ObjectBase + ";");

            @class.AppendLine(GetConstructors(baseType, out dummy));
            _assemblies = _assemblies.Union(dummy.Where(d => d != null));

            @class.AppendLine(GetProperties(baseType, out dummy));
            _assemblies = _assemblies.Union(dummy.Where(d => d != null));

            @class.AppendLine(GetMethods(baseType, out dummy));
            _assemblies = _assemblies.Union(dummy.Where(d => d != null));

            @class.AppendLine("}");
            @class.AppendLine("}");

            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.AddRange(_assemblies.Select(a => a.Location).ToArray());
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, @class.ToString());

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }

            Assembly assembly = results.CompiledAssembly;
            Built[baseType] = assembly.GetType("DirectTests.Proxy." + baseType.Namespace + "." + baseType.Name);   //TODO: no namespace
        }

        static string GetConstructors(Type baseType, out IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> dummy;
            var _assemblies = new List<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies.AddRange(dummy);
                return typeName;
            };

            var constructors = !baseType.IsInterface ?
                baseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(c => !c.IsPrivate && !c.IsAssembly && !c.IsFamilyOrAssembly)
                    .Select(c => c.GetParameters().Select(p => p.ParameterType)) :
                    new[] { Enumerable.Empty<Type>() };

            if (!constructors.Any())
                throw new InvalidOperationException("No public constructors");  //TODO

            var arg1 = new[]{ typeof(ObjectBase)};
            var output = new StringBuilder();
            foreach (var constructor in constructors)
            {
                output.AppendLine("public " + baseType.Name + //TODO, class name
                    "(" +
                    string.Join(", ", arg1.Concat(constructor).Select((a, i) => getFullTypeName(a) + " arg" + i)) +
                    ")");

                output.AppendLine(": base(" +
                    string.Join(", ", constructor.Select((a, i) => "arg" + (i + 1))) + ")");

                output.AppendLine("{");
                output.AppendLine("this." + _ObjectBase + " = arg0;");
                output.AppendLine("}");
            }

            assemblies = _assemblies.AsReadOnly();
            return output.ToString();
        }

        static string GetProperties(Type baseType, out IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> dummy;
            var _assemblies = new List<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies.AddRange(dummy);
                return typeName;
            };


            bool isProtected;
            var required = baseType.AllClassesAndInterfaces().Where(c => !c.IsInterface);
            var properties = required.SelectMany(c =>
                c.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetAccessors(true).Any(a => !a.IsPrivate && !a.IsAssembly && !a.IsFamilyOrAssembly && (a.IsVirtual || a.IsAbstract))).Select(p => new
                    {
                        isProtected = isProtected = ((p.GetMethod == null || p.GetMethod.IsFamily) && (p.SetMethod == null || p.SetMethod.IsFamily)),
                        getIsProtected = !isProtected && p.GetMethod != null && p.GetMethod.IsFamily,
                        setIsProtected = !isProtected && p.SetMethod != null && p.SetMethod.IsFamily,
                        isOverride = true,
                        isAbstract = p.GetAccessors(true).Any(a => a.IsAbstract),
                        interfaceName = string.Empty,
                        property = p
                    }));


            required = baseType.AllClassesAndInterfaces().Where(c => c.IsInterface);
            properties = properties.Union(required.SelectMany(i => i.GetProperties().Select(p => new
            {
                isProtected = false,
                getIsProtected = false,
                setIsProtected = false,
                isOverride = false,
                isAbstract = true,
                interfaceName = getFullTypeName(i) + ".",
                property = p
            })));

            var output = new StringBuilder();
            foreach (var property in properties)
            {
                var returnType = getFullTypeName(property.property.PropertyType);
                output.AppendLine(
                    (property.isProtected ? "protected " : property.isOverride ? "public " : string.Empty) +
                    (property.isOverride ? "override " : "") +
                    returnType + " " +
                    property.interfaceName + property.property.Name);

                output.AppendLine("{");

                if (property.property.GetMethod != null && 
                    !property.property.GetMethod.IsPrivate && 
                    !property.property.GetMethod.IsAssembly && 
                    !property.property.GetMethod.IsFamilyOrAssembly)
                {
                    output.AppendLine((property.getIsProtected ? "protected " : "") + "get");
                    output.AppendLine("{");
                    if (property.isAbstract)
                    {
                        output.AppendLine("return this." + _ObjectBase + ".GetProperty<" + returnType + ">(\"" + property.property.Name + "\");");
                    }
                    else
                    {
                        output.AppendLine(returnType + " val;");
                        output.AppendLine("if (this." + _ObjectBase + ".TryGetProperty<" + returnType + ">(\"" + property.property.Name + "\", out val))");
                        output.AppendLine("return val;");
                        output.AppendLine();
                        output.AppendLine("return base." + property.property.Name + ";");
                    }

                    output.AppendLine("}");
                }

                if (property.property.SetMethod != null &&
                    !property.property.SetMethod.IsPrivate &&
                    !property.property.SetMethod.IsAssembly &&
                    !property.property.SetMethod.IsFamilyOrAssembly)
                {
                    output.AppendLine((property.setIsProtected ? "protected " : "") + "set");
                    output.AppendLine("{");
                    if (property.isAbstract)
                    {
                        output.AppendLine("this." + _ObjectBase + ".SetProperty(\"" + property.property.Name + "\", value);");
                    }
                    else
                    {
                        output.AppendLine(returnType + " val;");
                        output.AppendLine("if (this." + _ObjectBase + ".TryGetProperty<" + returnType + ">(\"" + property.property.Name + "\", out val))");
                        output.AppendLine("this." + _ObjectBase + ".SetProperty(\"" + property.property.Name + "\", value);");
                        output.AppendLine("else");
                        output.AppendLine("base." + property.property.Name + " = value;");
                    }
                    
                    output.AppendLine("}");
                }

                output.AppendLine("}");
            }

            assemblies = _assemblies.AsReadOnly();
            return output.ToString();
        }

        static string GetMethods(Type baseType, out IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> dummy;
            var _assemblies = new List<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies.AddRange(dummy);
                return typeName;
            };

            var required = baseType.GetInterfaces() as IEnumerable<Type>;
            if (baseType.IsInterface)
                required = required.Union(new[] { baseType });

            var methods = required.SelectMany(i => i.GetMethods().Select(m => new
            {
                @interface = getFullTypeName(i),
                method = m
            })).ToArray();

            var kvp = getFullTypeName(typeof(KeyValuePair<Type, object>));
            var output = new StringBuilder();
            foreach (var method in methods)
            {
                var returnType = method.method.ReturnType == typeof(void) ? "void" : getFullTypeName(method.method.ReturnType);
                output.AppendLine(
                    returnType + " " +
                    method.@interface + "." + method.method.Name +
                    "(" + string.Join(", ", method.method.GetParameters().Select((p, i) => getFullTypeName(p.ParameterType) + " arg" + i)) + ")");
                output.AppendLine("{");

                output.AppendLine((returnType == "void" ? "" : "return ") +
                    "this." + _ObjectBase + ".Invoke" +
                    (returnType == "void" ? "" : ("<" + returnType + ">")) +
                    "(\"" + method.method.Name + "\", " + 
                    " new " + kvp + "[]" + " {" +
                    string.Join(", ", method.method.GetParameters().Select((p, i) => "new " + kvp + "(typeof(" + getFullTypeName(p.ParameterType)  +  "), arg" + i + ")")) + 
                    "});");

                output.AppendLine("}");
            }

            assemblies = _assemblies.AsReadOnly();
            return output.ToString();
        }


        static readonly Compiler Instance = new Compiler();
        public static Type Compile(Type baseType) 
        {
            return Instance._Compile(baseType);
        }
    }
}
