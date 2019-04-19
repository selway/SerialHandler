using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialHandler.Demo.Command
{
    /// <summary>
    /// The encoder of my command.
    /// </summary>
    public class MyCommandEncoder
    {
        /// <summary>
        /// Encode command of query state
        /// </summary>
        /// <returns></returns>
        public MyCommand EncodeQueryState()
        {
            return new MyCommand(MyCommandTransKey.Request, MyCommandKey.QueryState, new byte[] { });
        }
    }
}
