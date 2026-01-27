using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.Fresher.Core.MISAAttribute
{
    public class MISAColumnName:Attribute
    {
        public string Name { get; set; }
        public MISAColumnName(string name)
        {
            Name = name;
        }
    }
}
