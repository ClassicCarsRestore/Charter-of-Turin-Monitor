using System;

namespace tasklist.Models
{
    public class VirtualMapLocationRequestDTO
    {
        public string Name { get; set; }
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public double Rotation { get; set; }

    

    public VirtualMapLocationRequestDTO(VirtualMapLocation v){
        Name = v.Name;
        CoordinateX = v.CoordinateX;
        CoordinateY = v.CoordinateY;
        Rotation = v.Rotation;
    }
    
    }
}