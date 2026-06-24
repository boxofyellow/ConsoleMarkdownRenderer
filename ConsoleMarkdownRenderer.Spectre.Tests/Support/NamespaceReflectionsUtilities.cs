using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    public static class NamespaceReflectionsUtilities
    {
        private const string RequiredNamespacePrefix = "BoxOfYellow.";

        public static void AssertAllNamespacesStartWithBoxOfYellow(Assembly assembly)
        {
            // Inspect every type (including nested types) and verify
            // that the namespace it lives in starts with "BoxOfYellow." — every namespace
            // owned by these NuGet packages should sit under that prefix.
            var offenders = assembly.GetTypes()
                .Where(t => !string.IsNullOrEmpty(t.Namespace))
                .Where(t => !t.Namespace!.StartsWith(RequiredNamespacePrefix, StringComparison.Ordinal))
                .Select(t => $"{t.FullName} (namespace: {t.Namespace})")
                .Distinct()
                .ToList();

            Assert.HasCount(
                0,
                offenders,
                $"All types in '{assembly.GetName().Name}' must live under a " +
                $"'{RequiredNamespacePrefix}' namespace. Offenders: {string.Join(", ", offenders)}");
        }

        public static void AssertAllNamespacesOfPublicTypesAreInValidateNamesSpaces(Assembly assembly, HashSet<string> validNamespaces)
        {
            // Inspect every publicly visible type (including nested public types) and verify
            // that the namespace it lives in is one of the expected namespaces.
            var offenders = assembly.GetTypes()
                .Where(IsPubliclyVisible)
                .Where(t => !string.IsNullOrEmpty(t.Namespace))
                .Where(t => !validNamespaces.Contains(t.Namespace!))
                .Select(t => $"{t.FullName} (namespace: {t.Namespace})")
                .Distinct()
                .ToList();

            Assert.HasCount(
                0,
                offenders,
                $"All publicly visible types in '{assembly.GetName().Name}' must live under a " +
                $"valid namespace. Offenders: {string.Join(", ", offenders)}");
        }

        private static bool IsPubliclyVisible(Type type) => type.IsVisible;
    }
}