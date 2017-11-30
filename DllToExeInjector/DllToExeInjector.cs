using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeNet;
using PeNet.Structures;

namespace DllToExeInjector
{
    class DllToExeInjector
    {
        private const long _MIN_ZERO_BLOCK_SIZE = 20;
        private const int _END_OF_DATA_DIRECTORY_OFFSET = 0xf8;

        private IMAGE_SECTION_HEADER _FindWriteableSection(PeFile file)
        {
            foreach(var section in file.ImageSectionHeaders)
            {
                if((section.Characteristics & (uint)PeNet.Constants.SectionFlags.IMAGE_SCN_MEM_WRITE) != 0)
                {
                    return section;
                }
            }
            return null;
            
        }

        //TODO modify search algorithm
        private long _FindZeroBlock(PeFile file, long size, long startOffset = 0)
        {
            //long start = BitConverter.ToInt32(file.Buff, 0x3c) + _END_OF_DATA_DIRECTORY_OFFSET + file.ImageSectionHeaders.Length * 0x28;
            var section = _FindWriteableSection(file);
            long start = file.ConvertRvaToOffset(section.VirtualAddress);
            long end = start + section.VirtualSize.AlignUp(file.ImageNtHeaders.OptionalHeader.SectionAlignment);
            long candidate = (startOffset == 0) ? start : startOffset;
            long zeroCount = 0;
            long targetSize = Math.Max(size, _MIN_ZERO_BLOCK_SIZE);
            for (long i = candidate; i < end; i++)
            {
                if (file.Buff[i] == 0)
                {
                    if(zeroCount == 0)
                    {
                        candidate = i;
                    }
                    zeroCount++;
                    if(zeroCount >= targetSize)
                    {
                        return candidate;
                    }                
                }
                else
                {
                    zeroCount = 0;
                }
            }
            return -1;//TODO check  this case
        }
        
        public void InjectDll(string filePath, string dllPath, string functionName)
        {
            string injectedDllName = NTFSStreamUtils.GetAlternateStreamName("changed.exe", dllPath);

            var file = new PeFile(filePath);
            
            var newImportsSize = (file.ImageImportDescriptors.Length + 1) * 0x14;
            var newImportsPosition = _FindZeroBlock(file, newImportsSize);
            file.MoveImportTable(newImportsPosition);

            var newImportDesscriptorOffset = newImportsPosition + (file.ImageImportDescriptors.Length * 0x14);
            var newImportDesscriptor = new IMAGE_IMPORT_DESCRIPTOR(file.Buff, (uint)newImportDesscriptorOffset);
            var dllNameOffset = _FindZeroBlock(file, dllPath.Length + 1, (newImportsPosition + newImportsSize) + 0x14); 
            for(int i=0; i < injectedDllName.Length; i++)
            {
                file.Buff[i+dllNameOffset] = (byte)injectedDllName[i];
            }
            file.Buff[dllNameOffset + injectedDllName.Length] = 0;

            var functionNameOffset = _FindZeroBlock(file, functionName.Length + 3, dllNameOffset + dllPath.Length + 1);

            newImportDesscriptor.Name = (uint)file.ConvertOffsetToRva(dllNameOffset);
            newImportDesscriptor.TimeDateStamp = (uint)file.ConvertOffsetToRva(functionNameOffset);
            newImportDesscriptor.OriginalFirstThunk = newImportDesscriptor.ForwarderChain = 0;
            newImportDesscriptor.FirstThunk = (uint)file.ConvertOffsetToRva(newImportDesscriptorOffset) + 4;// TimeDateStamp address

            file.Buff[functionNameOffset] = 0;
            file.Buff[functionNameOffset + 1] = 0;
            for (int i=0; i < functionName.Length ; i++)
            {
                file.Buff[i + functionNameOffset+2] = (byte)functionName[i];
            }
            file.Buff[functionNameOffset + functionName.Length + 2] = 0;

            var importDirectorysSizeOffset = BitConverter.ToInt32(file.Buff, 0x3c) + 0x80 + 4;

            file.Buff[importDirectorysSizeOffset] = (byte)(newImportsSize & (0xFF));
            file.Buff[importDirectorysSizeOffset + 1] = (byte)((newImportsSize & (0xFF00)) >> 8);
            file.Buff[importDirectorysSizeOffset + 2] = (byte)((newImportsSize & (0xFF0000)) >> 16);
            file.Buff[importDirectorysSizeOffset + 3] = (byte)((newImportsSize & (0xFF000000)) >> 24);

            file.Save("changed.exe");
            NTFSStreamUtils.CopyToAlternateStream("changed.exe", dllPath);
        }
    }
}
