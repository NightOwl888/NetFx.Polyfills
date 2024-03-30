using System.Reflection;

#if !FEATURE_RUNTIMEINFORMATION
namespace System.Runtime.InteropServices
{
    public static class RuntimeInformation
    {
        private const string FrameworkName = ".NET Framework";

        private static string s_frameworkDescription;

        public static string FrameworkDescription
        {
            get
            {
                if (s_frameworkDescription == null)
                {
                    AssemblyFileVersionAttribute assemblyFileVersionAttribute = (AssemblyFileVersionAttribute)typeof(object).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute));
                    s_frameworkDescription = FrameworkName + assemblyFileVersionAttribute.Version;
                }

                return s_frameworkDescription;
            }
        }
    }
}
#endif