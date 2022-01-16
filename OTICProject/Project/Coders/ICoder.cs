using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Coders
{
    public interface ICoder
    {
        (byte[] blob, byte[] info) Encode(byte[] file);

        byte[] Decode(byte[] file, byte[] info, int oldSize);
    }
}
