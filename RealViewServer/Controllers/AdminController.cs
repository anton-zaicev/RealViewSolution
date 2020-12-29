using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.AspNetCore.Mvc;
using RealViewServer.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RealViewServer.Storage;

namespace RealViewServer.Controllers
{
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly DatabaseContext _dbContext;

        public AdminController(IWebHostEnvironment env, DatabaseContext dbContext)
        {
            _env = env;
            _dbContext = dbContext;
        }

        [HttpGet]
        public ViewResult Projects()
        {
            var list = _dbContext.Projects.Include(p => p.Photos).ToList();
            return View(list);
        }

        [HttpPost]
        public RedirectResult Projects(string name, string latitude, string longitude, string altitude)
        {
            var project = new Project
            {
                Name = name, 
                OriginLatitude = double.Parse(latitude.Replace('.',',')), 
                OriginLongitude = double.Parse(longitude.Replace('.', ',')),
                OriginAltitude = double.Parse(altitude.Replace('.', ','))
            };
            _dbContext.Projects.Add(project);
            _dbContext.SaveChanges();
            return Redirect("/admin/projects");
        }

        [HttpGet]
        public ViewResult Photos(string projectName)
        {
            var project = _dbContext.Projects.Include(p => p.Photos).First(p => p.Name.Equals(projectName));
            return View(project);
        }

        [HttpPost]
        public RedirectResult Photos(string projectName, List<IFormFile> files)
        {
            var webRoot = _env.WebRootPath;

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(webRoot, "img", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        formFile.CopyTo(stream);

                    ProcessPhoto(filePath, formFile.FileName, fileName, projectName);
                }
            }

            return Redirect($"/admin/projects/{projectName}/photos");
        }

        private void ProcessPhoto(string filePath, string fileName, string localFileName, string projectName)
        {
            var project = _dbContext.Projects.First(p => p.Name.Equals(projectName));
            var directories = ImageMetadataReader.ReadMetadata(filePath);

            string resolution = null;
            string fileSize = null;
            string created = null;
            double? latitude = null;
            double? longitude = null;
            double? altitude = null;
            double? direction = null;
            double? pitch = null;
            double? localX = null;
            double? localY = null;
            double? localZ = null;
            string app = null;

            foreach (var directory in directories)
            {
                if (directory.Name.Equals("JPEG"))
                {
                    var imageWidth = directory.Tags.FirstOrDefault(t => t.Name.Equals("Image Width"))?.Description;
                    var imageHeight = directory.Tags.FirstOrDefault(t => t.Name.Equals("Image Height"))?.Description;
                    resolution = $"{imageWidth.Split(' ')[0]} x {imageHeight.Split(' ')[0]}";
                }

                if (directory.Name.Equals("Exif SubIFD"))
                {
                    var userComment = directory.Tags.FirstOrDefault(t => t.Name.Equals("User Comment"))?.Description;
                    var json = JsonConvert.DeserializeObject<ExifUserCommentJson>(userComment);

                    pitch = json.Pitch;
                    if (pitch < -90)
                        pitch = -90;
                    else if (pitch > 90)
                        pitch = 90;

                    app = json.AppName;
                    altitude = json.Altitude;
                    direction = json.Direction;
                }

                if (directory.Name.Equals("GPS"))
                {
                    var gps = (GpsDirectory) directory;
                    var location = gps.GetGeoLocation();

                    if (location != null)
                    {
                        latitude = location.Latitude;
                        longitude = location.Longitude;
                    }

                    // var rawAltitude = directory.Tags.FirstOrDefault(t => t.Name.Equals("GPS Altitude"))?.Description;
                    // var rawDirection = directory.Tags.FirstOrDefault(t => t.Name.Equals("GPS Img Direction"))?.Description;
                    // altitude = double.Parse(rawAltitude.Split(' ')[0] ?? throw new InvalidOperationException());
                    // direction = double.Parse(rawDirection.Split(' ')[0] ?? throw new InvalidOperationException());
                }

                if (directory.Name.Equals("File"))
                {
                    fileSize = directory.Tags.FirstOrDefault(t => t.Name.Equals("File Size"))?.Description.Split(' ')[0];
                    created = directory.Tags.FirstOrDefault(t => t.Name.Equals("File Modified Date"))?.Description;
                }
            }

            var lat0 = project.OriginLatitude;
            var lat1 = latitude;
            var lon0 = project.OriginLongitude;
            var lon1 = longitude;

            var lat = (lat0 + lat1) / 2 * 0.01745;
            var dx = 111.3 * Math.Cos(lat.Value) * (lon1 - lon0);
            var dy = 111.3 * (lat1 - lat0);
            var distanceToOrigin = Math.Sqrt(dx.Value * dx.Value + dy.Value * dy.Value);
            distanceToOrigin *= 1000;

            localX = dx * 1000;
            localY = dy * 1000;
            localZ = altitude - project.OriginAltitude;

            var photo = new Photo
            {
                Uri = "/img/" + localFileName,
                FileName = fileName,
                ImageResolution = resolution,
                FileSize = fileSize,
                Created = created,
                Latitude = latitude.Value,
                Longitude = longitude.Value,
                Altitude = altitude.Value,
                Direction = direction.Value,
                Pitch = pitch.Value,
                DistanceToOrigin = distanceToOrigin,
                LocalX = localX.Value,
                LocalY = localY.Value,
                LocalZ = localZ.Value,
                AppName = app,
            };
            project.Photos.Add(photo);
            _dbContext.SaveChanges();
        }
    }

    internal class ExifUserCommentJson
    {
        public double Altitude { get; set; }
        public double Direction { get; set; }
        public double Pitch { get; set; }
        public string AppName { get; set; }
    }
}
