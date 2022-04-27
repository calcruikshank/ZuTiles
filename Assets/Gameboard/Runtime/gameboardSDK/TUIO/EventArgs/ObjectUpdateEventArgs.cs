namespace Gameboard.TUIO
{
    public class ObjectUpdateEventArgs
    {
        public uint s_id;
        public TUIOObject obj;

        public bool UpdatedTok;
        public bool UpdatedPtr;
        public bool UpdatedChg;
        public bool UpdatedBnd;
        public bool UpdatedDat;
    }
}