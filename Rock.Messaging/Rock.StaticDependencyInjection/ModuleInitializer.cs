// A ModuleInitializer class is required by the IL weaving mechanism used by
// Rock.Messaging.Rock.StaticDependencyInjection. It can be modified in any way as long as
// the following conditions are met:
//  - The name of the class must be exactly "ModuleInitializer".
//    - The class must not be nested.
//    - The class may be marked as either public or internal.
//    - The static, sealed, and abstract modifiers can be used and have
//      no effect on the class.
//  - The ModuleInitializer class must have a method named exactly "Run".
//    - The method must not be private or protected.
//      - It may marked as public, internal, or protected internal.
//    - The method must be marked as static.
//    - The method must return void.
//    - The method must take no arguments.
//  - The Run method must contain the following lines:
//        var compositionRoot = new CompositionRoot();
//
//        if (compositionRoot.IsEnabled)
//        {
//            compositionRoot.Bootstrap();
//        }

namespace Rock.Messaging.Rock.StaticDependencyInjection
{
    internal static class ModuleInitializer
    {
        internal static void Run()
        {
            var compositionRoot = new CompositionRoot();

            if (compositionRoot.IsEnabled)
            {
                compositionRoot.Bootstrap();
            }
        }
    }
}
