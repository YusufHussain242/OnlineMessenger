using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs_OnlineMessenger2020
{
    [Serializable]

    public class DTOs
    {
        public int variableIndex { get; set; }
        public byte[] data { get; set; }

        public DTOs() { }

        public DTOs(int VariableIndex, byte[] Data)
        {
            this.variableIndex = VariableIndex;
            this.data = Data;
        }

    }
}
