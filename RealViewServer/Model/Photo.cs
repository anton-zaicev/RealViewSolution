namespace RealViewServer.Model
{
    public class Photo
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public string FileName { get; set; }
        public string ImageResolution { get; set; }
        public string FileSize { get; set; }
        public string Created { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Direction { get; set; }
        public double Pitch { get; set; }
        public double DistanceToOrigin { get; set; }

        public double LocalX { get; set; }
        public double LocalY { get; set; }
        public double LocalZ { get; set; }
        
        public string AppName { get; set; }
    }
}
