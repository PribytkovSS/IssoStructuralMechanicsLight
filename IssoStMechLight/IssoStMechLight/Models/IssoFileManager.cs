using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IssoStMechLight.Models
{
    public interface IssoFileManager
    {
        string LastError();
        string FileName(); 
        string FileLocation();
        string FullFileName();

        Task<bool> WriteStreamToFile(Stream stream, string FileName);
        Task<bool> ReadFileToStream(Stream stream);
        Task<bool> ReadFileToStream(Stream stream, string Subfolder, string FileName);
    }
}
