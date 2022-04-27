
/*
 
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
   
   
    // > receive
    // 127.0.0.1 : 8051
   
    // send
    // nc -u 127.0.0.1 8051
 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using OSCsharp.Data;
using OSCsharp.Net;
using OSCsharp.Utils;

using Gameboard.Utilities;

namespace Gameboard.TUIO
{
    public class TUIOClient
    {
        System.Collections.Generic.IEnumerable<uint> tracked = Enumerable.Empty<uint>();
        Dictionary<uint, TUIOObject> objects = new Dictionary<uint, TUIOObject>();
        Dictionary<uint, ObjectUpdateEventArgs> updates = new Dictionary<uint, ObjectUpdateEventArgs>();
        private UDPReceiver udpReceiver;

        public int Port { get; private set; }

        public bool IsConnected { get { return udpReceiver.IsRunning; } }

        public FrmMessage LastFrame;

        public event EventHandler<ObjectCreateEventArgs> OnObjectCreate;
        public event EventHandler<ObjectDeleteEventArgs> OnObjectDelete;
        public event EventHandler<ObjectUpdateEventArgs> OnObjectUpdate;
        public event EventHandler<ExceptionEventArgs> OnErrorOccured;
        public event EventHandler<DebugHeatmapMessage> OnDebugHeatMapUpdated;

        private IJsonUtility jsonUtility;

        public TUIOClient(int port, IJsonUtility inJsonUtility)
        {
            jsonUtility = inJsonUtility;

            Port = port;
            udpReceiver = new UDPReceiver(Port, false);
            udpReceiver.MessageReceived += handlerOscMessageReceived;
            udpReceiver.ErrorOccured += handlerOscErrorOccured;
        }

        public void Connect()
        {
            if (!udpReceiver.IsRunning)
            {
                udpReceiver.Start();
            }
        }

        public void Disconnect()
        {
            if (udpReceiver.IsRunning)
            {
                udpReceiver.Stop();
            }
        }

        private void createTrackedObject(uint id)
        {
            if (!objects.ContainsKey(id))
            {
                var obj = new TUIOObject();
                obj.s_id = id;
                objects.Add(id, obj);
            }
            if (!updates.ContainsKey(id))
            {
                var update = new ObjectUpdateEventArgs();
                update.s_id = id;
                updates.Add(id, update);
            }
        }

        private bool newFrameReceived = false;
        private void parseOscMessage(OscMessage message)
        {
            uint id;
            ObjectUpdateEventArgs updateArgs;

            string address;
            if ((message.Address.StartsWith("/tuio2/") || message.Address.StartsWith("/tuiox/")) && message.Address.Length > 6)
            {
                address = message.Address.Substring(7);
            }
            else
            {
                Debug.Log($"Not a tuio2 message. {message.Address} not handled");
                return;
            }

            // If this isn't a frm (frame) type of message, and we are waiting to receive a new frame because we got an old one out of order, abort now.
            if(address != "frm" && !newFrameReceived)
            {
                return;
            }

            //TODO: this is an ever growing switch, put this logic elsewhere
            switch (address)
            {
                case "frm":
                    if (message.Data.Count == 0)
                    {
                        return;
                    }

                    uint frameId = (uint)(int)message.Data[0];
                    if(LastFrame != null && frameId < LastFrame.f_id)
                    {
                        // Older frame - abort, and wait until we get a new frame before continuing.
                        newFrameReceived = false;
                        return;
                    }

                    FrmMessage frm = new FrmMessage();
                    frm.f_id = frameId;
                    frm.time = ((OSCsharp.Data.OscTimeTag)message.Data[1]).FractionalSecond;
                    uint res = (uint)(int)message.Data[2];
                    frm.dim_width = res & 0xffff;
                    frm.dim_height = res >> 16;
                    frm.source = (string)message.Data[3];

                    LastFrame = frm;

                    // Got a new frame, so message actions may now continue;
                    newFrameReceived = true;

                    break;

                case "alv":
                    if (message.Data.Count == 0)
                    {
                        if (OnObjectDelete != null)
                        {
                            // Nothing being kept alive, so remove all sessions.
                            foreach (uint sessionId in tracked)
                            {
                                GameboardLogging.LogMessage($"alv mission session id {sessionId} will now be deleted.", GameboardLogging.MessageTypes.Verbose);

                                var eventArgs = new ObjectDeleteEventArgs();
                                eventArgs.s_id = sessionId;
                                OnObjectDelete(this, eventArgs);
                            }
                        }

                        tracked = Enumerable.Empty<uint>();
                        objects.Clear();
                    }
                    else
                    {
                        uint[] sessionIds = new uint[message.Data.Count];
                        for (int i = 0; i < message.Data.Count; i++)
                        {
                            sessionIds[i] = (uint)(int)message.Data[i];
                        }

                        //Get the new session ids
                        var newSessionIds = sessionIds.Except(tracked);
                        foreach (var i in newSessionIds)
                        {
                            if (OnObjectCreate != null)
                            {
                                GameboardLogging.LogMessage($"alv mission session id {i} doesn't exist, now being created.", GameboardLogging.MessageTypes.Verbose);
                                var eventArgs = new ObjectCreateEventArgs();
                                eventArgs.s_id = i;
                                OnObjectCreate(this, eventArgs);
                            }
                        }
                        List<uint> toRemove = new List<uint>();

                        //Get the missing session ids
                        var missingSessionIds = tracked.Except(sessionIds);
                        foreach (var i in missingSessionIds)
                        {
                            if (OnObjectDelete != null)
                            {
                                GameboardLogging.LogMessage($"alv mission session id {i} will now be deleted.", GameboardLogging.MessageTypes.Verbose);

                                var eventArgs = new ObjectDeleteEventArgs();
                                eventArgs.s_id = i;
                                OnObjectDelete(this, eventArgs);
                            }
                            toRemove.Add(i);
                        }

                        //Remove all the session ids we don't see anymore
                        foreach (var i in toRemove)
                        {
                            objects.Remove(i);
                        }

                        tracked = sessionIds;
                        foreach (var i in objects)
                        {
                            if (OnObjectUpdate != null)
                            {
                                ObjectUpdateEventArgs eventArgs;
                                updates.TryGetValue(i.Key, out eventArgs);
                                eventArgs.s_id = i.Key;
                                eventArgs.obj = i.Value;
                                OnObjectUpdate(this, eventArgs);
                                updates.Remove(i.Key);
                            }
                        }
                    }
                    break;

                case "tok":

                    if (message.Data.Count != 6 && message.Data.Count != 11)
                    {
                        GameboardLogging.LogMessage($"tok incorrect size. Acquired size was {message.Data.Count} however should be 6 or 11.", GameboardLogging.MessageTypes.Error);
                        GameboardLogging.LogMessage($"Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                        return;
                    }

                    id = (uint)(int)message.Data[0];
                    createTrackedObject(id);

                    TokMessage tok;

                    if (objects[id].HasTokMessage())
                    {
                        tok = objects[id].tok;
                    }
                    else
                    {
                        tok = new TokMessage();
                        objects[id].tok = tok;
                        tok.s_id = id;
                    }

                    tok.tu_id = (uint)(int)message.Data[1];
                    tok.c_id = (uint)(int)message.Data[2];
                    tok.x_pos = (float)message.Data[3];
                    tok.y_pos = (float)message.Data[4];

                    tok.angle = (float)message.Data[5];

                    if (message.Data.Count == 11)
                    {
                        tok.x_vel = (float)message.Data[6];
                        tok.y_vel = (float)message.Data[7];
                        tok.a_vel = (float)message.Data[8];
                        tok.m_acc = (float)message.Data[9];
                        tok.r_acc = (float)message.Data[10];
                    }

                    updates.TryGetValue(id, out updateArgs);
                    updateArgs.UpdatedTok = true;

                    break;

                case "ptr":

                    if (message.Data.Count != 9 && message.Data.Count != 14)
                    {
                        GameboardLogging.LogMessage($"ptr incorrect size. Acquired was was {message.Data.Count} however should be 9 or 14.", GameboardLogging.MessageTypes.Error);
                        GameboardLogging.LogMessage($"Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                        return;
                    }

                    id = (uint)(int)message.Data[0];
                    createTrackedObject(id);

                    PtrMessage ptr;

                    if (objects[id].HasPtrMessage())
                    {
                        ptr = objects[id].ptr;
                    }
                    else
                    {
                        ptr = new PtrMessage();
                        objects[id].ptr = ptr;
                        ptr.s_id = id;
                    }

                    ptr.tu_id = (uint)(int)message.Data[1];
                    ptr.c_id = (uint)(int)message.Data[2];
                    ptr.x_pos = (float)message.Data[3];
                    ptr.y_pos = (float)message.Data[4];

                    ptr.angle = (float)message.Data[5];
                    ptr.shear = (float)message.Data[6];
                    ptr.radius = (float)message.Data[7];
                    ptr.press = (float)message.Data[8];

                    if (message.Data.Count == 14)
                    {
                        ptr.x_vel = (float)message.Data[9];
                        ptr.y_vel = (float)message.Data[10];
                        ptr.p_vel = (float)message.Data[11];
                        ptr.m_acc = (float)message.Data[12];
                        ptr.p_acc = (float)message.Data[13];
                    }

                    updates.TryGetValue(id, out updateArgs);
                    updateArgs.UpdatedPtr = true;

                    break;

                case "chg":
                    if (message.Data.Count <= 1)
                    {
                        GameboardLogging.LogMessage($"chg incorrect size. Acquired size was {message.Data.Count} but should be more than 0.", GameboardLogging.MessageTypes.Error);
                        GameboardLogging.LogMessage($"Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                        return;
                    }

                    //GameboardLogging.LogMessage($"CHG Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);

                    int numOfPoints = (message.Data.Count - 1) / 2;
                    if (numOfPoints >= 3) // Minimum shape needs 3 points, as this at least makes a closed triangle.
                    {
                        id = (uint)(int)message.Data[0];
                        createTrackedObject(id);

                        ChgMessage chg;
                        if (objects[id].HasChgMessage())
                        {
                            chg = objects[id].chg;
                        }
                        else
                        {
                            chg = new ChgMessage();
                            objects[id].chg = chg;
                            chg.s_id = id;
                        }
                        chg.contour = new Tuple<float, float>[numOfPoints];
                        for (int i = 0; i < numOfPoints; i++)
                        {
                            chg.contour[i] = new Tuple<float, float>((float)message.Data[1 + i * 2], (float)message.Data[2 + i * 2]);

#if GAMEBOARD_VERBOSE_CONTOUR_LOGGING
                            GameboardLogging.LogMessage($"Raw TUIO Contour: Session ID {chg.s_id}, Point {i} = {chg.contour[i]}", GameboardLogging.MessageTypes.Verbose);
#endif
                        }

                        updates.TryGetValue(id, out updateArgs);
                        updateArgs.UpdatedChg = true;
                    }
                    else
                    {
                        GameboardLogging.LogMessage($"chg point count was {numOfPoints} but needs to be at least 3. Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                    }    
                    break;

                case "bnd":
                    if (message.Data.Count != 7 && message.Data.Count != 12)
                    {
                        GameboardLogging.LogMessage($"bnd incorrect size. Acquired size was {message.Data.Count} however shold be 7 or 12.", GameboardLogging.MessageTypes.Error);
                        GameboardLogging.LogMessage($"Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                        return;
                    }

                    id = (uint)(int)message.Data[0];
                    createTrackedObject(id);

                    BndMessage bnd;
                    if (objects[id].HasBndMessage())
                    {
                        bnd = objects[id].bnd;
                    }
                    else
                    {
                        bnd = new BndMessage();
                        objects[id].bnd = bnd;
                        bnd.s_id = id;
                    }

                    bnd.x_pos = (float)message.Data[1];
                    bnd.y_pos = (float)message.Data[2];
                    bnd.angle = (float)message.Data[3];
                    bnd.width = (float)message.Data[4];
                    bnd.height = (float)message.Data[5];
                    bnd.area = (float)message.Data[6];

                    if (message.Data.Count == 12)
                    {
                        bnd.x_vel = (float)message.Data[7];
                        bnd.y_vel = (float)message.Data[8];
                        bnd.a_vel = (float)message.Data[9];
                        bnd.m_acc = (float)message.Data[10];
                        bnd.r_acc = (float)message.Data[11];
                    }

                    objects[id].bnd = bnd;

                    updates.TryGetValue(id, out updateArgs);
                    updateArgs.UpdatedBnd = true;

                    break;

                case "dat":
                    if (message.Data.Count != 3)
                    {
                        GameboardLogging.LogMessage($"dat incorrect size. Acquired size was {message.Data.Count} but should be 3.", GameboardLogging.MessageTypes.Error);
                        GameboardLogging.LogMessage($"Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                        return;
                    }

                    id = (uint)(int)message.Data[0];
                    createTrackedObject(id);

                    DatMessage dat;
                    if (objects[id].HasDatMessage())
                    {
                        dat = objects[id].dat;
                    }
                    else
                    {
                        dat = new DatMessage();
                        objects[id].dat = dat;
                        dat.s_id = id;
                    }

                    dat.AddData((string)message.Data[1], (byte[])message.Data[2]);
                    updates.TryGetValue(id, out updateArgs);
                    updateArgs.UpdatedDat = true;
                    break;

                case "gb_debug_heatmap":
                    DebugHeatmapMessage heatmapMessage = new DebugHeatmapMessage();
                    
                    int row = (int)message.Data[0];
                    int col = (int)message.Data[1];
                    int startRow = (int)message.Data[2];
                    int endRow = (int)message.Data[3];
                    byte[] dataBytes = (byte[])message.Data[4];

                    heatmapMessage.heatmapData = new float[row, col];
                    for(int i = 0; i < dataBytes.Length - 1; i++)
                    {
                        int mapX = i % col;
                        int mapY = i / col;
                        heatmapMessage.heatmapData[mapX, mapY] = dataBytes[i] / 255f;
                    }

                    OnDebugHeatMapUpdated?.Invoke(this, heatmapMessage);

                    break;

                    default:
                        GameboardLogging.LogMessage($"{message.Address} not handled. Raw TUIO Message: {jsonUtility.SerializeObjectAssumeSuccess(message)}", GameboardLogging.MessageTypes.Verbose);
                    break;
            }
        }

        private void handlerOscErrorOccured(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            OnErrorOccured?.Invoke(this, exceptionEventArgs);
        }

        private void handlerOscMessageReceived(object sender, OscMessageReceivedEventArgs oscMessageReceivedEventArgs)
        {
            parseOscMessage(oscMessageReceivedEventArgs.Message);
        }

    }
}