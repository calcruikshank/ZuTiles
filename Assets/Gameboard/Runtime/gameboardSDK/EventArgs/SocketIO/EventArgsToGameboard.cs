namespace Gameboard.EventArgs
{
    public class EventArgsToGameboard
    {
        /// <summary>
        /// A unique ID for this message. If 2 messages have the same ID, then it is safe to assume the second is a duplicate of the same.
        /// </summary>
        public string id;
        
        /// <summary>
        /// Might be depreciated? Keeping to be safe....
        /// </summary>
        public string responseId;
        
        /// <summary>
        /// If this is a valid response, or if an error occured.
        /// </summary>
        public EventArgsCompanionServerResponseStatus.ResponseStatusCode responseStatus;
        
        /// <summary>
        /// The target that this message is going to (IE: the Connection ID of the Gameboard or the Connection ID of the Companion).
        /// </summary>
        public string to;
        
        /// <summary>
        /// This is a unique Connection ID for an individual companion connection. This is established whenever a player first joins with a companion, and every message from that
        /// user and their companion will match this From. If this From is different, but the UserID still comes through, then it is safe to assume a disconnect occured.
        /// </summary>
        public string from;

        /// <summary>
        /// The endpoint in the SDK that this call is targeting.
        /// </summary>
        public string endpoint;

        /// <summary>
        /// If this event includes a binary call.
        /// </summary>
        public bool binary;

        /// <summary>
        /// The JSON response data itself.
        /// </summary>
        public string body;
        
        /// <summary>
        /// The version of the endpoint that this call was intended for (to support previous endpoint versions).
        /// </summary>
        public int version;
    }
}