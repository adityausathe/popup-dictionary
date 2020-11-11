using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PopupDictionary
{
        [Serializable]
        public class UserPreferences
        {
            internal int _SearchEngine;
            public int SearchEngine
            {
                get { return _SearchEngine; }
                set { _SearchEngine = value; }

            }

            internal int _KeyConfig;
            public int KeyConfig
            {
                get { return _KeyConfig; }
                set { _KeyConfig = value; }

            }

            internal bool _status;
            public bool status
            {
                get { return _status; }
                set { _status = value; }

            }
    }
    }

