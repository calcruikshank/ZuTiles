using System;
using System.Collections.Generic;

namespace Gameboard.TUIO
{
    public class DatMessage
    {
        public uint s_id;

        private Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();

        public void AddData(string mimeType, byte[] value)
        {
            dict.Remove(mimeType);
            dict.Add(mimeType, value);
        }

        public byte[] GetData(string mimeType)
        {
            byte[] retVal;
            dict.TryGetValue(mimeType, out retVal);

            return retVal;
        }
    }
}