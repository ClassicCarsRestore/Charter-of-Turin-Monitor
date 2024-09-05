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

    [BsonElement("coordinateZ")]
    public double CoordinateZ { get; set; }

    [BsonElement("vertices")]
    public List<VerticesCoordinates> Vertices { get; set; }

    [BsonElement("color")]
    public string Color { get; set; }

    [BsonElement("activityIds")]
    public List<string> ActivityIds { get; set; }

    [BsonElement("capacity")]
    public int Capacity { get; set; }

    public VirtualMapLocation(string name, double coordinateX, double coordinateY, double coordinateZ, List<string> activityIds, List<VerticesCoordinates> vertices, string color, int capacity){
        Name = name;
        CoordinateX = coordinateX;
        CoordinateY = coordinateY;
        CoordinateZ = coordinateZ;
        ActivityIds = activityIds;
        Vertices = vertices;
        Color = color;
        Capacity = capacity;
    }
    }
}