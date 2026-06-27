using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class NamedTypeCollectionTests
    {
        private record class C1(int Value)
        {
            public static C1 One { get; } = new(1);
            public static C1 Two { get; } = new C2();
            public static C1 Three { get; } = new C3();

            public C1 Four { get; } = C4.Instance;       // Not static, should be ignored
            private static C1 Five { get; } = new C5();  // Not public, should be ignored
            internal static C1 Six { get; } = new C6();  // Internal, should be included
            public static C7 Seven { get; } = new C7();  // Wrong type, should be ignored
            public static void X() { Assert.IsNotNull(Five); }
        }

        private static class Other
        {
            public static C1 Eight { get; } = new C8(); // Can be added, if the type is added
        }

        private static class Bad1
        {
            public static C1 One { get; } = new C9();  // Duplicate name
        }

        private static class Bad2
        {
            public static C1 Bad_2 { get; } = C1.One; // Duplicate value
        }   

        private static class Bad3
        {
            public static C1 Bad_3 { get; } = new(10); // Duplicate  type
        }

        private record class C2() : C1(2) { }
        private record class C3() : C1(3) { }
        private record class C4() : C1(4) { public readonly static C4 Instance = new(); }
        private record class C5() : C1(5) { }
        private record class C6() : C1(6) { }
        private record class C7() : C1(7) { }
        private record class C8() : C1(8) { }
        private record class C9() : C1(9) { }
        // 10 is used for duplicate type test

        [TestMethod]
        [DataRow(1, typeof(C1), nameof(C1.One))]
        [DataRow(2, typeof(C2), nameof(C1.Two))]
        [DataRow(3, typeof(C3), nameof(C1.Three))]
        public void Can_Make_From_One_Type(int value, Type type, string name) 
            => AssertMatches(new NamedTypeCollection<C1>(Mappings.GetPropertyValues<C1>([typeof(C1)])), value, type, name, count: 3);

        [TestMethod]
        [DataRow(1, typeof(C1), nameof(C1.One))]
        [DataRow(2, typeof(C2), nameof(C1.Two))]
        [DataRow(3, typeof(C3), nameof(C1.Three))]
        [DataRow(8, typeof(C8), nameof(Other.Eight))]
        public void Can_Make_From_One_Multiple_Types(int value, Type type, string name) 
            => AssertMatches(new NamedTypeCollection<C1>(Mappings.GetPropertyValues<C1>([typeof(C1), typeof(Other)])), value, type, name, count: 4);

        [TestMethod]
        [DataRow(8, typeof(C8), nameof(Other.Eight))]
        [DataRow(9, typeof(C9), nameof(Bad1.One))]
        public void Can_Make_From_One_Multiple_Different_Types(int value, Type type, string name) 
            => AssertMatches(new NamedTypeCollection<C1>(Mappings.GetPropertyValues<C1>([typeof(Other), typeof(Bad1)])), value, type, name, count: 2);

        [TestMethod]
        [DataRow(typeof(Bad1))]
        [DataRow(typeof(Bad2))]
        [DataRow(typeof(Bad3))]
        public void Should_Throw_If_Duplicate(Type badType)
            => Assert.Throws<ArgumentException>(() => new NamedTypeCollection<C1>(Mappings.GetPropertyValues<C1>([typeof(C1), badType])));

        private static void AssertMatches(NamedTypeCollection<C1> collection, int value, Type type, string name, int count)
        {
            Assert.HasCount(count, collection.NameMap.Forward);
            Assert.HasCount(count, collection.TypeNameMap.Forward);

            Assert.IsTrue(collection.NameMap.Forward.TryGetValue(name, out var forwardNameValue));
            TestUtilities.AssertTheseMatch(value, forwardNameValue.Value, shouldMatch: true);
            TestUtilities.AssertTheseMatch(type, forwardNameValue.GetType(), shouldMatch: true);

            Assert.IsTrue(collection.TypeNameMap.Forward.TryGetValue(type.Name, out var forwardTypeNameValue));
            TestUtilities.AssertTheseMatch(value, forwardTypeNameValue.Value, shouldMatch: true);
            TestUtilities.AssertTheseMatch(type, forwardTypeNameValue.GetType(), shouldMatch: true);

            TestUtilities.AssertTheseMatch(forwardNameValue, forwardTypeNameValue, shouldMatch: true);

            Assert.IsTrue(collection.NameMap.Reverse.TryGetValue(forwardNameValue, out var reverseNameKey));
            TestUtilities.AssertTheseMatch(name, reverseNameKey, shouldMatch: true);

            Assert.IsTrue(collection.TypeNameMap.Reverse.TryGetValue(forwardTypeNameValue, out var reverseTypeNameKey));
            TestUtilities.AssertTheseMatch(type.Name, reverseTypeNameKey, shouldMatch: true);
        }
    }
}