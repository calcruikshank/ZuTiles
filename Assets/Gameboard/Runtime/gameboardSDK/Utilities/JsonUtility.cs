using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Gameboard.EventArgs;

namespace Gameboard.Utilities
{
    public class JsonUtility : GameboardUtility, IJsonUtility
    {
        JsonSerializerSettings serializerSettings;
        JsonSerializerSettings deSerializerSettings;

        public event EventHandler<JsonErrorEventArgs> SerializationErrorEvent;
        public event EventHandler<JsonErrorEventArgs> DeSerializationErrorEvent;

        public JsonUtility()
        {
            serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,

                Error = delegate (object sender, ErrorEventArgs args)
                {
                    args.ErrorContext.Handled = true;
                    SerializationErrorOccured(args.ErrorContext.Error.Source, args.ErrorContext.Error.Message);
                },
            };

            deSerializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,

                Error = delegate (object sender, ErrorEventArgs args)
                {
                    args.ErrorContext.Handled = true;
                    DeserializationErrorOccured(args.ErrorContext.Error.Source, args.ErrorContext.Error.Message);
                },
            };
        }

        public JsonUtilityDeserializeResponse<T> DeserializeObject<T>(string inJson)
        {
            try
            {
                JsonErrorEventArgs errorEventArgs = null;
                DeSerializationErrorEvent = (originObj, eventArgs) => 
                {
                    errorEventArgs = eventArgs;
                };

                T deserializedObject = JsonConvert.DeserializeObject<T>(inJson, deSerializerSettings);

                if (errorEventArgs != null)
                {
                    GameboardLogging.LogMessage($"JsonUtility::DeserializeObject failure for JSON {inJson} - {errorEventArgs.errorMessage}", GameboardLogging.MessageTypes.Error);
                    return new JsonUtilityDeserializeResponse<T>()
                    {
                        success = false,
                        deserializedArgs = default(T),
                    };
                }
                else
                {
                    return new JsonUtilityDeserializeResponse<T>()
                    {
                        success = true,
                        deserializedArgs = deserializedObject,
                    };
                }
            }
            catch(Exception e)
            {
                GameboardLogging.LogMessage("Deserialization Failure: " + e.Message, GameboardLogging.MessageTypes.Error);

                return new JsonUtilityDeserializeResponse<T>()
                {
                    success = false,
                    deserializedArgs = default(T),
                };
            }
        }

        /// <summary>
        /// Unsafe serialization. Only use this if you're entirely certain the object exists.
        /// </summary>
        /// <param name="inObject"></param>
        /// <returns></returns>
        public string SerializeObjectAssumeSuccess(object inObject)
        {
            JsonUtilitySerializeResponse result = SerializeObject(inObject);
            return result.serialized;
        }

        public JsonUtilitySerializeResponse SerializeObject(object inObject)
        {
            try
            {
                return new JsonUtilitySerializeResponse()
                { 
                    success = true,
                    serialized = JsonConvert.SerializeObject(inObject, serializerSettings),
                };
            }
            catch(Exception e)
            {
                // TODO: Need to send this message into our error handler!
                Debug.LogError("Serialization Failure: " + e.Message);

                return new JsonUtilitySerializeResponse()
                {
                    success = false,
                    serialized = "",
                };
            }
        }

        public void SerializationErrorOccured(string inJson, string inError)
        {
            if(SerializationErrorEvent != null)
            {
                JsonErrorEventArgs eventArgs = new JsonErrorEventArgs()
                {
                    sourceJson = inJson,
                    errorMessage = inError
                };

                SerializationErrorEvent(this, eventArgs);
            }
        }

        public void DeserializationErrorOccured(string inJson, string inError)
        {
            if (DeSerializationErrorEvent != null)
            {
                JsonErrorEventArgs eventArgs = new JsonErrorEventArgs()
                {
                    sourceJson = inJson,
                    errorMessage = inError
                };

                DeSerializationErrorEvent(this, eventArgs);
            }
        }
    }
}