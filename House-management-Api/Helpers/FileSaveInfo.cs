using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace House_management_Api.Shared
{
    public class FileSaveInfo
    {
        private static string[] PermittedExtImage = new string[] { ".jpeg", ".jpg", ".png", ".gif" };
        private static string[] PermittedExtDocs = new string[] { ".jpeg", ".jpg", ".png", ".gif", ".doc", ".docx", ".pdf" };
        public static bool VerifyImage(IFormFile file)
        {
            string[] permittedExtImage = PermittedExtImage;
            long maxSize = getMaxSize();
            if (file.Length > maxSize) return false;
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!permittedExtImage.Contains(ext)) return false;

            return true;
        }

        public static bool VerifyDocument(IFormFile file)
        {
            string[] permittedExtImage = PermittedExtDocs;
            long maxSize = getMaxSize();
            if (file.Length > maxSize) return false;
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!permittedExtImage.Contains(ext)) return false;

            return true;
        }

        public static long getMaxSize()
        {
            long max = 1024 * 1024 * 3; // 3Mo
            return max;
        }

        public static string[] getSignatureInfo(string contentRootPath, string fileName)
        {
            string src = "Uploads/Signature/";
            string fileRename = generateFileName(fileName);
            return getInfo(contentRootPath, src, fileRename);
        }

        public static string[] getDocsInfo(string contentRootPath, string fileName)
        {
            string src = "Uploads/Docs/";
            string fileRename = generateFileName(fileName);
            return getInfo(contentRootPath, src, fileRename);
        }

        public static string[] getPhotoInfo(string contentRootPath, string fileName)
        {
            string src = "Uploads/Photos/";
            string fileRename = generateFileName(fileName);
            return getInfo(contentRootPath, src, fileRename);
        }

        public static string[] getInfo(string contentRootPath, string src, string fileName)
        {
            DateTime current = DateTime.Now;
            string year = current.Year.ToString();

            //Chemin relatif du dossier dans lequel le fichier sera sauvegardé 
            string folderPath = src + year;

            //Chemin absolu du dossier dans lequel le fichier sera sauvegardé
            string absoluteFolderPath = Path.Combine(contentRootPath, folderPath);

            if (!Directory.Exists(absoluteFolderPath))
            {
                Directory.CreateDirectory(absoluteFolderPath);
            }

            //Construire le chemin relatif du fichier
            string fileRename = "";
            Guid fileId = Guid.NewGuid();
            fileRename = fileId.ToString() + "-" + fileName;
            string relativeFilePath = Path.Combine(folderPath, fileRename);

            //Construire le chemin absolu du fichier
            string absoluteFilePath = Path.Combine(absoluteFolderPath, fileRename);

            return new string[] { absoluteFilePath, relativeFilePath };

            /* dans un fichier html <img src="/Uploads/Photos/2023/photo.jpg" alt="Ma superbe image" />  */
        }

        public static void deleteAllFiles(List<string> pathList)
        {
            foreach (string path in pathList)
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public static string generateFileName(string fileName) 
        { 
            var name = Guid.NewGuid().ToString().Replace("-", "");
            var lastIndex = fileName.LastIndexOf('.');
            var ext = fileName.Substring(lastIndex);
            return name+ ext;
        }

    }

}
