using System;
using System.IO;
using Trinet.Core.IO.Ntfs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllToExeInjector
{
    static class NTFSStreamUtils
    {
        public static string GetAlternateStreamName(string destinationPath, string sourcePath)
        {
            var destName = Path.GetFileName(destinationPath);
            var sourceName = Path.GetFileName(sourcePath);
            return destName + ":" + sourceName;
        }

        public static string CopyToAlternateStream(string destinationPath, string sourcePath)
        {

            var destination = new FileInfo(destinationPath);
            var sourceName = Path.GetFileName(sourcePath);
            if (destination.AlternateDataStreamExists(sourceName))
            {
                destination.DeleteAlternateDataStream(sourceName);
            }
            var destDataStream = destination.GetAlternateDataStream(sourceName);

            
            using (var destFileStream = destDataStream.OpenWrite())
            using (var sourceFileStream = File.OpenRead(sourcePath))
            {
                sourceFileStream.CopyTo(destFileStream);
            }
            foreach(var st in destination.ListAlternateDataStreams())
            {
                Console.WriteLine(st.Name);
            }
            return destination.Name + ":" + sourceName;
        }
    }
}
