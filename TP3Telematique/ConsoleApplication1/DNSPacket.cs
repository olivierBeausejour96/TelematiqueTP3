using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP3
{
    //http://www.firewall.cx/networking-topics/protocols/domain-name-system-dns/160-protocols-dns-query.html
    public class DNSPacket
    {
        UInt16 TransactionID;
        UInt16 Flags;
        UInt16 Questions;
        UInt16 AnswerRRs;
        UInt16 AuthorityRRs;
        UInt16 AdditionalRRs;
        List<DNSQuestion> Queries;
        List<DNSAnswer> Answers;

        public DNSPacket(List<DNSQuestion> queries)
        {
            TransactionID = 0;
            Flags = 0x0100;
            Flags |= 1 << 7;
            Queries = queries;
            Questions = (UInt16)Queries.Count;
        }

        public DNSPacket(byte[] ba)
        {
            int ind = 0;

            TransactionID = 0;
            TransactionID |= (UInt16)(ba[ind++] << 4);
            TransactionID |= (UInt16)(ba[ind++]);

            Flags = 0;
            Flags |= (UInt16)(ba[ind++] << 4);
            Flags |= (UInt16)(ba[ind++]);

            Questions = 0;
            Questions |= (UInt16)(ba[ind++] << 4);
            Questions |= (UInt16)(ba[ind++]);

            AnswerRRs = 0;
            AnswerRRs |= (UInt16)(ba[ind++] << 4);
            AnswerRRs |= (UInt16)(ba[ind++]);

            AuthorityRRs = 0;
            AuthorityRRs |= (UInt16)(ba[ind++] << 4);
            AuthorityRRs |= (UInt16)(ba[ind++]);

            AdditionalRRs = 0;
            AdditionalRRs |= (UInt16)(ba[ind++] << 4);
            AdditionalRRs |= (UInt16)(ba[ind++]);

            Queries = new List<DNSQuestion>();
            for (int i = 0; i < Questions; i++)
            {
                Queries.Add(new DNSQuestion(ba, ref ind));
            }

            Answers = new List<DNSAnswer>();
            for (int i = 0; i < AnswerRRs; i++)
            {
                Answers.Add(new DNSAnswer(ba, ref ind));
            }
        }

        public void SetQueryFlag(bool b)
        {
            if (b)
                Flags |= 0x8000;
            else
                Flags &= 0x7FFF;

        }

        public byte[] toByteArray()
        {
            int totalLength = 0;
            List<byte[]> byteQueries = new List<byte[]>();
            for (int i = 0; i < Questions; i++)
            {
                var tmp = Queries[i].toByteArray();
                totalLength += tmp.Length;
                byteQueries.Add(tmp);
            }
            totalLength += 6 * (sizeof(UInt16));
            int ind = 0;
            byte[] ret = new byte[totalLength];

            var ba = BitConverter.GetBytes(TransactionID);
            for (int i = 0; i < ba.Length; i++)
            {
                ret[ind++] = ba[i];
            }

            ba = BitConverter.GetBytes(Flags);
            for (int i = 0; i < ba.Length; i++)
            {
                ret[ind++] = ba[i];
            }

            ba = BitConverter.GetBytes(Questions);
            for (int i = 1; i >= 0; i--)
            {
                ret[ind++] = ba[i];
            }

            ba = BitConverter.GetBytes(AnswerRRs);
            for (int i = 0; i < ba.Length; i++)
            {
                ret[ind++] = ba[i];
            }

            ba = BitConverter.GetBytes(AuthorityRRs);
            for (int i = 0; i < ba.Length; i++)
            {
                ret[ind++] = ba[i];
            }

            ba = BitConverter.GetBytes(AdditionalRRs);
            for (int i = 0; i < ba.Length; i++)
            {
                ret[ind++] = ba[i];
            }

            for (int i = 0; i < byteQueries.Count; i++)
            {
                for (int j = 0; j < byteQueries[i].Length; j++)
                {
                    ret[ind++] = byteQueries[i][j];
                }
            }
            return ret;
        }
    }
}
