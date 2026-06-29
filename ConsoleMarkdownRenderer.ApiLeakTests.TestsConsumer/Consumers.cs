// This assembly exposes every type from ConsoleMarkdownRenderer.ApiLeakTests.TestsDonor
// through the public API surface, each donor type used in exactly one place via a
// distinct "leak vector". None of these shapes need to be functional; they only
// need to compile. See Donors.cs for the matching list of donor types.

using ConsoleMarkdownRenderer.ApiLeakTests.TestsDonor;

[assembly: DonorAssembly]

namespace ConsoleMarkdownRenderer.ApiLeakTests.TestsConsumer;

public class BaseTypeConsumer : DonorBaseType { }
public class InterfaceConsumer : DonorInterface { }
public class BaseGenericConsumer : DonorBaseGenericDefinition<int> { }
public class BaseGenericParameterConsumer : List<DonorBaseGenericArg> { }
public class InterfaceGenericConsumer : DonorGenericInterface<int> { }
public class InterfaceGenericParameterConsumer : List<DonorInterfaceGenericArg> { }
public class TypeConstraintConsumer<T> where T : DonorTypeConstraint { }
public class InterfaceConstraintConsumer<T> where T : DonorConstraintInterface { }
public class TypeParameterAttributeConsumer<[DonorTypeParam] T> { }
[DonorType] public class TypeAttributeConsumer { }
public delegate DonorDelegateReturn DelegateConsumer(DonorDelegateParam parameter);
public delegate void GenericDelegateConsumer<T>() where T : DonorDelegateConstraint;
public class ConstructorConsumer
{
    [DonorCtor] public ConstructorConsumer() { }
    public ConstructorConsumer(DonorCtorParam parameter) { }
    public ConstructorConsumer([DonorCtorParam] int value) { }
}
public class MethodConsumer
{
    public DonorMethodReturn ReturnType() => throw null!;
    public void Parameter(DonorMethodParam parameter) { }
    [DonorMethod] public void Attribute() { }
    public void ParameterAttribute([DonorMethodParam] int value) { }
    [return: DonorMethodReturn] public void ReturnAttribute() { }
    public void GenericConstraint<T>() where T : DonorMethodTypeConstraint { }
    public void GenericParameterAttribute<[DonorMethodTypeParam] T>() { }
    public void DefaultEnum(DonorDefaultEnumParam value = default) { }
    public void ByRef(ref DonorByRefParam value) => value = null!;
    public void In(in DonorInParam value) { }
    public void Out(out DonorOutParam value) => value = null!;
    public void Params(params DonorParamsElement[] values) { }
    public (DonorTupleA First, DonorTupleB Second) Tuple() => default;
    public Func<DonorFuncArg> FuncReturn() => throw null!;
    public DonorNullableStruct? Nullable() => null;
}
public class PropertyConsumer
{
    public DonorPropertyType PropertyType { get; set; } = null!;
    [DonorProperty] public int Attribute { get; set; }
    public int this[DonorIndexerParam index] => 0;
    public int AccessorAttribute { [DonorPropertyAccessor] get => 0; set { } }
}
public class FieldConsumer
{
    public DonorFieldType FieldType = null!;
    public static DonorStaticFieldType StaticField = null!;
    [DonorField] public int Attribute;
    public DonorArrayElement[] Array = null!;
    public DonorJaggedArrayElement[][] Jagged = null!;
    public DonorMultiDimArrayElement[,] MultiDim = null!;
    public DonorGenericDefinition<int> Generic = null!;
    public List<DonorGenericArgument> GenericArg = null!;
}
public unsafe class UnsafeConsumer
{
    public DonorPointerElement* Pointer;
    public delegate*<DonorFunctionPointerParam, DonorFunctionPointerReturn> FunctionPointer;
}
public class EventConsumer
{
    public event DonorEventHandler HandlerType = null!;
    public event EventHandler<DonorEventArgsType> GenericHandler = null!;
    [DonorEvent] public event EventHandler Attribute = null!;
    public event EventHandler AccessorAttribute
    {
        [DonorEventAccessor] add { }
        remove { }
    }
}
public class OperatorConsumer
{
    public static DonorOperatorReturn operator +(OperatorConsumer left, OperatorConsumer right) => throw null!;
    public static int operator -(OperatorConsumer left, DonorOperatorParam right) => 0;
    public static implicit operator DonorConversionTarget(OperatorConsumer value) => throw null!;
    public static implicit operator OperatorConsumer(DonorConversionSource value) => throw null!;
}
public interface IMemberLeakConsumer
{
    DonorInterfaceMethodReturn Method();
}
public class OuterConsumer
{
    public class NestedConsumer
    {
        public DonorNestedMemberType Field = null!;
    }
}
public class AttributeArgumentConsumer
{
    [DonorEnumArg(DonorAttrCtorEnumArgument.Value)] public void ConstructorEnumArgument() { }
    [DonorTypeArg(typeof(DonorAttrTypeofArgument))] public void ConstructorTypeArgument() { }
    [DonorTypeArrayArg([typeof(DonorAttrArrayTypeofArgument)])] public void ConstructorTypeArrayArgument() { }
    [DonorNamedEnum(Mode = DonorAttrNamedEnumArgument.Value)] public void NamedEnumArgument() { }
    [DonorNamedType(NamedType = typeof(DonorAttrNamedTypeofArgument))] public void NamedTypeArgument() { }
    [DonorNamedFieldEnum(Mode = DonorAttrNamedFieldEnum.Value)] public void NamedFieldEnumArgument() { }
    [DonorObjectArg(typeof(DonorAttrObjectTypeofArgument))] public void ConstructorObjectTypeofArgument() { }
    [DonorNamedObject(Value = typeof(DonorAttrNamedObjectTypeofArgument))] public void NamedObjectTypeofArgument() { }
    [DonorObjectArg(DonorAttrObjectEnumArgument.Value)] public void ConstructorObjectEnumArgument() { }
}
public class ProtectedConsumer
{
    protected ProtectedConsumer(DonorProtectedCtorParam parameter) { }
    protected DonorProtectedMethodReturn Method() => throw null!;
    protected DonorProtectedField Field = null!;
    protected DonorProtectedProperty Property { get; set; } = null!;
    protected event DonorProtectedEventHandler Event = null!;
}
public class ProtectedInternalConsumer
{
    protected internal ProtectedInternalConsumer(DonorProtectedInternalCtorParam parameter) { }
    protected internal DonorProtectedInternalMethodReturn Method() => throw null!;
    protected internal DonorProtectedInternalField Field = null!;
    protected internal DonorProtectedInternalProperty Property { get; set; } = null!;
    protected internal event DonorProtectedInternalEventHandler Event = null!;
}
public class ProtectedNestedConsumer
{
    protected class NestedProtectedConsumer : DonorProtectedNestedBase
    {
        public DonorProtectedNestedMember Field = null!;
    }

    protected internal class NestedProtectedInternalConsumer : DonorProtectedInternalNestedBase { }
}
