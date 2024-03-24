using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transport.Client
{
    public interface IClient
    {
        Task SendAsync(Memory<byte> buffer);

        Task<int> ReceiveAsync(Memory<byte> buffer);
    }
}
