using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP3
{
    public class DNSAnswer
    {
        string name;
        public UInt16 type;
        UInt16 addressclass;
        UInt16 TTL;
        UInt16 DataLength;
        public byte[] data;

        public DNSAnswer(byte[] ba, ref int ind)
        {
            name = "";
            var nb = ba[ind++];
            while (nb != 0)
            {
                if (nb > 20)
                {
                    ind++;
                    break;
                }
                char[] tmpstr = new char[nb];
                for (int i = 0; i < nb; i++)
                {
                    tmpstr[i] = (char)ba[ind++];
                }
                name += new string(tmpstr);
                nb = ba[ind++];
                if (nb != 0)
                {
                    name += ".";
                }
            }
            type = 0;
            type |= (UInt16)(ba[ind++] << 4);
            type |= (UInt16)(ba[ind++]);

            addressclass = 0;
            addressclass |= (UInt16)(ba[ind++] << 4);
            addressclass |= (UInt16)(ba[ind++]);

            TTL = 0;
            TTL |= (UInt16)(ba[ind++] << 12);
            TTL |= (UInt16)(ba[ind++] << 8);
            TTL |= (UInt16)(ba[ind++] << 4);
            TTL |= (UInt16)(ba[ind++]);

            DataLength = 0;
            DataLength |= (UInt16)(ba[ind++] << 4);
            DataLength |= (UInt16)(ba[ind++]);

            if (DataLength != 4)
            {
                Console.WriteLine("Something weird happened");
            }

            data = new byte[DataLength];

            for (int i = 0; i < DataLength; i++)
            {
                data[i] = ba[ind++];
            }
        }
    }
}
