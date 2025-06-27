using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NoDev.Common.Storage;
using NoDev.Infinity.Security;
using NoDev.InfinityToolLib;
using NoDev.InfinityToolLib.Tools;

namespace NoDev.Infinity.Tools
{
    internal class ToolFactory
    {
        private static readonly Type ToolType = typeof(ITool);
        private static readonly Type LiteralCollectionType = typeof (ILiteralCollection);

        private readonly Dictionary<string, ITool> _toolControls;
        private readonly IToolImageRetriever _imageRetriever;

        internal ToolFactory(IToolImageRetriever imageRetriever)
        {
            _imageRetriever = imageRetriever;
            _toolControls = new Dictionary<string, ITool>();
        }

        internal ITool GetToolInstance(string toolId, ILiteralCollection literals = null)
        {
            if (_toolControls.ContainsKey(toolId))
                return _toolControls[toolId];

            var imageLocation = _imageRetriever.RetrieveToolImageLocation(toolId);

            if (imageLocation == null || !AssemblyValidator.IsNoDevAssembly(imageLocation))
                return null;

            var tool = InstantiateTool(imageLocation, toolId, literals);

            if (tool != null)
            {
                tool.SetSettings(new DictionarySettingsStorage(
                    Storage.GetInstance(Environment.SpecialFolder.LocalApplicationData).GetFilePath("Tools\\" + toolId)
                ));
            }
            
            _toolControls.Add(toolId, tool);

            return tool;
        }

        private static ITool InstantiateTool(string imageLocation, string toolId, ILiteralCollection literals)
        {
            var asm = Assembly.UnsafeLoadFrom(imageLocation);

            var guidAttr = asm.GetCustomAttribute<GuidAttribute>();

            if (guidAttr == null)
            {
#if DEBUG
                throw new Exception(string.Format("Tool assembly GUID missing ({0}).", toolId));
#else
                return null;
#endif
            }

            if (guidAttr.Value != toolId)
            {
#if DEBUG
                throw new Exception(string.Format("Tool assembly GUID ({0}) does not match the tool ID ({1}).", guidAttr.Value, toolId));
#else
                return null;
#endif
            }
            
            var types = asm.GetTypes();

            var toolType = types.FirstOrDefault(t => t.IsClass && t.GetInterfaces().Contains(ToolType));

            if (toolType == null)
            {
#if DEBUG
                throw new Exception(string.Format("Tool panel not found in tool assembly ({0}).", toolId));
#else
                return null;
#endif
            }

            var ctor = toolType.GetConstructor(new Type[0]);

            if (ctor == null)
            {
#if DEBUG
                throw new Exception(string.Format("Valid constructor not found for tool ({0}).", toolId));
#else
                return null;
#endif
            }

            if (literals == null)
                goto returnTool;

            var literalClass = types.FirstOrDefault(t =>
                t.IsClass && t.IsAbstract && t.IsSealed
                && t.FullName == "NoDev.__Dynamic.Literals");

            if (literalClass == null)
                goto returnTool;

            var methods = literalClass.GetMethods(BindingFlags.Public);

            foreach (var method in methods)
            {
                var ps = method.GetParameters();

                if (ps.Length != 1 || ps[0].ParameterType != LiteralCollectionType)
                    continue;

                method.Invoke(null, new object[] { literals });

                break;
            }

            returnTool:
            return (ITool)ctor.Invoke(null);
        }
    }
}
