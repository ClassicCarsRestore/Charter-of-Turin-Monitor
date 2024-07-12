using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tasklist.Models
{
    public class VirtualMapLocation
    {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    [BsonElement("coordinateX")]
    public double CoordinateX { get; set; }

    [BsonElement("coordinateY")]
    public double CoordinateY { get; set; }

    [BsonElement("rotation")]
    public double Rotation { get; set; }

    [BsonElement("activityIds")]
    public List<string> ActivityIds { get; set; }

    public VirtualMapLocation(string name, double coordinateX, double coordinateY, double rotation, List<string> activityIds){
        Name = name;
        CoordinateX = coordinateX;
        CoordinateY = coordinateY;
        Rotation = rotation;
        ActivityIds = activityIds;
    }
    }
}