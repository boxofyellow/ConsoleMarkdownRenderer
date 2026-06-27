using System.Reflection;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Fakes.Support
{
    [FakeSourceFile]
    internal static class DisplayOptionsExtensions
    {
        public static SpectreDisplayOptions ToSpectreOptions(this DisplayOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            var result = ToSpectreOptionsMethod.Invoke(options, []) 
                       ?? throw new ApplicationException($"The method '{FakesConstants.ToSpectreOptionsName}' on {typeof(DisplayOptions)} returned null. It was expected to return a non-null instance of {typeof(SpectreDisplayOptions)}.");
            return (SpectreDisplayOptions)result;
        }

        private static readonly MethodInfo ToSpectreOptionsMethod = GetToSpectreOptionsMethod();
        
        private static MethodInfo GetToSpectreOptionsMethod()
        {
            var method = typeof(DisplayOptions).GetMethod(FakesConstants.ToSpectreOptionsName, BindingFlags.NonPublic | BindingFlags.Instance)
                       ?? throw new ApplicationException($"The method '{FakesConstants.ToSpectreOptionsName}' was not found on {typeof(DisplayOptions)}.");
            if (method.ReturnType != typeof(SpectreDisplayOptions))
            {
                throw new ApplicationException($"The method '{FakesConstants.ToSpectreOptionsName}' on {typeof(DisplayOptions)} has an unexpected return type. {typeof(SpectreDisplayOptions)} was expected but found {method.ReturnType}");
            }
            if (method.GetParameters().Length != 0)
            {
                throw new ApplicationException($"The method '{FakesConstants.ToSpectreOptionsName}' on {typeof(DisplayOptions)} has unexpected parameters. It was expected to have no parameters but found {method.GetParameters().Length}.");
            }
            if (method.IsGenericMethod)
            {
                throw new ApplicationException($"The method '{FakesConstants.ToSpectreOptionsName}' on {typeof(DisplayOptions)} is a generic method. It was expected to be a non-generic method.");
            }
            return method;
        }
    }
}