using Gameboard.EventArgs;
using System;

namespace Gameboard.Utilities
{
    public interface IJsonUtility
    {
        event EventHandler<JsonErrorEventArgs> SerializationErrorEvent;
        event EventHandler<JsonErrorEventArgs> DeSerializationErrorEvent;

        JsonUtilitySerializeResponse SerializeObject(object inObject);
        string SerializeObjectAssumeSuccess(object inObject);
        JsonUtilityDeserializeResponse<T> DeserializeObject<T>(string inJson);
        void SerializationErrorOccured(string inJson, string inError);
        void DeserializationErrorOccured(string inJson, string inError);
    }
}