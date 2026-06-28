using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

public class ApiLeakChecker
{
    public static bool CheckForLeaks(Assembly assembly, Dictionary<Type, bool> allowedLeaks)
    {
        var checker = new ApiLeakChecker(assembly, allowedLeaks);
        checker.CheckForLeaksImplementation();
        return checker._hasLeaks;
     }


    private readonly Assembly _assembly;
    private readonly Dictionary<Type, bool> _allowedLeaks;
    private bool _hasLeaks = false;

    private ApiLeakChecker(Assembly assembly, Dictionary<Type, bool> allowedLeaks)
    {
        _assembly = assembly;
        _allowedLeaks = allowedLeaks;
    }


    private void CheckForLeaksImplementation()
    {
        foreach (var type in GetExternallyVisibleTypes(_assembly))
        {
            InspectType(type);
        }
    }

    private IEnumerable<Type> GetExternallyVisibleTypes(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (IsExternallyVisibleType(type))
            {
                yield return type;
            }
        }
    }

    private bool IsExternallyVisibleType(Type type) 
        => type.IsPublic || (type.IsNestedPublic && AreContainingTypesPublic(type));

    private bool AreContainingTypesPublic(Type type)
    {
        for (var current = type.DeclaringType; current != null; current = current.DeclaringType)
        {
            if (!(current.IsPublic || current.IsNestedPublic))
            {
                return false;
            }
        }
        return true;
    }

    private void InspectType(Type type)
    {
        CheckReferencedType(type, $"type {type.FullName}");
        InspectCustomModifiers(type, $"type {type.FullName}");

        if (type.BaseType != null)
        {
            CheckReferencedType(type.BaseType, $"{type.FullName} base type");
        }

        foreach (var implementedInterface in type.GetInterfaces())
        {
            CheckReferencedType(
                implementedInterface,
                $"{type.FullName} implemented interface");
        }

        foreach (var genericParam in type.GetGenericArguments())
        {
            InspectGenericParameter(
                genericParam,
                $"{type.FullName} generic parameter '{genericParam.Name}'");
        }

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(type),
            $"{type.FullName} attribute");

        if (typeof(MulticastDelegate).IsAssignableFrom(type.BaseType))
        {
            InspectDelegateSignature(type);
        }

        const BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        foreach (var constructor in type.GetConstructors(flags))
        {
            if (IsExternallyVisibleConstructor(constructor))
            {
                InspectConstructor(type, constructor);
            }
        }

        foreach (var method in type.GetMethods(flags))
        {
            if (IsExternallyVisibleMethod(method))
            {
                InspectMethod(type, method);
            }
        }

        foreach (var property in type.GetProperties(flags))
        {
            if (IsExternallyVisibleProperty(property))
            {
                InspectProperty(type, property);
            }
        }

        foreach (var field in type.GetFields(flags))
        {
            if (IsExternallyVisibleField(field))
            {
                InspectField(type, field);
            }
        }

        foreach (var evt in type.GetEvents(flags))
        {
            if (IsExternallyVisibleEvent(evt))
            {
                InspectEvent(type, evt);
            }
        }
    }

    private void InspectDelegateSignature(Type delegateType)
    {
        var invokeMethod = delegateType.GetMethod(
            "Invoke",
            BindingFlags.Instance | BindingFlags.Public);

        if (invokeMethod != null)
        {
            InspectMethod(delegateType, invokeMethod);
        }
    }

    private void InspectConstructor(
        Type declaringType,
        ConstructorInfo constructor)
    {
        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(constructor),
            $"{declaringType.FullName}.ctor attribute");

        InspectCustomModifiers(constructor, $"{declaringType.FullName}.ctor");

        foreach (var parameter in constructor.GetParameters())
        {
            InspectParameter(
                parameter,
                $"{declaringType.FullName}.ctor parameter '{parameter.Name}'");
        }
    }

    private void InspectMethod(
        Type declaringType,
        MethodInfo method)
    {
        CheckReferencedType(
            method.ReturnType,
            $"{declaringType.FullName}.{method.Name} return type");

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(method),
            $"{declaringType.FullName}.{method.Name} attribute");

        InspectCustomModifiers(method, $"{declaringType.FullName}.{method.Name}");

        foreach (var genericArg in method.GetGenericArguments())
        {
            InspectGenericParameter(
                genericArg,
                $"{declaringType.FullName}.{method.Name} generic parameter '{genericArg.Name}'");
        }

        foreach (var parameter in method.GetParameters())
        {
            InspectParameter(
                parameter,
                $"{declaringType.FullName}.{method.Name} parameter '{parameter.Name}'");
        }

        var returnParameter = method.ReturnParameter;
        if (returnParameter != null)
        {
            InspectAttributes(
                CustomAttributeData.GetCustomAttributes(returnParameter),
                $"{declaringType.FullName}.{method.Name} return attribute");

            InspectParameterCustomModifiers(
                returnParameter,
                $"{declaringType.FullName}.{method.Name} return");
        }
    }

    private void InspectProperty(
        Type declaringType,
        PropertyInfo property)
    {
        CheckReferencedType(
            property.PropertyType,
            $"{declaringType.FullName}.{property.Name} property type");

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(property),
            $"{declaringType.FullName}.{property.Name} property attribute");

        InspectCustomModifiers(property, $"{declaringType.FullName}.{property.Name} property");

        foreach (var indexParameter in property.GetIndexParameters())
        {
            InspectParameter(
                indexParameter,
                $"{declaringType.FullName}.{property.Name} indexer parameter '{indexParameter.Name}'");
        }

        foreach (var accessor in property.GetAccessors(nonPublic: true).Where(IsExternallyVisibleMethod))
        {
            InspectAttributes(
                CustomAttributeData.GetCustomAttributes(accessor),
                $"{declaringType.FullName}.{property.Name} accessor '{accessor.Name}' attribute");

            InspectCustomModifiers(
                accessor,
                $"{declaringType.FullName}.{property.Name} accessor '{accessor.Name}'");
        }
    }

    private void InspectField(
        Type declaringType,
        FieldInfo field)
    {
        CheckReferencedType(
            field.FieldType,
            $"{declaringType.FullName}.{field.Name} field type");

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(field),
            $"{declaringType.FullName}.{field.Name} field attribute");

        InspectCustomModifiers(field, $"{declaringType.FullName}.{field.Name} field");
    }

    private void InspectEvent(
        Type declaringType,
        EventInfo evt) 
    {
        if (evt.EventHandlerType != null)
        {
            CheckReferencedType(
                evt.EventHandlerType,
                $"{declaringType.FullName}.{evt.Name} event type");
        }

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(evt),
            $"{declaringType.FullName}.{evt.Name} event attribute");

        var addMethod = evt.GetAddMethod(nonPublic: true);
        var removeMethod = evt.GetRemoveMethod(nonPublic: true);
        var raiseMethod = evt.GetRaiseMethod(nonPublic: true);

        foreach (var accessor in new[] { addMethod, removeMethod, raiseMethod }
            .Where(m => m != null)
            .Where(m => IsExternallyVisibleMethod(m!)))
        {
            InspectAttributes(
                CustomAttributeData.GetCustomAttributes(accessor!),
                $"{declaringType.FullName}.{evt.Name} accessor '{accessor!.Name}' attribute");

            InspectCustomModifiers(
                accessor!,
                $"{declaringType.FullName}.{evt.Name} accessor '{accessor!.Name}'");
        }
    }

    private void InspectGenericParameter(
        Type genericParameter,
        string context)
    {
        foreach (var constraint in genericParameter.GetGenericParameterConstraints())
        {
            CheckReferencedType(
                constraint,
                $"{context} constraint");
        }

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(genericParameter),
            $"{context} attribute");
    }

    private void InspectParameter(
        ParameterInfo parameter,
        string context)
    {
        CheckReferencedType(parameter.ParameterType, context);

        InspectAttributes(
            CustomAttributeData.GetCustomAttributes(parameter),
            $"{context} attribute");

        InspectParameterCustomModifiers(parameter, context);

        if (parameter.HasDefaultValue)
        {
            if (parameter.ParameterType.IsEnum)
            {
                CheckReferencedType(
                    parameter.ParameterType,
                    $"{context} default value enum type");
            }

            if (parameter.DefaultValue is Type defaultType)
            {
                CheckReferencedType(
                    defaultType,
                    $"{context} default typeof value");
            }
        }
    }

    private void InspectAttributes(
        IEnumerable<CustomAttributeData> attributes,
        string context)
    {
        foreach (var attribute in attributes)
        {
            CheckReferencedType(attribute.AttributeType, context);

            foreach (var constructorArgument in attribute.ConstructorArguments)
            {
                InspectCustomAttributeArgument(
                    constructorArgument,
                    $"{context} constructor argument");
            }

            foreach (var namedArgument in attribute.NamedArguments)
            {
                CheckReferencedType(
                    namedArgument.TypedValue.ArgumentType,
                    $"{context} named argument '{namedArgument.MemberName}' type");

                InspectCustomAttributeArgument(
                    namedArgument.TypedValue,
                    $"{context} named argument '{namedArgument.MemberName}'");
            }
        }
    }

    private void InspectCustomAttributeArgument(
        CustomAttributeTypedArgument argument,
        string context)
    {
        if (argument.ArgumentType != null)
        {
            CheckReferencedType(argument.ArgumentType, $"{context} argument type");
        }

        if (argument.Value is Type typeValue)
        {
            CheckReferencedType(typeValue, $"{context} typeof value");
        }

        if (argument.Value is IReadOnlyCollection<CustomAttributeTypedArgument> collection)
        {
            foreach (var item in collection)
            {
                InspectCustomAttributeArgument(item, $"{context} array element");
            }
        }
    }

    private void InspectCustomModifiers(MemberInfo member, string context)
    {
        switch (member)
        {
            case MethodInfo method:
                InspectParameterCustomModifiers(method.ReturnParameter, $"{context} return");
                foreach (var parameter in method.GetParameters())
                {
                    InspectParameterCustomModifiers(
                        parameter,
                        $"{context} parameter '{parameter.Name}'");
                }
                break;

            case PropertyInfo property:
                foreach (var indexParameter in property.GetIndexParameters())
                {
                    InspectParameterCustomModifiers(
                        indexParameter,
                        $"{context} indexer parameter '{indexParameter.Name}'");
                }
                break;

            case FieldInfo field:
                foreach (var modifierType in field.GetOptionalCustomModifiers())
                {
                    CheckReferencedType(
                        modifierType,
                        $"{context} optional custom modifier"); 
                }

                foreach (var modifierType in field.GetRequiredCustomModifiers())
                {
                    CheckReferencedType(
                        modifierType,
                        $"{context} required custom modifier");
                }
                break;
        }
    }

    private void InspectParameterCustomModifiers(
        ParameterInfo parameter,
        string context)
    {
        foreach (var modifierType in parameter.GetOptionalCustomModifiers())
        {
            CheckReferencedType(
                modifierType,
                $"{context} optional custom modifier");
        }

        foreach (var modifierType in parameter.GetRequiredCustomModifiers())
        {
            CheckReferencedType(
                modifierType,
                $"{context} required custom modifier");
        }
    }

    private void CheckReferencedType(Type type, string context)
    {
        foreach (var referencedType in FlattenType(type))
        {
            // Generic parameters don't count as leaks since they don't actually expose any particular type.
            if (referencedType.IsGenericParameter)
            {
                continue;
            }

            // Types from the same assembly don't count as leaks.
            if (referencedType.Assembly == _assembly)
            {
                continue;
            }

            // Anything in the System namespace is allowed to be leaked.
            if (string.Compare(referencedType.Namespace, "System", StringComparison.Ordinal) == 0 ||
                (referencedType.Namespace?.StartsWith("System.", StringComparison.Ordinal) ?? false))
            {
                continue;
            }

            if (_allowedLeaks.ContainsKey(referencedType))
            {
                _allowedLeaks[referencedType] = false;
                continue;
            }
            else
            {
                Logger.LogMessage($"{context} exposes {referencedType} from assembly '{referencedType.Assembly.GetName().Name}'{Environment.NewLine}");
                _hasLeaks = true;
            }
        }
    }

    private IEnumerable<Type> FlattenType(Type type)
    {
        if (type.IsByRef || type.IsPointer || type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType != null)
            {
                foreach (var inner in FlattenType(elementType))
                {
                    yield return inner;
                }
            }

            yield break;
        }

        yield return type;

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            yield return genericTypeDefinition;

            foreach (var arg in type.GetGenericArguments())
            {
                foreach (var inner in FlattenType(arg))
                {
                    yield return inner;
                }
            }
        }
    }

    private bool IsExternallyVisibleConstructor(ConstructorInfo constructor)
    {
        return constructor.IsPublic || constructor.IsFamily;
    }

    private bool IsExternallyVisibleMethod(MethodInfo method)
    {
        return method.IsPublic || method.IsFamily;
    }

    private bool IsExternallyVisibleProperty(PropertyInfo property)
    {
        return property.GetAccessors(nonPublic: true).Any(IsExternallyVisibleMethod);
    }

    private bool IsExternallyVisibleField(FieldInfo field)
    {
        return field.IsPublic || field.IsFamily;
    }

    private bool IsExternallyVisibleEvent(EventInfo evt)
    {
        return new[]
        {
            evt.GetAddMethod(nonPublic: true),
            evt.GetRemoveMethod(nonPublic: true),
            evt.GetRaiseMethod(nonPublic: true)
        }
        .Where(m => m != null)
        .Any(m => IsExternallyVisibleMethod(m!));
    }
}