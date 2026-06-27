using System.Reflection;
using System.Runtime.CompilerServices;
using Markdig.Renderers;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using STHJS = System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    public static class ConventionsHelper
    {
        public static void FindViolations<TAttribute>(
            Type rootType,
            Func<TAttribute, string> extractPath,
            IEnumerable<Type> allowedPublicTypes,
            IEnumerable<string> allowedPublicFolders,
            IEnumerable<string> allowedClassFileNameMismatch,
            IEnumerable<string> allowedStaticFolders,
            IEnumerable<Type> allowedApiLeaks)
            where TAttribute : Attribute
        {
            bool hasViolations = false;
            var assembly = rootType.Assembly;

            var publicDic = allowedPublicTypes.ToDictionary(t => t, t => true);
            var allowedFoldersDic = allowedPublicFolders.ToDictionary(f => f, f => true);
            var allowedClassFileNameMismatchDic = allowedClassFileNameMismatch.ToDictionary(f => f, f => true);
            var allowedStaticFoldersDic = allowedStaticFolders.ToDictionary(f => f, f => true);
            var allowedApiLeaksDic = allowedApiLeaks.ToDictionary(t => t, t => true);

            var root = assembly.GetName().Name ?? string.Empty;
            if (!root.StartsWith("BoxOfYellow.ConsoleMarkdownRenderer"))
            {
                hasViolations = true;
                Logger.LogMessage("Assembly name does not start with the expected prefix.");
            }

            var rootPath = string.Empty;
            if (rootType.IsDefined(typeof(TAttribute), inherit: false))
            {
                var attribute = rootType.GetCustomAttribute<TAttribute>();
                if (attribute != null)
                {
                    rootPath = NormalizePath(extractPath(attribute));
                }
            }
            var rootDirectory = EnsureTrailingSeparator(Path.GetDirectoryName(rootPath));

            var types = assembly
                .GetTypes()
                // Exclude compiler-generated types (e.g. anonymous types, state machines) and nested types, as they won't have the attribute
                .Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false));

            foreach (var type in types)
            {
                var typeName = type.Name.Split('`')[0];

                if (!type.IsNested)
                {
                    // Exclude nested types, they parent can serve the purpose of the attribute
                    var attribute = type.GetCustomAttribute<TAttribute>();
                    if (attribute == null)
                    {
                        LogViolation($"The {typeof(TAttribute).Name} attribute is missing on {type.FullName}.");
                    }
                    else
                    {
                        var path = NormalizePath(extractPath(attribute));
                        Assert.IsFalse(string.IsNullOrEmpty(path), $"The {typeof(TAttribute).Name} attribute on {type.FullName} has an empty path.");
                        var name = Path.GetFileNameWithoutExtension(path);
                        if (typeName != name)
                        {
                            if (!CheckAllowed(allowedClassFileNameMismatchDic, name))
                            {
                                LogViolation($"The class name {typeName} does not match the file name {name}.");
                            }
                        }

                        // This is full path without the file name
                        var prefixPath = EnsureTrailingSeparator(Path.GetDirectoryName(path));

                        // The is the relative path from the root directory without the file name
                        string? postfixPath = null;
                        if (prefixPath.StartsWith(rootDirectory))
                        {
                            postfixPath = Path.TrimEndingDirectorySeparator(ExtractPostFix(rootDirectory, prefixPath));
                            if (postfixPath.Contains('.'))
                            {
                                LogViolation($"The file path postfix {postfixPath} contains a '.' character, which is invalid for {type.FullName}.");
                                postfixPath = null;
                            }
                        }
                        else
                        {
                            LogViolation($"The file path {prefixPath} does not start with the expected root directory {rootDirectory}.");
                            postfixPath = null;
                        }

                        // The is the namespace without the leading BoxOfYellow.ConsoleMarkdownRenderer, and all '.' replaced with directory separators
                        string? postfixNamespace = null;
                        if (type.Namespace?.StartsWith(root) ?? false)
                        {
                            postfixNamespace = ExtractPostFix(root, type.Namespace);
                            if (!string.IsNullOrEmpty(postfixNamespace))
                            {
                                if (postfixNamespace.StartsWith('.'))
                                {
                                    postfixNamespace = postfixNamespace
                                                        .Substring(1)
                                                        .Replace('.', Path.DirectorySeparatorChar);
                                }
                                else
                                {
                                    LogViolation($"The namespace postfix {postfixNamespace} is invalid for {type.FullName}.");
                                    postfixNamespace = null;
                                }
                            }
                        }
                        else
                        {
                            LogViolation($"The namespace prefix {type.Namespace} is invalid for {type.FullName}.");
                        }

                        if (postfixPath is not null && postfixNamespace is not null && postfixPath != postfixNamespace)
                        {
                            LogViolation($"The namespace postfix {postfixNamespace} does not match the file path postfix {postfixPath} for {type.FullName}.");
                        }

                        // This should name of the directory for the relative path
                        var last = postfixPath is null
                                 ? string.Empty 
                                 : Path.GetFileNameWithoutExtension(Path.TrimEndingDirectorySeparator(postfixPath));


                        if (type.IsAssignableTo(typeof(IMarkdownObjectRenderer)) || type.IsAssignableTo(typeof(IMarkdownRenderer)))
                        {
                            if (last != "ObjectRenderers")
                            {
                                LogViolation($"The ObjectRenderer class {type.FullName} is not located in an ObjectRenderers folder.");
                            }
                        }
                        // This is check for static... why no type.IsStatic 🤷
                        else if (type.IsClass && type.IsAbstract && type.IsSealed && !type.IsInterface)
                        {
                            if (last != "Support")
                            {
                                if (!CheckAllowed(allowedStaticFoldersDic, last))
                                {
                                    LogViolation($"The static class {type.FullName} is located in {last} folder.");
                                }
                            }
                        }
                        else if (type.IsAbstract && !type.IsInterface)
                        {
                            if (last != "Support")
                            {
                                LogViolation($"The abstract class {type.FullName} is not located in a Support folder.");
                            }
                        }
                        else if (type.IsAssignableTo(typeof(STHJS.JsonConverter)))
                        {
                            if (last != nameof(STHJS.JsonConverter))
                            {
                                LogViolation($"The JsonConverter class {type.FullName} is not located in a {nameof(STHJS.JsonConverter)} folder.");
                            }
                        }

                        if (type.IsPublic && last != string.Empty)
                        {
                            if (!CheckAllowed(allowedFoldersDic, last))
                            {
                                LogViolation($"The public class {type.FullName} is located in {last} folder");
                            }
                        }
                    }
                }

                if (type.IsClass && type.IsPublic && !type.IsSealed && !type.IsAbstract)
                {
                    LogViolation($"The public class {type.FullName} is not sealed.");
                }

                if (type.IsClass && type.IsAbstract && !type.IsSealed && !type.IsInterface && !typeName.EndsWith("Base"))
                {
                    LogViolation($"The abstract class {type.FullName} does not have a name ending with 'Base'.");
                }

                if (type.IsPublic)
                {
                    if (!CheckAllowed(publicDic, type))
                    {
                        LogViolation($"{type.FullName} is public.");
                    }
                }
            }

            hasViolations |= ApiLeakChecker.CheckForLeaks(assembly, allowedApiLeaksDic);

            CheckAllUsed(publicDic, nameof(allowedPublicTypes));
            CheckAllUsed(allowedFoldersDic, nameof(allowedPublicFolders));
            CheckAllUsed(allowedClassFileNameMismatchDic, nameof(allowedClassFileNameMismatch));
            CheckAllUsed(allowedStaticFoldersDic, nameof(allowedStaticFolders));
            CheckAllUsed(allowedApiLeaksDic, nameof(allowedApiLeaks));

            if (hasViolations)
            {
                Assert.Fail("Convention violations were found. See log for details.");
            }

            void LogViolation(string message)
            {
                hasViolations = true;
                Logger.LogMessage(message + Environment.NewLine);
            }

            bool CheckAllowed<T>(Dictionary<T, bool> dic, T key) where T : notnull
            {
                if (dic.ContainsKey(key))
                {
                    dic[key] = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            void CheckAllUsed<T>( Dictionary<T, bool> dic, string name) where T : notnull
            {
                var unused = dic.Where(kv => kv.Value).Select(kv => kv.Key).ToArray();
                if (unused.Length > 0)
                {
                    LogViolation($"The following overrides were not used in {name}: {string.Join(", ", unused)}");
                }
            }
        }

        private static string NormalizePath(string path)
        {
            return Path.DirectorySeparatorChar switch
            {
                '\\' => path.Replace('/', '\\'),
                '/' => path.Replace('\\', '/'),
                _ => throw new NotSupportedException($"Unsupported directory separator: {Path.DirectorySeparatorChar}"),
            };
        }

        private static string EnsureTrailingSeparator(string? path) 
            => Path.TrimEndingDirectorySeparator(path ?? string.Empty) + Path.DirectorySeparatorChar;

        private static string ExtractPostFix(string root, string path) 
            => path.Substring(root.Length);
    }
}