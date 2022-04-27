using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.TUIO
{
    public class TUIOObject
    {
        public uint s_id; //session id
        public TokMessage tok;
        public PtrMessage ptr;
        public ChgMessage chg;
        public BndMessage bnd;
        public DatMessage dat;

        public bool HasTokMessage()
        {
            return tok != null;
        }

        public bool HasPtrMessage()
        {
            return ptr != null;
        }

        public bool HasChgMessage()
        {
            return chg != null;
        }

        public bool HasDatMessage()
        {
            return dat != null;
        }
        public bool HasBndMessage()
        {
            return bnd != null;
        }

    }
}