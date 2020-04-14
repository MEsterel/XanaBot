using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace XanaBot.Data
{
    [DataContract]
    public class XUser
    {
        [DataMember]
        public ulong Id { get; set; }

        [DataMember]
        public ulong JobId { get; set; }

        [DataMember]
        public double Money { get; set; }

        public XUser(ulong id)
        {
            Id = id;
            ResetDefault();
        }

        public void ResetDefault()
        {
            JobId = 0;

            Money = 0;
        }
    }
}
