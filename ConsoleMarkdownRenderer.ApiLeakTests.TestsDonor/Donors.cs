// This assembly only donates types. Every type is intentionally empty; the
// ConsoleMarkdownRenderer.ApiLeakTests.TestsConsumer assembly references each one
// exactly once, through a single distinct "leak vector", so an API-leak
// completeness test can confirm that ApiLeakChecker discovers every donated
// type. A donated type that is never discovered points straight at the vector
// (its name) that the checker is blind to.

namespace ConsoleMarkdownRenderer.ApiLeakTests.TestsDonor;

public class DonorBaseType { }
public interface DonorInterface { }
public class DonorBaseGenericDefinition<T> { }
public class DonorBaseGenericArg { }
public interface DonorGenericInterface<T> { }
public class DonorInterfaceGenericArg { }
public class DonorTypeConstraint { }
public interface DonorConstraintInterface { }
public class DonorMethodTypeConstraint { }
public class DonorDelegateConstraint { }
public class DonorDelegateReturn { }
public class DonorDelegateParam { }
public class DonorCtorParam { }
public class DonorMethodReturn { }
public class DonorMethodParam { }
public class DonorPropertyType { }
public class DonorIndexerParam { }
public class DonorFieldType { }
public class DonorStaticFieldType { }
public class DonorArrayElement { }
public class DonorJaggedArrayElement { }
public class DonorMultiDimArrayElement { }
public class DonorParamsElement { }
public class DonorByRefParam { }
public class DonorInParam { }
public class DonorOutParam { }
public class DonorGenericDefinition<T> { }
public class DonorGenericArgument { }
public class DonorFuncArg { }
public class DonorTupleA { }
public class DonorTupleB { }
public struct DonorNullableStruct { }
public struct DonorPointerElement { }
public class DonorFunctionPointerReturn { }
public class DonorFunctionPointerParam { }
public delegate void DonorEventHandler();
public class DonorEventArgsType { }
public delegate void DonorProtectedEventHandler();
public class DonorOperatorReturn { }
public class DonorOperatorParam { }
public class DonorConversionSource { }
public class DonorConversionTarget { }
public class DonorInterfaceMethodReturn { }
public class DonorNestedMemberType { }
public class DonorProtectedNestedBase { }
public class DonorProtectedNestedMember { }
public class DonorProtectedCtorParam { }
public class DonorProtectedMethodReturn { }
public class DonorProtectedField { }
public class DonorProtectedProperty { }
public delegate void DonorProtectedInternalEventHandler();
public class DonorProtectedInternalCtorParam { }
public class DonorProtectedInternalMethodReturn { }
public class DonorProtectedInternalField { }
public class DonorProtectedInternalProperty { }
public class DonorProtectedInternalNestedBase { }
public enum DonorDefaultEnumParam { Value = 0 }
public enum DonorAttrCtorEnumArgument { Value = 0 }
public enum DonorAttrNamedEnumArgument { Value = 0 }
public enum DonorAttrNamedFieldEnum { Value = 0 }
public enum DonorAttrObjectEnumArgument { Value = 0 }
public sealed class DonorTypeAttribute : Attribute { }
public sealed class DonorTypeParamAttribute : Attribute { }
public sealed class DonorCtorAttribute : Attribute { }
public sealed class DonorCtorParamAttribute : Attribute { }
public sealed class DonorMethodAttribute : Attribute { }
public sealed class DonorMethodParamAttribute : Attribute { }
public sealed class DonorMethodReturnAttribute : Attribute { }
public sealed class DonorMethodTypeParamAttribute : Attribute { }
public sealed class DonorPropertyAttribute : Attribute { }
public sealed class DonorPropertyAccessorAttribute : Attribute { }
public sealed class DonorFieldAttribute : Attribute { }
public sealed class DonorEventAttribute : Attribute { }
public sealed class DonorEventAccessorAttribute : Attribute { }
public sealed class DonorAssemblyAttribute : Attribute { }
public sealed class DonorModuleAttribute : Attribute { }
public sealed class DonorEnumArgAttribute : Attribute
{
    public DonorEnumArgAttribute(DonorAttrCtorEnumArgument value) { }
}
public sealed class DonorTypeArgAttribute : Attribute
{
    public DonorTypeArgAttribute(Type type) { }
}
public sealed class DonorTypeArrayArgAttribute : Attribute
{
    public DonorTypeArrayArgAttribute(Type[] types) { }
}
public sealed class DonorNamedEnumAttribute : Attribute
{
    public DonorAttrNamedEnumArgument Mode { get; set; }
}
public sealed class DonorNamedTypeAttribute : Attribute
{
    public Type? NamedType { get; set; }
}
public sealed class DonorNamedFieldEnumAttribute : Attribute
{
    public DonorAttrNamedFieldEnum Mode;
}
public sealed class DonorObjectArgAttribute : Attribute
{
    public DonorObjectArgAttribute(object value) { }
}
public sealed class DonorNamedObjectAttribute : Attribute
{
    public object? Value { get; set; }
}
public class DonorAttrTypeofArgument { }
public class DonorAttrArrayTypeofArgument { }
public class DonorAttrNamedTypeofArgument { }
public class DonorAttrObjectTypeofArgument { }
public class DonorAttrNamedObjectTypeofArgument { }
