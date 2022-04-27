using UnityEngine;

namespace Gameboard.EventArgs
{
    public class QueuedEvent
    {
        public string targetDestinationId;
        public string eventGuid;
        public object bodyObject;
        public byte[] byteArray;
        public float startTime;
        public DataTypes.EventQueueStates eventState;
        public EventArgsToCompanionServer eventArgs;
        public virtual DataTypes.DeviceTypes targetDeviceType { get; }

        public float startedTime { get; private set; }
        public float endedTime { get; private set; }
        public bool wasCancelled { get { return eventState == DataTypes.EventQueueStates.Cancelled; } }

        // NOTE: timeSinceEnded only gets called from an Update in timeSinceEnded, therefore it's okay to use Time.time as it's on the main thread.
        public float timeSinceEnded { get { return Time.time - endedTime; } }

        /// <summary>
        /// How long this event took to complete.
        /// </summary>
        public float eventTime { get { return eventState == DataTypes.EventQueueStates.Completed ? endedTime - startedTime : Time.time - startedTime; } }

        /// <summary>
        /// How long this event took to process. If it is still processing, then will return how long it has been processing. If the event has not yet begun, it will return 0. ONLY call this on the main Unity thread!
        /// </summary>
        public float eventProcessingTime
        {
            get
            {
                if (eventState == DataTypes.EventQueueStates.Cancelled ||
                   eventState == DataTypes.EventQueueStates.TimedOut ||
                   eventState == DataTypes.EventQueueStates.Completed)
                {
                    return endedTime - startedTime;
                }
                else if (eventState == DataTypes.EventQueueStates.Processing)
                {
                    return Time.time - startedTime;
                }
                else if (eventState == DataTypes.EventQueueStates.WaitingToProcess)
                {
                    return 0f;
                }

                return 0f;
            }
        }

        // NOTE: We have to pass in the time below instead of getting Time.time. This is because these actions usually occur outside the main Unity thread, and calling Time.time outside the main Unity thread crashes the engine.

        public void BeginProcessingEvent(float inTimeValue)
        {
            eventState = DataTypes.EventQueueStates.Processing;
            startedTime = inTimeValue;
        }

        public void EventTimedOut(float inTimeValue)
        {
            eventState = DataTypes.EventQueueStates.TimedOut;
            endedTime = inTimeValue;
        }

        public void EventCancelled(float inTimeValue)
        {
            eventState = DataTypes.EventQueueStates.Cancelled;
            endedTime = inTimeValue;
        }

        public void EventCompleted(float inTimeValue)
        {
            eventState = DataTypes.EventQueueStates.Completed;
            endedTime = inTimeValue;
        }
    }
}