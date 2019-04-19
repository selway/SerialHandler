using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialHandler.Demo.Command
{
    /// <summary>
    /// Key of command transfic type.
    /// </summary>
    public enum MyCommandTransKey : byte
    {
        /// <summary>
        /// Key of the requset command.
        /// </summary>
        Request = 0x22,
        /// <summary>
        /// Key of the Response command.
        /// </summary>
        Response = 0x55,

    }

    /// <summary>
    /// Key of command type.
    /// </summary>
    public enum MyCommandKey : byte
    {
        /// <summary>
        /// Key of the query device state command.
        /// </summary>
        QueryState = 0x21,

        /// <summary>
        /// Key of hart reporter.
        /// </summary>
        HartReport = 0x19
    }
}
