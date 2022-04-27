namespace Gameboard.EventArgs
{
    public class GameboardQueuedEvent : QueuedEvent
    {
        public string targetGameboardId { get { return targetDestinationId; } set { targetDestinationId = value; } }
        public override DataTypes.DeviceTypes targetDeviceType { get { return DataTypes.DeviceTypes.Gameboard; } }
    }
}