using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP3
{
    public class DNSQuestion
    {
        string name;
        UInt16 type;
        UInt16 addressclass;

        //https://www2.cs.duke.edu/courses/fall16/compsci356/DNS/DNS-primer.pdf
        public DNSQuestion(string name)
        {
            this.name = name;
            type = 1;// TYPE A query
            addressclass = 1;// Class IN Query
        }

        public DNSQuestion(byte[] ba, ref int ind)
        {
            name = "";
            var nb = ba[ind++];
            while (nb != 0)
            {
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
        }

        public byte[] toByteArray()
        {
            var subnames = name.Split('.');
            int byteArrayLength = name.Length + 2 + 2 * (sizeof(UInt16));

            byte[] ret = new byte[byteArrayLength];
            int ind = 0;
            for (int i = 0; i < subnames.Length; i++)
            {
                ret[ind++] = (byte)subnames[i].Length;
                for (int j = 0; j < subnames[i].Length; j++)
                {
                    ret[ind++] = (byte)subnames[i][j];
                }
            }
            ret[ind++] = 0;

            //type
            ret[ind++] = 0;
            ret[ind++] = 1;
            //addressclass
            ret[ind++] = 0;
            ret[ind++] = 1;
            return ret;
        }

        public void print()
        {
            Console.WriteLine("name {0}", name);
            Console.WriteLine("type {0}", type);
            Console.WriteLine("addressclass {0}", addressclass);
        }
    }
}
