namespace Gameboard.EventArgs
{
    public class CompanionMatZoneEventArgs : CompanionMessageResponseArgs
    {
        public string userIdWhoTriggeredZone;
        public string matZoneIdTriggered;
        public string objectIdDroppedInZone;
    }
}