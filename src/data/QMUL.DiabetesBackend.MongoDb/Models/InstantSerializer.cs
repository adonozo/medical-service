namespace QMUL.DiabetesBackend.MongoDb.Models;

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

/// <summary>
/// A custom BSON serializer for NodaTime <see cref="Instant"/>. The instant is saved as a <see cref="BsonDateTime"/>
/// object in MongoDB.
/// </summary>
public class InstantSerializer : SerializerBase<Instant>
{
    public override Instant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var date = BsonDateTimeSerializer.Instance.Deserialize(context)
                   ?? throw new ArgumentException("Unable to parse BSON Instant field");
        return Instant.FromDateTimeUtc(date.ToUniversalTime());
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
    {
        var bsonField = new BsonDateTime(value.ToDateTimeUtc());
        BsonDateTimeSerializer.Instance.Serialize(context, bsonField);
    }
}