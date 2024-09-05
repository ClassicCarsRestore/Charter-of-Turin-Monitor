using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tasklist.Models
{
    public class VerticesCoordinates
    {

    [BsonElement("x")]
    public float X { get; set; }

    [BsonElement("z")]
    public float Z { get; set; }

    public VerticesCoordinates(float x, float z)
    {
        X = x;
        Z = z;
    }


    }
}
