using System.Collections.Generic;

namespace RealViewServer.Model
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double OriginLatitude { get; set; }
        public double OriginLongitude { get; set; }
        public double OriginAltitude { get; set; }
        public List<Photo> Photos { get; set; }

        public Project()
        {
            Photos = new List<Photo>();
        }
    }
}
