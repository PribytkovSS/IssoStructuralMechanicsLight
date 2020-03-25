using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.PlatformConfiguration;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using IssoStMechLight.Models;

namespace IssoStMechLight.UWP
{   
    public class IssoFilesUWP: IssoFileManager
    {
        public string lastError = "";
        public string fileName = "", fileLocation = "", fullFileName = "";
        StorageFile file;

        public async Task<StorageFile> GetFileAsync(bool open)
        {
            lastError = "";
            StorageFile filePicked;
            try
            {
                if (!open)
                {
                    FileSavePicker pic = new FileSavePicker();
                    pic.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    pic.FileTypeChoices.Clear();
                    List<string> choises = new List<string>() { ".isso" };
                    pic.FileTypeChoices.Add("Файлы модели Isso", choises);
                    pic.SuggestedFileName = "Модель";
                    pic.DefaultFileExtension = ".isso";
                    filePicked = await pic.PickSaveFileAsync();
                } else
                {
                    FileOpenPicker pic = new FileOpenPicker();
                    pic.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    pic.FileTypeFilter.Clear();
                    pic.FileTypeFilter.Add(".isso");
                    filePicked = await pic.PickSingleFileAsync();
                }
                return filePicked;                
            } catch (Exception e)
            {
                lastError = e.Message;
                return null;
            }
        }

        public async Task<bool> WriteStreamToFile(Stream stream, string FileName)
        {
            if (FileName == "")
            {
                file = await GetFileAsync(false); 
            };
            if (file != null)
            {
                fileName = file.Name;
                fileLocation = Path.GetDirectoryName(file.Path);
                fullFileName = Path.Combine(fileLocation, fileName);

                Stream outStream = await file.OpenStreamForWriteAsync();
                outStream.SetLength(0);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(outStream);
                stream.Close();
                await outStream.FlushAsync();
                outStream.Close();
                return true;
            }
            else return false;
        }

        public async Task<bool> ReadFileToStream(Stream stream)
        {
            StorageFile fileToOpen = await GetFileAsync(true);
            if (fileToOpen != null)
            {
                fileName = fileToOpen.Name;
                fileLocation = Path.GetDirectoryName(fileToOpen.Path);
                fullFileName = Path.Combine(fileLocation, fileName);

                return await ReadFileToStream(stream, fileToOpen);
            }
            else return false;
        }

        public async Task<bool> ReadFileToStream(Stream stream, string Subfolder, string FileName)
        {
            StorageFolder localFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            try
            {
                if (Subfolder != "")
                {
                    StorageFolder subFolder = await localFolder.GetFolderAsync(Subfolder);
                    return await ReadFileToStream(stream, await subFolder.GetFileAsync(FileName)); 
                }
                else return await ReadFileToStream(stream, await localFolder.GetFileAsync(FileName));                 
            }
            catch (Exception E)
            {
                lastError = E.Message;
                return false;
            }
        }

        public async Task<bool> ReadFileToStream(Stream stream, StorageFile File)
        {
            Stream fileStream = await File.OpenStreamForReadAsync();
            stream.SetLength(0);

            fileStream.CopyTo(stream);
            fileStream.Close();

            file = File;
            return true;
        }

        string IssoFileManager.LastError()
        {
            return lastError;
        }

        string IssoFileManager.FileName()
        {
            return fileName;
        }

        string IssoFileManager.FileLocation()
        {
            return fileLocation;
        }

        string IssoFileManager.FullFileName()
        {
            return fullFileName;
        }
    }
}
