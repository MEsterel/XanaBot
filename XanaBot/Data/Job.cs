using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanaBot.Data
{
    class Job
    {
        public ulong RoleId { get; set; }
        public List<string> AccessibleCommands { get; set; }
    }
}
