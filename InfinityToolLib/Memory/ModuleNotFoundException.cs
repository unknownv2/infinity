using System;

namespace NoDev.InfinityToolLib.Memory
{
    [Serializable]
    internal class ModuleNotFoundException : Exception
    {
        internal ModuleNotFoundException()
        {
            
        }

        internal ModuleNotFoundException(string moduleName)
            : base(string.Format("Could not locate module '{0}' in process.", moduleName))
        {
            
        }
    }
}
