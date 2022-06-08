using Gameboard.EventArgs;
using Gameboard.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameboard.Utilities
{
    public class CompanionHandlerUtility : GameboardUtility, ICompanionHandlerUtility
    {
        public ICompanionCommunicationsUtility communicationsUtility { get; private set; }
        public IGameboardConfig gameboardConfig { get; private set; }
        public IJsonUtility jsonUtility { get; private set; }

        private float cachedTime;

        public CompanionHandlerUtility()
        {
            companionEventQueueDict = new Dictionary<string, Queue<CompanionQueuedEvent>>();
            companionProcessingDict = new Dictionary<string, CompanionQueuedEvent>();

            gameboardEventQueueDict = new Dictionary<string, Queue<GameboardQueuedEvent>>();
            gameboardProcessingDict = new Dictionary<string, GameboardQueuedEvent>();

            eventResponseList = new List<SendMessageToCompanionServiceResponse>();

            CurrentEvents = new List<QueuedEvent>();
            EndedEvents = new List<QueuedEvent>();
        }

        ~CompanionHandlerUtility()
        {
            communicationsUtility.CompanionMessageReceived -= IngestMessageReceivedFromCompanion;
            communicationsUtility.CompanionAckReceived -= CompanionServerAckReceived;
        }

        public void InjectDependencies(ICompanionCommunicationsUtility inCommunications, IJsonUtility inJsonUtility, IGameboardConfig inGameboardConfig)
        {
            communicationsUtility = inCommunications;
            jsonUtility = inJsonUtility;
            gameboardConfig = inGameboardConfig;

            communicationsUtility.CompanionMessageReceived += IngestMessageReceivedFromCompanion;
            communicationsUtility.CompanionAckReceived += CompanionServerAckReceived;
        }

        public override void ProcessUpdate()
        {
            // ProcessUpdate always occurs on the main thread, so it's safe to fetch Time.time here.
            cachedTime = Time.time;

            UpdateEventQueueProcessing();
            UpdatedEndedEventsList();
        }

        #region Received Message Events
        public IncomingEventDelegate<GameboardButtonPressedEventArgs> ButtonPressedEvent { get; set; }
        public IncomingEventDelegate<GameboardDropdownChangedEventArgs> DropdownSelectionChangedEvent { get; }
        public IncomingEventDelegate<GameboardMatZoneDropOccuredEventArgs> MatZoneDropOccuredEvent { get; }
        public IncomingEventDelegate<GameboardTickBoxStateChangedEventArgs> TickboxStateChangedEvent { get; }
        public IncomingEventDelegate<GameboardDiceRolledEventArgs> DiceRolledEvent { get; set; }
        public IncomingEventDelegate<GameboardCardPlayedEventArgs> CardPlayedEvent { get; set; }
        public IncomingEventDelegate<GameboardUserPresenceEventArgs> UserPresenceChangedEvent { get; set; }
        public IncomingEventDelegate<GameboardCompanionButtonPressedEventArgs> CompanionButtonPressedEvent { get; set; }
        public IncomingEventDelegate<GameboardCompanionCardsButtonPressedEventArgs> CompanionCardsButtonPressedEvent { get; set; }

        public Dictionary<string, Queue<CompanionQueuedEvent>> companionEventQueueDict { get; set; }
        public Dictionary<string, CompanionQueuedEvent> companionProcessingDict { get; set; }
        public Dictionary<string, Queue<GameboardQueuedEvent>> gameboardEventQueueDict { get; set; }
        public Dictionary<string, GameboardQueuedEvent> gameboardProcessingDict { get; set; }
        public List<SendMessageToCompanionServiceResponse> eventResponseList { get; set; }
        public List<QueuedEvent> CurrentEvents { get; set; }
        public List<QueuedEvent> EndedEvents { get; set; }
        #endregion

        #region Received Message Ingestion
        public void IngestMessageReceivedFromCompanion(object sender, string jsonPayload)
        {
            JsonUtilityDeserializeResponse<EventArgsToGameboard> deserializedPayload = jsonUtility.DeserializeObject<EventArgsToGameboard>(jsonPayload);
            if (!deserializedPayload.success)
            {
                GameboardLogging.LogMessage("--- Failed to deserialize EventArgsToGameboard payload. ", GameboardLogging.MessageTypes.Error);
                return;
            }
            
            //NOTE: Currently hardcoding this to 1. Later, we'll be sending this from the server.
            deserializedPayload.deserializedArgs.version = 1;

            int targetSlashIndex = deserializedPayload.deserializedArgs.endpoint.IndexOf('/');
            string toStripEndpoint = deserializedPayload.deserializedArgs.endpoint.Remove(targetSlashIndex + 1);
            string actualEndpoint = targetSlashIndex == -1 ? deserializedPayload.deserializedArgs.endpoint : deserializedPayload.deserializedArgs.endpoint.Replace(toStripEndpoint, "");

            Debug.Log("--- Digesting endpoint " + actualEndpoint + " from " + deserializedPayload.deserializedArgs.from + " with body " + deserializedPayload.deserializedArgs.body);

            switch (actualEndpoint)
            {
                case "buttonPressed":
                    ManageIncomingEvent<GameboardButtonPressedEventArgs>(ButtonPressedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "dropdownSelectionChanged":
                    ManageIncomingEvent<GameboardDropdownChangedEventArgs>(DropdownSelectionChangedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "matZoneDropOccured":
                    ManageIncomingEvent<GameboardMatZoneDropOccuredEventArgs>(MatZoneDropOccuredEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "tickBoxStateChanged":
                    ManageIncomingEvent<GameboardTickBoxStateChangedEventArgs>(TickboxStateChangedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "diceRolled":
                    ManageIncomingEvent<GameboardDiceRolledEventArgs>(DiceRolledEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "cardPlayed":
                    ManageIncomingEvent<GameboardCardPlayedEventArgs>(CardPlayedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "userPresenceChange":
                    ManageIncomingEvent<GameboardUserPresenceEventArgs>(UserPresenceChangedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "companionButtonPressed":
                    ManageIncomingEvent<GameboardCompanionButtonPressedEventArgs>(CompanionButtonPressedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                case "cardsButtonPressed":
                    ManageIncomingEvent<GameboardCompanionCardsButtonPressedEventArgs>(CompanionCardsButtonPressedEvent, deserializedPayload.deserializedArgs, actualEndpoint);
                    break;

                default:
                    GameboardLogging.LogMessage($"Unmanaged Endpoint Ingestion! Attempted Endpoint was {deserializedPayload.deserializedArgs.endpoint}", GameboardLogging.MessageTypes.Error);
                    break;
            }
        }
        #endregion

        #region Helper Methods
        #region Event Arg Construction
        /// <summary>
        /// Send a message to a specific User.
        /// </summary>
        /// <param name="inVersionTag"></param>
        /// <param name="targetUserId"></param>
        /// <param name="inEndpoint"></param>
        /// <returns></returns>
        private EventArgsToCompanionServer BuildEventArgsToTargetUser(int inVersionTag, string targetUserId, string inEndpoint)
        {
            return new EventArgsToCompanionServer()
            {
                endpoint = $"{"companion"}/{inEndpoint}",
                to = $"{targetUserId}.u@{gameboardConfig.gameboardId}.b",
            };
        }

        /// <summary>
        /// Send a message to another Gameboard.
        /// </summary>
        /// <param name="inVersionTag"></param>
        /// <param name="targetGameboardId"></param>
        /// <param name="inEndpoint"></param>
        /// <returns></returns>
        private EventArgsToCompanionServer BuildEventArgsToTargetGameboard(int inVersionTag, string targetGameboardId, string inEndpoint)
        {
            return new EventArgsToCompanionServer()
            {
                endpoint = $"{"board"}/{inEndpoint}",
                to = $"{targetGameboardId}.b",
            };
        }

        /// <summary>
        /// Send a message that results in responding directly to this gameboard.
        /// </summary>
        /// <param name="inVersionTag"></param>
        /// <param name="inTargetName"></param>
        /// <param name="inEndpoint"></param>
        /// <returns></returns>
        private EventArgsToCompanionServer BuildEventArgsToTargetSelfGameboard(int inVersionTag, string inEndpoint)
        {
            return new EventArgsToCompanionServer()
            {
                endpoint = $"{"board"}/{inEndpoint}",
                to = $"{gameboardConfig.gameboardId}.b",
            };
        }
        #endregion

        #region Companion Event Queuing
        private string AddEventToCompanionQueue(string inUserId, EventArgsToCompanionServer inEventArgs, object inBodyObject, byte[] inByteArray = null)
        {
            string newEventId = Guid.NewGuid().ToString();

            lock (companionEventQueueDict)
            {
                if (!companionEventQueueDict.ContainsKey(inUserId))
                {
                    companionEventQueueDict.Add(inUserId, new Queue<CompanionQueuedEvent>());
                }

                CompanionQueuedEvent newQueuedEvent = new CompanionQueuedEvent()
                {
                    eventGuid = newEventId,
                    targetUserId = inUserId,
                    bodyObject = inBodyObject,
                    eventArgs = inEventArgs,
                    byteArray = inByteArray,
                    eventState = DataTypes.EventQueueStates.WaitingToProcess,
                };

                companionEventQueueDict[inUserId].Enqueue(newQueuedEvent);
                CurrentEvents.Add(newQueuedEvent);
            }

            return newEventId;
        }

        private string AddEventToGameboardQueue(string inGameboardId, EventArgsToCompanionServer inEventArgs, object inBodyObject, byte[] inByteArray = null)
        {
            string newEventId = Guid.NewGuid().ToString();

            lock (gameboardEventQueueDict)
            {
                if (!gameboardEventQueueDict.ContainsKey(inGameboardId))
                {
                    gameboardEventQueueDict.Add(inGameboardId, new Queue<GameboardQueuedEvent>());
                }

                GameboardQueuedEvent newQueuedEvent = new GameboardQueuedEvent()
                {
                    eventGuid = newEventId,
                    targetGameboardId = inGameboardId,
                    bodyObject = inBodyObject,
                    eventArgs = inEventArgs,
                    byteArray = inByteArray,
                    eventState = DataTypes.EventQueueStates.WaitingToProcess,
                };

                gameboardEventQueueDict[inGameboardId].Enqueue(newQueuedEvent);
                CurrentEvents.Add(newQueuedEvent);
            }

            return newEventId;
        }

        private async Task<SendMessageToCompanionServiceResponse> AwaitQueuedCompanionEvent(string inUserId, string inEventId)
        {            
            SendMessageToCompanionServiceResponse responseObject = await AwaitEventAndReceiveResponseData(inEventId);

            lock(companionProcessingDict)
            {
                if(companionProcessingDict[inUserId] == null)
                {
                    GameboardLogging.LogMessage($"=== Companion processing queue completion mismatch! UserID {inUserId} completed event with ID {inEventId} however the registered processing event was null!", GameboardLogging.MessageTypes.Error);
                }
                else if(companionProcessingDict[inUserId].eventGuid != inEventId)
                {
                    GameboardLogging.LogMessage($"=== Companion processing queue completion mismatch! UserID {inUserId} completed event with ID {inEventId} however was waiting for the event ID {companionProcessingDict[inUserId].eventGuid}", GameboardLogging.MessageTypes.Error);
                }

                companionProcessingDict[inUserId] = null;
            }

            return responseObject;
        }

        private async Task<SendMessageToCompanionServiceResponse> AwaitQueuedGameboardEvent(string inGameboardId, string inEventId)
        {
            SendMessageToCompanionServiceResponse responseObject = await AwaitEventAndReceiveResponseData(inEventId);

            lock (gameboardProcessingDict)
            {
                if (gameboardProcessingDict[inGameboardId] == null)
                {
                    GameboardLogging.LogMessage($"=== Gameboard processing queue completion mismatch! GameboardID {inGameboardId} completed event with ID {inEventId} however the registered processing event was null!", GameboardLogging.MessageTypes.Error);
                }
                else if (gameboardProcessingDict[inGameboardId].eventGuid != inEventId)
                {
                    GameboardLogging.LogMessage($"=== Companion processing queue completion mismatch! GameboardID {inGameboardId} completed event with ID {inEventId} however was waiting for the event ID {gameboardProcessingDict[inGameboardId].eventGuid}", GameboardLogging.MessageTypes.Error);
                }

                gameboardProcessingDict[inGameboardId] = null;
            }

            return responseObject;
        }

        private async Task<SendMessageToCompanionServiceResponse> AwaitEventAndReceiveResponseData(string inEventId)
        {
            await WaitForEventCompletion(inEventId);

            QueuedEvent queuedEvent = EndedEvents.Find(s => s.eventGuid == inEventId);
            if(queuedEvent == null)
            {
                GameboardLogging.LogMessage($"--- Queued event {inEventId} registered as completed however does not exist in EndedEvents!", GameboardLogging.MessageTypes.Error);

                SendMessageToCompanionServiceResponse cancelledResponseObject = new SendMessageToCompanionServiceResponse()
                {
                    success = false,
                    errorMessage = "Processing failure occured in AwaitEventAndReceiveResponseData",
                    responseForEventId = inEventId,
                };
                return cancelledResponseObject;
            }

            // This event has ended. Determine reason for ending and respond appropriately.
            switch (queuedEvent.eventState)
            {
                case DataTypes.EventQueueStates.WaitingToProcess:
                case DataTypes.EventQueueStates.Processing:
                    GameboardLogging.LogMessage("--- AwaitEvent should never reach this state! Let Red know.", GameboardLogging.MessageTypes.Error);
                    SendMessageToCompanionServiceResponse impossibleResponseObject = new SendMessageToCompanionServiceResponse()
                    {
                        success = false,
                        errorMessage = "Impossible state for AwaitEventAndReceiveResponseData was reached. Report this to Gameboard.",
                        responseForEventId = inEventId,
                    };
                return impossibleResponseObject;

                case DataTypes.EventQueueStates.Cancelled:
                case DataTypes.EventQueueStates.TimedOut:
                    // Event failed due to being Cancelled or Timing Out.
                    SendMessageToCompanionServiceResponse cancelledResponseObject = new SendMessageToCompanionServiceResponse()
                    {
                        success = false,
                        errorMessage = queuedEvent.eventState == DataTypes.EventQueueStates.Cancelled ? "Event was cancelled." : "Event has timed out.",
                        responseForEventId = inEventId,
                    };
                return cancelledResponseObject;


                case DataTypes.EventQueueStates.Completed:
                    // That's the stuff - it completed successfully!
                    SendMessageToCompanionServiceResponse responseObject = eventResponseList.Find(s => s.responseForEventId == inEventId);
                    if (responseObject == null)
                    {
                        GameboardLogging.LogMessage("--- AwaitEventAndReceiveResponseData determined event " + inEventId + " ended however no response object was found!", GameboardLogging.MessageTypes.Warning);
                    }
                    else
                    {
                        lock (eventResponseList)
                        {
                            eventResponseList.Remove(responseObject);
                        }
                    }

                return responseObject;
            }

            // Should never get here, but in case we do, catch it.
            SendMessageToCompanionServiceResponse impossibleFailureResponse = new SendMessageToCompanionServiceResponse()
            {
                success = false,
                errorMessage = "Reached end of AwaitEventAndReceiveResponseData with no valid resolution. Please report this to Gameboard!",
                responseForEventId = inEventId,
            };
            return impossibleFailureResponse;
        }

        private async Task WaitForEventCompletion(string inEventId)
        {
            while (EndedEvents.Find(s => s.eventGuid == inEventId) == null)
            {
                await Task.Delay(100);
            }

            return;
        }

        private void UpdateEventQueueProcessing()
        {
            // RED NOTE: Not a fan of the following being duplicated. Can clean this up later.

            if (companionProcessingDict != null && companionEventQueueDict != null)
            {
                lock (companionProcessingDict)
                {
                    lock (companionEventQueueDict)
                    {
                        foreach (KeyValuePair<string, Queue<CompanionQueuedEvent>> thisPair in companionEventQueueDict)
                        {
                            if (!companionProcessingDict.ContainsKey(thisPair.Key))
                            {
                                companionProcessingDict.Add(thisPair.Key, null);
                            }

                            if (companionProcessingDict[thisPair.Key] == null && thisPair.Value.Count > 0)
                            {
                                CompanionQueuedEvent queuedEvent = thisPair.Value.Dequeue();
                                queuedEvent.BeginProcessingEvent(cachedTime);

                                companionProcessingDict[thisPair.Key] = queuedEvent;
                                PerformMessageTransfer(queuedEvent);
                            }
                        }
                    }
                }
            }
            
            if (gameboardProcessingDict != null && gameboardEventQueueDict != null)
            {
                lock (gameboardProcessingDict)
                {
                    lock (gameboardEventQueueDict)
                    {
                        foreach (KeyValuePair<string, Queue<GameboardQueuedEvent>> thisPair in gameboardEventQueueDict)
                        {
                            if (!gameboardProcessingDict.ContainsKey(thisPair.Key))
                            {
                                gameboardProcessingDict.Add(thisPair.Key, null);
                            }

                            if (gameboardProcessingDict[thisPair.Key] == null && thisPair.Value.Count > 0)
                            {
                                GameboardQueuedEvent queuedEvent = thisPair.Value.Dequeue();
                                queuedEvent.BeginProcessingEvent(cachedTime);

                                gameboardProcessingDict[thisPair.Key] = queuedEvent;
                                PerformMessageTransfer(queuedEvent);
                            }
                        }
                    }
                }
            }
        }

        private void UpdatedEndedEventsList()
        {
            if(EndedEvents == null)
            {
                return;
            }

            lock (EndedEvents)
            {
                for (int i = EndedEvents.Count - 1; i >= 0; i--)
                {
                    // Events that have ended are kept for 5 seconds to give everything a chance to see it's ended, then we cull them.
                    if (EndedEvents[i].timeSinceEnded >= 5f)
                    {
                        EndedEvents.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Cancels all running events for a Companion and clears any queued events.
        /// </summary>
        /// <param name="inUserId"></param>
        public void ClearQueueForPlayer(string inUserId)
        {
            // Convert any events in the queue for this player into cancelled events. They'll be removed entirely 30 seconds after being cancelled, allowing everything to have a moment to remove them.
            lock (companionEventQueueDict)
            {
                if (companionEventQueueDict.ContainsKey(inUserId))
                {
                    while (companionEventQueueDict.Count > 0)
                    {
                        CompanionQueuedEvent queuedEvent = companionEventQueueDict[inUserId].Dequeue();
                        EventEnded(DataTypes.EventQueueStates.Cancelled, queuedEvent);
                    }
                }
            }
        }

        private void PerformMessageTransfer(QueuedEvent inQueuedEvent)
        {
            JsonUtilitySerializeResponse serializedBody = jsonUtility.SerializeObject(inQueuedEvent.bodyObject);
            if (!serializedBody.success)
            {
                SendMessageToCompanionServiceResponse errorResponse = new SendMessageToCompanionServiceResponse()
                {
                    success = false,
                    eventArgs = null,
                    responseForEventId = inQueuedEvent.eventGuid,
                    errorMessage = "Failed to serialize body object.",
                };

                lock (eventResponseList)
                {
                    eventResponseList.Add(errorResponse);
                }

                return;
            }

            inQueuedEvent.eventArgs.body = serializedBody.serialized;

            SendMessageToCompanionService(inQueuedEvent.eventArgs, inQueuedEvent.eventGuid, inQueuedEvent.byteArray);
        }
        #endregion

        #region Data Sending
        /// <summary>
        /// Used to send messages to the companion service when the bodyObject is a raw string, and not something that should be serialized.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventArgs"></param>
        /// <param name="bodyObject"></param>
        /// <returns></returns>
        private void SendRawBodyMessageToCompanionService(EventArgsToCompanionServer eventArgs, string rawBodyObject, string inEventId, byte[] byteArray)
        {
            eventArgs.body = rawBodyObject;
            SendMessageToCompanionService(eventArgs, inEventId, byteArray);
        }

        private void SendMessageToCompanionService(EventArgsToCompanionServer eventArgs, string inEventId, byte[] byteArray)
        {
            JsonUtilitySerializeResponse serializedResult = jsonUtility.SerializeObject(eventArgs);
            if (serializedResult.success)
            {
                SendMessageToCompanionWithRetry(inEventId, gameboardConfig.eventRetryCount, serializedResult.serialized, byteArray);
            }    
            else
            {
                SendMessageToCompanionServiceResponse errorResponse = new SendMessageToCompanionServiceResponse()
                {
                    success = false,
                    eventArgs = null,
                    responseForEventId = inEventId,
                    errorMessage = "SendMessageToCompanionService failed to serialize eventArgs.",
                };

                lock (eventResponseList)
                {
                    eventResponseList.Add(errorResponse);
                }
            }
        }


        private async void SendMessageToCompanionWithRetry(string inEventId, int retriesLeft, string inJsonPayload, byte[] inByteArray)
        {
            if (Application.isEditor)
            {
                // As we currently have no actual companions in the editor, create a manual response.
                // Doing this here also retains the same pipeline for editor and Gameboard.
                CreateMessageResponseForEditor(inEventId);
            }
            else
            {
                // Not in the editor, so send it off to the Companion Service.
                communicationsUtility.SendMessageToCompanionServer("m", inEventId, inJsonPayload, inByteArray);
            }

            Task awaitEventCompletion = WaitForEventCompletion(inEventId);
            if (await Task.WhenAny(awaitEventCompletion, Task.Delay(gameboardConfig.eventTimeoutLength)) == awaitEventCompletion)
            {
                // Event Completed!
            }
            else
            {
                retriesLeft -= 1;
                if (retriesLeft < 0)
                {
                    // Fatal Timeout
                    GameboardLogging.LogMessage($"--- EventID {inEventId} had a fatal timeout.", GameboardLogging.MessageTypes.Error);
                    EventEnded(DataTypes.EventQueueStates.TimedOut, inEventId);
                }
                else
                {
                    // Retry the event
                    GameboardLogging.LogMessage($"--- EventID {inEventId} timed out. Trying again. {retriesLeft} attempts remaining.", GameboardLogging.MessageTypes.Verbose);
                    SendMessageToCompanionWithRetry(inEventId, retriesLeft, inJsonPayload, inByteArray);
                }
            }
        }
        #endregion

        #region Ending Events
        void EventEnded(DataTypes.EventQueueStates inEndedState, string inEventId)
        {
            if(string.IsNullOrEmpty(inEventId))
            {
                GameboardLogging.LogMessage("--- EventEnded called with a blank event ID!", GameboardLogging.MessageTypes.Error);
                return;
            }

            QueuedEvent targetEvent = CurrentEvents.Find(s => s.eventGuid == inEventId);
            if(targetEvent == null)
            {
                GameboardLogging.LogMessage("--- EventEnded however no event with ID " + inEventId + " was found in CurrentEvents!", GameboardLogging.MessageTypes.Error);
                return;
            }

            EventEnded(inEndedState, targetEvent);
        }

        void EventEnded(DataTypes.EventQueueStates inEndedState, QueuedEvent inEvent)
        {
            if(inEvent == null)
            {
                GameboardLogging.LogMessage("--- EventEnded was called with a null QueuedEvent!", GameboardLogging.MessageTypes.Error);
                return;
            }

            switch (inEndedState)
            {
                case DataTypes.EventQueueStates.WaitingToProcess:
                case DataTypes.EventQueueStates.Processing:
                    GameboardLogging.LogMessage("--- Never call EventEnded with WaitingToProcess or Processing states!", GameboardLogging.MessageTypes.Error);
                break;

                case DataTypes.EventQueueStates.Cancelled:
                    inEvent.EventCancelled(cachedTime);
                break;
                
                case DataTypes.EventQueueStates.TimedOut:
                    inEvent.EventTimedOut(cachedTime);
                break;
                
                case DataTypes.EventQueueStates.Completed:
                    inEvent.EventCompleted(cachedTime);
                break;
            }

            if (inEvent.targetDeviceType == DataTypes.DeviceTypes.Gameboard)
            {
                lock (gameboardProcessingDict)
                {
                    gameboardProcessingDict[inEvent.targetDestinationId] = null;
                }
            }
            else if (inEvent.targetDeviceType == DataTypes.DeviceTypes.Companion)
            {
            }
            else
            {
                GameboardLogging.LogMessage("--- Device type of " + inEvent.targetDeviceType + " is not setup with EventEnded!", GameboardLogging.MessageTypes.Warning);
            }

            lock (EndedEvents)
            {
                EndedEvents.Add(inEvent);
            }

            lock (CurrentEvents)
            {
                CurrentEvents.Remove(inEvent);
            }
        }
        #endregion

        void CompanionServerAckReceived(object origin, ServerMessageCompletionObject completionObject)
        {
            // Check if this event is still waiting on a response. If it isn't, then this is a duplicate response, so ignore it.
            QueuedEvent targetEvent = CurrentEvents.Find(s => s.eventGuid == completionObject.eventId);
            if(targetEvent == null)
            {
                GameboardLogging.LogMessage("--- Ack received for event " + completionObject.eventId + " however no event with this ID is processing!", GameboardLogging.MessageTypes.Warning);
                return;
            }
            else if(targetEvent.eventState != DataTypes.EventQueueStates.Processing)
            {
                if(targetEvent.eventState == DataTypes.EventQueueStates.WaitingToProcess)
                {
                    GameboardLogging.LogMessage("--- Ack received for event " + completionObject.eventId + " however this event never started processing!", GameboardLogging.MessageTypes.Error);
                }

                // Any other reason here and it's safe to assume that this event already completed, therefore this is a duplicate response, so just discard it by doing nothing here, and no need to throw a message.

                return;
            }

            SendMessageToCompanionServiceResponse serviceResponse = null;

            JsonUtilityDeserializeResponse<EventArgsToGameboard> deserializedResponse = jsonUtility.DeserializeObject<EventArgsToGameboard>(completionObject.message);
            if (deserializedResponse.success)
            {
                if (deserializedResponse.deserializedArgs.responseStatus != EventArgsCompanionServerResponseStatus.ResponseStatusCode.OK)
                {
                    serviceResponse = new SendMessageToCompanionServiceResponse()
                    {
                        success = false,
                        eventArgs = null,
                        responseForEventId = completionObject.eventId,
                        errorMessage = "Response Status Not Ok: " + deserializedResponse.deserializedArgs.responseStatus.ToString(),
                    };
                }
                else
                {
                    serviceResponse = new SendMessageToCompanionServiceResponse()
                    {
                        success = true,
                        eventArgs = deserializedResponse.deserializedArgs,
                        responseForEventId = completionObject.eventId,
                    };
                }
            }
            else
            {
                serviceResponse = new SendMessageToCompanionServiceResponse()
                {
                    success = false,
                    eventArgs = null,
                    responseForEventId = completionObject.eventId,
                    errorMessage = "Failed to deserialize response from Companion Server.",
                };
            }

            EventEnded(DataTypes.EventQueueStates.Completed, completionObject.eventId);

            lock (eventResponseList)
            {
                eventResponseList.Add(serviceResponse);
            }
        }

        private CompanionMessageResponseArgs BuildMessageErrorResponseFromErrorMessage(string inErrorMessage)
        {
            CompanionMessageResponseArgs responseArgs = new CompanionMessageResponseArgs()
            {
                errorResponse = new CompanionPlayerPresenceErrorResponse()
                {
                    Message = inErrorMessage
                }
            };

            if(responseArgs == null)
            {
                GameboardLogging.LogMessage("BuildErrorResponseFromErrorMessage created a null responseArgs! This should never happen.", GameboardLogging.MessageTypes.Error);
            }

            return responseArgs;
        }

        private CompanionCreateObjectEventArgs BuildObjectErrorResponseFromErrorMessage(string inErrorMessage)
        {
            CompanionCreateObjectEventArgs responseArgs = new CompanionCreateObjectEventArgs()
            {
                errorResponse = new CompanionPlayerPresenceErrorResponse()
                {
                    Message = inErrorMessage
                }
            };

            if (responseArgs == null)
            {
                GameboardLogging.LogMessage("BuildObjectErrorResponseFromErrorMessage created a null responseArgs! This should never happen.", GameboardLogging.MessageTypes.Error);
            }

            return responseArgs;
        }

        private CompanionErrorResponse CreateErrorResponseIfNeeded<T>(int errorCode)
        {
            try
            {
                if (errorCode == 0)
                {
                    return null;
                }
                else
                {
                    return new CompanionErrorResponse()
                    {
                        Message = Enum.GetValues(typeof(T)).GetValue(errorCode).ToString()
                    };
                }
            }
            catch(Exception e)
            {
                GameboardLogging.LogMessage($"CreateErrorResponseIfNeeded exception. Errorcode: {errorCode}, Response type: {typeof(T).ToString()}. Message: {e.Message}, Exception: {e.InnerException}", GameboardLogging.MessageTypes.Error);
                return null;
            }
        }

        private CompanionMessageResponseArgs CreateCompanionMessageResponseArgs<T>(SendMessageToCompanionServiceResponse inResponse)
        {
            if(inResponse == null)
            {
                return BuildMessageErrorResponseFromErrorMessage($"Entered SendMessageToCompanionServiceResponse was null. Error was: {inResponse.errorMessage}");
            }

            if (inResponse.eventArgs == null)
            {
                return BuildMessageErrorResponseFromErrorMessage($"Entered SendMessageToCompanionServiceResponse eventArgs was null. Error was: {inResponse.errorMessage}");
            }

            if (inResponse.eventArgs.body == null)
            {
                return BuildMessageErrorResponseFromErrorMessage($"Entered SendMessageToCompanionServiceResponse eventArgs.body was null. Error was: {inResponse.errorMessage}");
            }

            if (!inResponse.success)
            {
                return BuildMessageErrorResponseFromErrorMessage(inResponse.errorMessage);
            }

            JsonUtilityDeserializeResponse<CompanionMessageResponseArgs> deserializedBody = jsonUtility.DeserializeObject<CompanionMessageResponseArgs>(inResponse.eventArgs.body);
            if (deserializedBody.success)
            {
                //------------------------------------------------------------------------------------------------------------------
                // This is placeholder code to cover userId being used in some cases in place of ownerId. When all userIds are changed to ownerId, this will be removed.
                if(string.IsNullOrEmpty(deserializedBody.deserializedArgs.ownerId) && !string.IsNullOrEmpty(deserializedBody.deserializedArgs.userId))
                {
                    deserializedBody.deserializedArgs.ownerId = deserializedBody.deserializedArgs.userId;
                }
                //------------------------------------------------------------------------------------------------------------------

                try
                {
                    deserializedBody.deserializedArgs.errorResponse = CreateErrorResponseIfNeeded<T>(deserializedBody.deserializedArgs.errorId);
                    return deserializedBody.deserializedArgs;
                }
                catch(Exception e)
                {
                    GameboardLogging.LogMessage("CreateCompanionMessageResponseArgs failed with exception " + e.Message + " / " + e.InnerException, GameboardLogging.MessageTypes.Error);
                    return BuildMessageErrorResponseFromErrorMessage("exception occured when processing companion response body.");
                }
            }
            else
            {
                return BuildMessageErrorResponseFromErrorMessage("Unable to deserialize companion response body.");
            }
        }

        private CompanionCreateObjectEventArgs CreateCompanionObjectResponseArgs<T>(SendMessageToCompanionServiceResponse inResponse)
        {
            GameboardLogging.Verbose($"CreateCompanionObjectResponseArgs Received Response: {inResponse}");
            GameboardLogging.Verbose($"CreateCompanionObjectResponseArgs Received args: {inResponse.eventArgs}");

            if (inResponse == null)
            {
                return BuildObjectErrorResponseFromErrorMessage($"Entered SendMessageToCompanionServiceResponse was null. Error was: null");
            }

            if (inResponse.eventArgs == null)
            {
                return BuildObjectErrorResponseFromErrorMessage($"Entered SendMessageToCompanionServiceResponse eventArgs was null. Error was: {inResponse.errorMessage}");
            }

            if (inResponse.eventArgs.body == null || inResponse.eventArgs.body == "null")
            {
                return BuildObjectErrorResponseFromErrorMessage($"Entered SendMessageToCompanionServiceResponse eventArgs.body was null. Error was: {inResponse.errorMessage}");
            }

            if (!inResponse.success)
            {
                return BuildObjectErrorResponseFromErrorMessage(inResponse.errorMessage);
            }

            JsonUtilityDeserializeResponse<CompanionCreateObjectEventArgs> createEventArgs = jsonUtility.DeserializeObject<CompanionCreateObjectEventArgs>(inResponse.eventArgs.body);
            if(createEventArgs.success)
            {
                //------------------------------------------------------------------------------------------------------------------
                // This is placeholder code to cover userId being used in some cases in place of ownerId. When all userIds are changed to ownerId, this will be removed.
                if (string.IsNullOrEmpty(createEventArgs.deserializedArgs.ownerId) && !string.IsNullOrEmpty(createEventArgs.deserializedArgs.userId))
                {
                    createEventArgs.deserializedArgs.ownerId = createEventArgs.deserializedArgs.userId;
                }
                //------------------------------------------------------------------------------------------------------------------

                createEventArgs.deserializedArgs.errorResponse = CreateErrorResponseIfNeeded<T>(createEventArgs.deserializedArgs.errorId);
                return createEventArgs.deserializedArgs;
            }
            else
            {
                GameboardLogging.LogMessage($"CreateCompanionObjectResponseArgs failed to deserialize eventArgs.body.", GameboardLogging.MessageTypes.Error);
                return null;
            }
        }

        private CompanionUserPresenceEventArgs CreateCompanionUserPresenceResponseArgs<T>(SendMessageToCompanionServiceResponse inResponse)
        {
            if (!inResponse.success)
            {
                return BuildMessageErrorResponseFromErrorMessage(inResponse.errorMessage) as CompanionUserPresenceEventArgs;
            }

            JsonUtilityDeserializeResponse<List<GameboardUserPresenceEventArgs>> deserializedPlayer = jsonUtility.DeserializeObject<List<GameboardUserPresenceEventArgs>>(inResponse.eventArgs.body);
            if (deserializedPlayer.success)
            {
                CompanionUserPresenceEventArgs responseEventArgs = new CompanionUserPresenceEventArgs()
                {
                    playerPresenceList = deserializedPlayer.deserializedArgs,
                };

                return responseEventArgs;
            }
            else
            {
                return BuildMessageErrorResponseFromErrorMessage("Unable to deserialize companion response body to player presence list.") as CompanionUserPresenceEventArgs;
            }
        }

        /// <summary>
        /// Fabricates a Companion Service response, to allow proper cleaner Companion testing while in the editor.
        /// </summary>
        /// <param name="inVersion"></param>
        /// <param name="inEventId"></param>
        private void CreateMessageResponseForEditor(string inEventId)
        {
            QueuedEvent targetEvent = CurrentEvents.Find(s => s.eventGuid == inEventId);
            ServerMessageCompletionObject completionObject = new ServerMessageCompletionObject()
            {
                eventId = inEventId,
                message = "{\"responseStatus\":\"200\",\"body\":\"[{\\\"ownerId\\\":\\\"" + targetEvent.targetDestinationId + "\\\"}]\"}",
            };

            CompanionServerAckReceived(this, completionObject);
        }
        #endregion

        #region Outgoing Events Originating on Gameboard

        #region System Actions
        public async Task<CompanionMessageResponseArgs> DisplaySystemPopup(int versionTag, string userId, string inTextToDisplay, float inTimeInSecondsToDisplay)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "displaySystemPopup");
            EventArgDisplaySystemPopup displaySystemPopupEventArgs = new EventArgDisplaySystemPopup()
            {
                textToDisplay = inTextToDisplay,
                timeInSecondsToDisplayPopup = inTimeInSecondsToDisplay,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, displaySystemPopupEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionDisplayMessageErrorResponse.DisplayMessageErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> ResetPlayPanel(int versionTag, string userId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "resetPlayPanel");
            EventArgResetPlayPanel resetPlayPanelEventArgs = new EventArgResetPlayPanel()
            {
                // Has no arguments.
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, resetPlayPanelEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionDisplayMessageErrorResponse.DisplayMessageErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> SetTopLabel(int versionTag, string userId, string labelText)
        {
            var topLabel = new TopLabelProperty()
            {
                label = labelText
            };
            return await SetConfiguration(versionTag, userId, topLabel, ConfigurationProperty.TopLabel);
        }
        #endregion

        #region User Presence
        public async Task<CompanionUserPresenceEventArgs> GetUserPresenceList(int versionTag)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetSelfGameboard(versionTag, "getUserPresenceList");

            string eventId = AddEventToGameboardQueue(gameboardConfig.gameboardId, eventArgs, "");
            SendMessageToCompanionServiceResponse response = await AwaitEventAndReceiveResponseData(eventId);

            return CreateCompanionUserPresenceResponseArgs<CompanionUserPresenceEventArgs>(response);
        }
        #endregion

        #region Asset Management
        public async Task<CompanionCreateObjectEventArgs> LoadAsset(int versionTag, string userId, byte[] byteArray, string guid = "")
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "setAsset");
            EventArgObjectAsset objectAsset = new EventArgObjectAsset()
            {
                id = !String.IsNullOrEmpty(guid) ? guid : Guid.NewGuid().ToString(),
                mime = "image/png",
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, objectAsset, byteArray);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            CompanionCreateObjectEventArgs responseArgs = CreateCompanionObjectResponseArgs<CompanionAssetErrorResponse>(companionResponse);
            if(responseArgs.errorResponse != null)
            {
                GameboardLogging.LogMessage("--- CreateCompanionObjectResponseArgs failure: " + responseArgs.errorResponse.Message, GameboardLogging.MessageTypes.Error);
            }

            return responseArgs;
        }

        public async Task<CompanionMessageResponseArgs> DeleteAsset(int versionTag, string userId, string assetId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "deleteAsset");
            EventArgDeleteAsset deleteAssetEventArgs = new EventArgDeleteAsset()
            {
                deleteAssetId = assetId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, deleteAssetEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionContainerErrorResponse.ContainerErrorTypes>(companionResponse);
        }

        private async Task<CompanionMessageResponseArgs> SetConfiguration(int versionTag, string userId, object value, ConfigurationProperty property)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "setConfiguration");
            EventArgSetConfiguration setCardPropertyEventArgs = new EventArgSetConfiguration()
            {
                property = (int)property,
                value = value,
                ownerId = gameboardConfig.gameboardId,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, setCardPropertyEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionPropertyErrorResponse>(companionResponse);
        }

        #endregion

        #region Object Management
        public async Task<CompanionMessageResponseArgs> changeObjectDisplayState(int versionTag, string userId, string inObjectId, DataTypes.ObjectDisplayStates inNewDisplayState)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "changeObjectDisplayState");
            EventArgChangeObjectDisplayState changeObjectDisplayStateArgs = new EventArgChangeObjectDisplayState()
            {
                objectId = inObjectId,
                newDisplayState = inNewDisplayState
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, changeObjectDisplayStateArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionDialogErrorResponse.DialogErrorTypes>(companionResponse);
        }
        #endregion

        #region Models
        public async Task<CompanionMessageResponseArgs> TintModelWithRGBColor(int versionTag, string userId, string objectId, Color rgbColor)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "TintModelWithRGBColor");
            EventArgTintModelColor tintModelColorEventArgs = new EventArgTintModelColor()
            {
                objectIdToTint = objectId,
                hexColorForTint = ColorUtility.ToHtmlStringRGBA(rgbColor)
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, tintModelColorEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionModelErrorResponse.ModelErrorTypes>(companionResponse);
        }
        #endregion

        #region Cards
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionCard(int versionTag, string userId, string cardId, string inFrontTextureId, string inBackTextureId, int inTextureWidth, int inTextureHeight)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionCard");
            EventArgCreateCard createCardEventArg = new EventArgCreateCard()
            {
                id = cardId,
                frontTextureId = string.IsNullOrEmpty(inFrontTextureId) ? "0" : inFrontTextureId,
                backTextureId = string.IsNullOrEmpty(inBackTextureId) ? "0" : inBackTextureId,
                width = inTextureWidth,
                height = inTextureHeight,

                // cardModelId = // In place for Phase 2 
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createCardEventArg);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionCardErrorResponse>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> SetFacingDirectionOfCard(int versionTag, string userId, string inCardId, DataTypes.CardFacingDirections inFacingDirection)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "setFacingDirectionOfCard");
            EventArgSetCardFacingDirection setCardFacingDirectionEventArgs = new EventArgSetCardFacingDirection()
            {
                cardId = inCardId,
                newFacingDirection = inFacingDirection
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, setCardFacingDirectionEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionCardErrorResponse.CardErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> SetControlAsset(int versionTag, string userId, ControlAssetType assetType, string assetGuid)
        {
            Enum.TryParse(Enum.GetName(typeof(ControlAssetType), assetType), out ConfigurationProperty property);
            return await SetConfiguration(versionTag, userId, assetGuid, property);
        }

        public async Task<CompanionMessageResponseArgs> SetCardTemplate(int versionTag, string userId, CompanionCardTemplateType templateType)
        {
            return await SetConfiguration(versionTag, userId, (int)templateType, ConfigurationProperty.CardTemplate);
        }
        public async Task<CompanionMessageResponseArgs> SetCardHighlight(int versionTag, string userId, CardHighlights cardHighlight)
        {
            return await SetCardProperty(versionTag, userId, cardHighlight, CardProperty.Highlight);
        }

        public async Task<CompanionMessageResponseArgs> SetCardFrontAssetId(int versionTag, string userId, CardAssetId cardAssetId)
        {
            return await SetCardProperty(versionTag, userId, cardAssetId, CardProperty.FrontAssetId);
        }

        public async Task<CompanionMessageResponseArgs> SetCardBackAssetId(int versionTag, string userId, CardAssetId cardAssetId)
        {
            return await SetCardProperty(versionTag, userId, cardAssetId, CardProperty.BackAssetId);
        }

        private async Task<CompanionMessageResponseArgs> SetCardProperty(int versionTag, string userId, object value, CardProperty property)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "setCardProperty");
            EventArgsSetCardProperty setCardPropertyEventArgs = new EventArgsSetCardProperty()
            {
                property = (int)property,
                value = value,
                ownerId = gameboardConfig.gameboardId,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, setCardPropertyEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionPropertyErrorResponse.CompanionPropertyErrorTypes>(companionResponse);
        }

        #endregion

        #region Card Hand Displays
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionHandDisplay(int versionTag, string userId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionHandDisplay");
            EventArgCreateHandDisplay createHandDisplayArgs = new EventArgCreateHandDisplay()
            {
                id = Guid.NewGuid().ToString(),
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createHandDisplayArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            CompanionCreateObjectEventArgs createObjectEventArgs = CreateCompanionObjectResponseArgs<CompanionHandDisplayErrorResponse>(companionResponse);

            if(Application.isEditor)
            {
                createObjectEventArgs.newObjectUid = createHandDisplayArgs.id;
            }

            return createObjectEventArgs;
        }

        public async Task<CompanionMessageResponseArgs> AddCardToHand(int versionTag, string userId, string inCardHandDisplayId, string inCardId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "addCardToHand");
            EventArgAddCardToHand cardHandArgs = new EventArgAddCardToHand()
            {
                cardHandDisplayId = inCardHandDisplayId,
                cardId = inCardId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, cardHandArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionHandDisplayErrorResponse.CompanionHandDisplayErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> RemoveCardFromHand(int versionTag, string userId, string inCardHandDisplayId, string inRemoveCardId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "removeCardFromHand");
            EventArgRemoveCardFromHand removeCardFromHandArgs = new EventArgRemoveCardFromHand()
            {
                handDisplayId = inCardHandDisplayId,
                cardToRemoveId = inRemoveCardId,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, removeCardFromHandArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionHandDisplayErrorResponse.CompanionHandDisplayErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> RemoveAllCardsFromHand(int versionTag, string userId, string cardHandDisplayId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "removeAllCardsFromHand");
            EventArgRemoveAllCardsFromHand removeAllCardsFromHandArgs = new EventArgRemoveAllCardsFromHand()
            {
                handDisplayId = cardHandDisplayId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, removeAllCardsFromHandArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionHandDisplayErrorResponse.CompanionHandDisplayErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> ShowCompanionHandDisplay(int versionTag, string userId, string cardHandDisplayId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "showCompanionHandDisplay");
            EventArgShowCompanionHandDisplay showHandDisplayEventArgs = new EventArgShowCompanionHandDisplay()
            {
                id = cardHandDisplayId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, showHandDisplayEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionHandDisplayErrorResponse.CompanionHandDisplayErrorTypes>(companionResponse);
        }
        #endregion

        #region Dice
        public async Task<CompanionMessageResponseArgs> RollDice(int versionTag, string userId, int[] inDiceSizesToRoll, int inAddedModifier, Color inDiceTint, string inDiceNotation)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "rollDice");
            EventArgRollDice rollDiceEventArgs = new EventArgRollDice()
            {
                diceSizesToRoll = inDiceSizesToRoll,
                addedModifier = inAddedModifier,
                diceTintHexColor = $"#{ColorUtility.ToHtmlStringRGB(inDiceTint)}",
                diceNotation = inDiceNotation,
                ownerId = gameboardConfig.gameboardId,
                //orderedDiceTextureUIDs // Phase 2
                //customDiceUIDs // Phase 4
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, rollDiceEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionDiceRollErrorResponse>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> SetDiceBackground(int versionTag, string userId, string assetGuid)
        {
            return await SetConfiguration(versionTag, userId, assetGuid, ConfigurationProperty.DiceBackgroundAssetId);
        }

        public async Task<CompanionMessageResponseArgs> SetDiceSelectorVisibility(int versionTag, string userId, bool visible)
        {
            return await SetConfiguration(versionTag, userId, visible, ConfigurationProperty.DiceSelectorEnabled);
        }

        #endregion

        #region Mats
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionMat(int versionTag, string userId, string inBackgroundTextureUid)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionMat");
            EventArgCreateCompanionMat createCompanionMatArgs = new EventArgCreateCompanionMat()
            {
                id = Guid.NewGuid().ToString(),
                backgroundTextureId = inBackgroundTextureUid
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createCompanionMatArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionMatErrorResponse>(companionResponse);
        }

        public async Task<CompanionCreateObjectEventArgs> AddZoneToMat(int versionTag, string userId, string inCompanionMatId, Vector2 inZoneCenter, Vector2 inSizeOfZone)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "addZoneToMat");
            EventArgAddZoneToMat addZoneToMatArgs = new EventArgAddZoneToMat()
            {
                companionMatId = inCompanionMatId,
                zoneCenterPosition = inZoneCenter,
                zoneSize = inSizeOfZone
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, addZoneToMatArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionMatErrorResponse>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> AddObjectToCompanionMat(int versionTag, string userId, string inCompanionMatId, string inObjectToAddId, Vector2 inPositionOfObject)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "addObjectToMat");
            EventArgAddObjectToMat addObjectToMatArgs = new EventArgAddObjectToMat()
            {
                companionMatId = inCompanionMatId,
                objectToAddId = inObjectToAddId,
                positionOfObject = inPositionOfObject,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, addObjectToMatArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionMatErrorResponse.MatErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> RemoveObjectFromCompanionMat(int versionTag, string userId, string inCompanionMatId, string inObjectToRemoveId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "removeObjectFromMat");
            EventArgRemoveObjectFromMat removeObjectFromMatArgs = new EventArgRemoveObjectFromMat()
            {
                matId = inCompanionMatId,
                objectToRemoveId = inObjectToRemoveId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, removeObjectFromMatArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionMatErrorResponse.MatErrorTypes>(companionResponse);
        }
        #endregion

        #region Dialogs
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionDialog(int versionTag, string userId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionDialog");
            EventArgCreateDialog createDialogEventArgs = new EventArgCreateDialog()
            {
                id = Guid.NewGuid().ToString(),
                // dialogSize, // Setup for Phase 2
                // dialogPosition, // Setup for Phase 2
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createDialogEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionDialogErrorResponse>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> AddObjectToDialog(int versionTag, string userId, string inDialogId, string inObjectToAddId, Vector2 inAttachPosition)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "addObjectToDialog");
            EventArgAddObjectToDialog addObjectToDialogArgs = new EventArgAddObjectToDialog()
            {
                dialogId = inDialogId,
                objectToAddId = inObjectToAddId,
                attachPosition = inAttachPosition
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, addObjectToDialogArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionDialogErrorResponse.DialogErrorTypes>(companionResponse);
        }
        #endregion

        #region Buttons
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionButton(int versionTag, string userId, string newButtonText, string inButtonDownTextureId, string inButtonUpTextureId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionButton");
            EventArgCreateButton createButtonEventArgs = new EventArgCreateButton()
            {
                id = Guid.NewGuid().ToString(),
                buttonText = newButtonText,
                buttonDownTextureId = inButtonDownTextureId,
                buttonUpTextureId = inButtonDownTextureId,
                ownerId = gameboardConfig.gameboardId,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createButtonEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionButtonErrorResponse>(companionResponse);
        }
        #endregion

        #region Dropdowns
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionDropdown(int versionTag, string userId, List<string> orderedStringList, int defaultIndex)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionDropdown");
            EventArgCreateDropdown createDropdownArgs = new EventArgCreateDropdown()
            {
                id = Guid.NewGuid().ToString(),
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createDropdownArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionDropdownErrorResponse>(companionResponse);
        }
        #endregion

        #region Labels
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionLabel(int versionTag, string userId, string textToDisplay)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionLabel");
            EventArgCreateCompanionLabel createCompanionLabelArgs = new EventArgCreateCompanionLabel()
            {
                id = Guid.NewGuid().ToString(),
                labelText = textToDisplay,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createCompanionLabelArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionLabelErrorResponse>(companionResponse);
        }
        #endregion

        #region Tickboxes
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionTickbox(int versionTag, string userId, string inOnTextureId, string inOffTextureId, DataTypes.TickboxStates inStartingState, bool inIsInputLocked)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createCompanionTickbox");
            EventArgCreateCompanionTickbox createCompanionTickboxEventArgs = new EventArgCreateCompanionTickbox()
            {
                id = Guid.NewGuid().ToString(),
                onTextureId = inOnTextureId,
                offTextureId = inOffTextureId,
                startingState = inStartingState,
                isInputLocked = inIsInputLocked,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createCompanionTickboxEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionTickboxErrorResponse>(companionResponse);
        }
        #endregion

        #region Containers
        public async Task<CompanionCreateObjectEventArgs> CreateContainer(int versionTag, string userId, DataTypes.CompanionContainerSortingTypes inSortingType)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "createContainer");
            EventArgCreateContainer createContainerEventArgs = new EventArgCreateContainer()
            {
                id = Guid.NewGuid().ToString(),
                sortingType = inSortingType,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, createContainerEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionContainerErrorResponse.ContainerErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> PlaceObjectInContainer(int versionTag, string userId, string inContainerId, string inObjectToPlaceId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "placeObjectInContainer");
            EventArgPlaceObjectInContainer placeObjInContainerEventArgs = new EventArgPlaceObjectInContainer()
            {
                containerId = inContainerId,
                objectToPlaceId = inObjectToPlaceId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, placeObjInContainerEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionContainerErrorResponse.ContainerErrorTypes>(companionResponse);
        }

        public async Task<CompanionMessageResponseArgs> RemoveObjectFromContainer(int versionTag, string userId, string inContainerId, string inObjectToRemoveId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "removeObjectFromContainer");
            EventArgRemoveObjectFromContainer removeObjFromContainerEventArgs = new EventArgRemoveObjectFromContainer()
            {
                containerId = inContainerId,
                objectToRemoveId = inObjectToRemoveId
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, removeObjFromContainerEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionMessageResponseArgs<CompanionContainerErrorResponse.ContainerErrorTypes>(companionResponse);
        }
        #endregion

        #region Companion Buttons
        public async Task<CompanionMessageResponseArgs> SetCompanionButtonValues(int versionTag, string userId, string buttonId, string inButtonLabelText, string inButtonCallback, string assetId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetUser(versionTag, userId, "setCompanionButton");
            EventArgSetCompanionButton setCompanionButtonEventArgs;
            setCompanionButtonEventArgs = new EventArgSetCompanionButton()
            {
                buttonId = buttonId,
                buttonText = inButtonLabelText,
                buttonCallback = inButtonCallback,
                assetId = assetId,
            };

            string eventId = AddEventToCompanionQueue(userId, eventArgs, setCompanionButtonEventArgs);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitQueuedCompanionEvent(userId, eventId);

            return CreateCompanionObjectResponseArgs<CompanionContainerErrorResponse.ContainerErrorTypes>(companionResponse);
        }
        #endregion

        #endregion

        #region Incoming Events Originating on Companions
        public void StartGame(int versionTag)
        {
            throw new System.NotImplementedException();
        }

        void ManageIncomingEvent<T>(IncomingEventDelegate<T> inEvent, EventArgsToGameboard inEventArgs, string inEndpoint)
        {
            if(inEvent == null)
            {
                GameboardLogging.LogMessage("No event was supplied to ManageIncomingEvent!", GameboardLogging.MessageTypes.Error);
                return;
            }

            if(inEvent.GetInvocationList().Length == 0)
            {
                GameboardLogging.LogMessage("The supplied event for ManageIncomingEvent has no listeners!", GameboardLogging.MessageTypes.Warning);
                return;
            }

            if (inEventArgs == null)
            {
                GameboardLogging.LogMessage("EventArgs were null for ManageIncomingEvent!", GameboardLogging.MessageTypes.Error);
                return;
            }

            if (inEventArgs.body == null)
            {
                GameboardLogging.LogMessage("EventArgs.Body was null for ManageIncomingEvent!", GameboardLogging.MessageTypes.Error);
                return;
            }

            JsonUtilityDeserializeResponse<T> deserializedIncomingArgs = jsonUtility.DeserializeObject<T>(inEventArgs.body);
            if (deserializedIncomingArgs.success)
            {
                GameboardIncomingEventArg eventArgs = (deserializedIncomingArgs.deserializedArgs as GameboardIncomingEventArg);
                //------------------------------------------------------------------------------------------------------------------
                // This is placeholder code to cover userId being used in some cases in place of ownerId. When all userIds are changed to ownerId, this will be removed.
                if (string.IsNullOrEmpty(eventArgs.ownerId) && !string.IsNullOrEmpty(eventArgs.userId))
                {
                    eventArgs.ownerId = eventArgs.userId;
                }
                //------------------------------------------------------------------------------------------------------------------

                inEvent?.Invoke(deserializedIncomingArgs.deserializedArgs);
            }
            else
            {
                GameboardLogging.LogMessage($"ManageIncomingEventBody: Failed to deserialize incoming event body! Received body was: {inEventArgs.body}.", GameboardLogging.MessageTypes.Error);
            }
        }

        public void EndGame(int versionTag)
        {
            throw new System.NotImplementedException();
        }

        public void AddUser(int versionTag)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Editor Only Actions
#if UNITY_EDITOR
        public async Task<CompanionUserPresenceEventArgs> UserLeft(int versionTag, string inUserId)
        {
            EventArgsToCompanionServer eventArgs = BuildEventArgsToTargetSelfGameboard(versionTag, "leave");

            string eventId = Guid.NewGuid().ToString();
            SendRawBodyMessageToCompanionService(eventArgs, inUserId, eventId, null);
            SendMessageToCompanionServiceResponse companionResponse = await AwaitEventAndReceiveResponseData(eventId);

            return CreateCompanionUserPresenceResponseArgs<CompanionUserPresenceEventArgs>(companionResponse);
        }
#endif
        #endregion
    }
}