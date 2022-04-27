namespace Gameboard.EventArgs
{
    public class CompanionQueuedEvent : QueuedEvent
    {
        public string targetUserId { get { return targetDestinationId; } set { targetDestinationId = value; } }
        public override DataTypes.DeviceTypes targetDeviceType { get { return DataTypes.DeviceTypes.Companion; } }
    }
}