using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeNet;
using PeNet.Structures;

namespace DllToExeInjector
{
    static class PeFileExtensions
    {
        public static IMAGE_SECTION_HEADER DefineSectionByRva(this PeFile file, long rva)
        {
            var alignment = file.ImageNtHeaders.OptionalHeader.SectionAlignment;
            for (int i = 0; i < file.ImageSectionHeaders.Length; i++)
            {
                uint start = file.ImageSectionHeaders[i].VirtualAddress;
                uint end = start + file.ImageSectionHeaders[i].VirtualSize.AlignUp(alignment);
                if( (rva >= start) && (rva < end))
                {
                    return file.ImageSectionHeaders[i];
                }
            }
            return null;
        }

        public static IMAGE_SECTION_HEADER DefineSectionByOffset(this PeFile file, long offset)
        {
            var alignment = file.ImageNtHeaders.OptionalHeader.SectionAlignment;
            for (int i = 0; i < file.ImageSectionHeaders.Length; i++)
            {
                uint start = file.ImageSectionHeaders[i].PointerToRawData;
                uint end = start + file.ImageSectionHeaders[i].SizeOfRawData;
                if ((offset >= start) && (offset < end))
                {
                    return file.ImageSectionHeaders[i];
                }
            }
            return null;
        }

        public static long ConvertRvaToOffset(this PeFile file, long rva)
        {
            if (file.DefineSectionByRva(rva) is IMAGE_SECTION_HEADER section)
            {
                return rva - section.VirtualAddress + section.PointerToRawData;
            }
            return rva;//Do additional checks
        }

        public static long ConvertOffsetToRva(this PeFile file, long offset)
        {
            if (file.DefineSectionByOffset(offset) is IMAGE_SECTION_HEADER section)
            {
                return offset + section.VirtualAddress - section.PointerToRawData;
            }
            return offset;//Do additional checks
        }

        public static void MoveImportTable(this PeFile file, long newOffset)
        {
            var importDirectoryOffset = BitConverter.ToInt32(file.Buff, 0x3c) + 0x80;
            var oldRva =(uint) BitConverter.ToInt32(file.Buff, importDirectoryOffset);
            uint oldOffset = (uint)file.ConvertRvaToOffset(oldRva);
            var size = BitConverter.ToInt32(file.Buff, importDirectoryOffset + 4);
            for (int i=0; i < size; i++)
            {
                file.Buff[i + newOffset] = file.Buff[i + oldOffset];
                file.Buff[i + oldOffset] = 0;
            }

            var newRva = (uint)file.ConvertOffsetToRva(newOffset);
            file.Buff[importDirectoryOffset] = (byte)(newRva & (0xFF));
            file.Buff[importDirectoryOffset+1] = (byte)((newRva & (0xFF00)) >> 8);
            file.Buff[importDirectoryOffset+2] = (byte)((newRva & (0xFF0000)) >> 16);
            file.Buff[importDirectoryOffset+3] = (byte)((newRva & (0xFF000000)) >> 24);
        }

        public static void Save(this PeFile file, string name)
        {
            System.IO.File.WriteAllBytes(name, file.Buff);
        }

    }
}
