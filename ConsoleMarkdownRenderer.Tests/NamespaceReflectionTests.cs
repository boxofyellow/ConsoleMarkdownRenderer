using System.Reflection;
using BoxOfYellow.ConsoleMarkdownRenderer.Fakes;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Reflection-based guards that lock in the public API conventions for the NuGet
    /// packages produced by this repository:
    ///
    /// 1. Every type declared in the <c>BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers</c>
    ///    namespace is internal (no class, interface, enum, struct, or delegate is publicly visible).
    /// 2. Every namespace declared by a publicly visible type in either NuGet-published assembly
    ///    starts with <c>BoxOfYellow.</c>
    /// </summary>
    [TestClass]
    public class NamespaceReflectionTests
    {
        private const string ObjectRenderersNamespace = "BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers";
        private const string RequiredNamespacePrefix = "BoxOfYellow.";

        /// <summary>
        /// The <see cref="MarkdownDisplayer"/> assembly is the
        /// <c>BoxOfYellow.ConsoleMarkdownRenderer</c> NuGet package.
        /// </summary>
        private static Assembly RendererAssembly => typeof(MarkdownDisplayer).Assembly;

        /// <summary>
        /// The <see cref="FakeMarkdownDisplayer"/> assembly is the
        /// <c>BoxOfYellow.ConsoleMarkdownRenderer.Fakes</c> NuGet package.
        /// </summary>
        private static Assembly FakesAssembly => typeof(FakeMarkdownDisplayer).Assembly;

        [TestMethod]
        public void ObjectRenderers_Namespace_HasNoPubliclyVisibleTypes()
        {
            // Cover both the renderer and fakes assemblies — neither should ever
            // expose anything in BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers.
            var offenders = new[] { RendererAssembly, FakesAssembly }
                .SelectMany(a => a.GetTypes())
                .Where(t => t.Namespace == ObjectRenderersNamespace)
                .Where(IsPubliclyVisible)
                .Select(t => $"{t.Assembly.GetName().Name}::{t.FullName}")
                .ToList();

            Assert.AreEqual(
                0,
                offenders.Count,
                $"All types in '{ObjectRenderersNamespace}' must be internal. " +
                $"Found publicly visible types: {string.Join(", ", offenders)}");
        }

        [TestMethod]
        public void RendererAssembly_AllNamespaces_StartWithBoxOfYellowPrefix()
            => AssertAllPublicNamespacesStartWithBoxOfYellow(RendererAssembly);

        [TestMethod]
        public void FakesAssembly_AllNamespaces_StartWithBoxOfYellowPrefix()
            => AssertAllPublicNamespacesStartWithBoxOfYellow(FakesAssembly);

        private static void AssertAllPublicNamespacesStartWithBoxOfYellow(Assembly assembly)
        {
            // Inspect every publicly visible type (including nested public types) and verify
            // that the namespace it lives in starts with "BoxOfYellow." — every namespace
            // owned by these NuGet packages should sit under that prefix.
            var offenders = assembly.GetTypes()
                .Where(IsPubliclyVisible)
                .Where(t => !string.IsNullOrEmpty(t.Namespace))
                .Where(t => !t.Namespace!.StartsWith(RequiredNamespacePrefix, StringComparison.Ordinal))
                .Select(t => $"{t.FullName} (namespace: {t.Namespace})")
                .Distinct()
                .ToList();

            Assert.AreEqual(
                0,
                offenders.Count,
                $"All publicly visible types in '{assembly.GetName().Name}' must live under a " +
                $"'{RequiredNamespacePrefix}' namespace. Offenders: {string.Join(", ", offenders)}");
        }

        /// <summary>
        /// A type is "publicly visible" when it (and every enclosing type, for nested types) is
        /// declared <c>public</c>. <see cref="Type.IsVisible"/> captures both conditions.
        /// </summary>
        private static bool IsPubliclyVisible(Type type) => type.IsVisible;
    }
}
