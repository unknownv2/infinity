using System;
using System.IO;
using System.Runtime.InteropServices;
using NoDev.Common.IO;
using NoDev.InfinityToolLib.Security;

namespace NoDev.InfinityToolLib.Memory
{
    public class ModuleImageParser
    {
        // cache this
        private static readonly int PageSize = Environment.SystemPageSize;

        private readonly uint _peRva;

        public ModuleImageParser(Stream moduleStream)
        {
            AssemblyProtection.EnsureProtected();

            var header = moduleStream.Read(PageSize);

            // MS-DOS "MZ" magic
            if (header.ReadUInt16() != 0x5A4D)
                throw new BadImageFormatException();

            // get PE image RVA from MS-DOS header
            _peRva = header.ReadUInt32(0x3C);

            // "PE\0\0" magic
            if (header.ReadUInt32((int)_peRva) != 0x00004550)
                throw new BadImageFormatException();

            var sectionCount = header.ReadUInt16((int)_peRva + 6);

            var sectionDescriptorRva = _peRva + 24 + header.ReadUInt16((int) _peRva + 20);

            var endRva = sectionDescriptorRva + (sectionCount * 40);

            if (endRva > PageSize)
            {
                var x = 2;

                while (endRva > (x * PageSize))
                    x++;

                moduleStream.Position = 0;
                header = moduleStream.Read(x * PageSize);
            }
            
            var io = new EndianIO(header);

            while (sectionCount-- != 0)
            {
                
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 40)]
    public struct SectionDescriptor
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)] public string Name;
        public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }
}
