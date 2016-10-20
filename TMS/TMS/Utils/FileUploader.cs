﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Teek.Models
{

    public class FileUploader
    {

        public string RootPath { get; private set; }
        public string RelativeRootPath { get; private set; }

        public FileUploader()
        {
            var rootPath = ConfigurationManager.AppSettings["TMSRootPath"];
            this.RelativeRootPath = rootPath.StartsWith("~") ? rootPath.Substring(1) : rootPath;
            this.RootPath = HostingEnvironment.MapPath(rootPath);

            if (!Directory.Exists(this.RootPath))
            {
                Directory.CreateDirectory(this.RootPath);
            }
        }

        public string UploadFile(HttpPostedFileBase file, string containingFolder)
        {
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = Path.GetFileName(file.FileName);
                //_{Guid.NewGuid().ToString()}{fileExtension}";
            var guidFolder = Guid.NewGuid().ToString();
            var relativeFilePath = Path.Combine(this.RelativeRootPath, containingFolder, guidFolder, fileName);
            var fullFilePath = HostingEnvironment.MapPath(relativeFilePath);
            var fullFolderPath = Path.GetDirectoryName(fullFilePath);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            file.SaveAs(fullFilePath);
            return relativeFilePath.Replace("\\", "/");
        }

        public string UploadImage(HttpPostedFileBase file, string containingFolder)
        {
            return this.UploadImage(file, containingFolder, null, null);
        }

        public string UploadImage(HttpPostedFileBase file, string containingFolder, int? maxWidth, int? maxHeight)
        {
            try
            {
                // Try to parse the image
                var bitmap = Image.FromStream(file.InputStream) as Bitmap;

                if ((maxWidth != null && bitmap.Width > maxWidth) || (maxHeight != null && bitmap.Height > maxHeight))
                {
                    throw new ImageUploadException("The file is reached the maximum size!");
                }

                return this.UploadFile(file, containingFolder);
            }
            catch (ArgumentException)
            {
                throw new ImageUploadException("Cannot read this image.");
            }
        }

    }

    public class ImageUploadException : Exception
    {

        public ImageUploadException() : base() { }

        public ImageUploadException(string message) : base(message) { }

        public ImageUploadException(string message, Exception innerException) : base(message, innerException) { }

    }

}