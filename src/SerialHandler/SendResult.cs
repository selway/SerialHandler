using System;
using System.Collections.Generic;
using System.Text;

namespace SerialHandler
{
    /// <summary>
    /// The reslut state of send command to serial port.
    /// </summary>
    public enum SendResult
    {
        /// <summary>
        /// Failure.
        /// </summary>
        Success,
        /// <summary>
        /// No command response.
        /// </summary>
        NoResponse,
        /// <summary>
        /// Serial handler is busing.This means that serial handler is sending the last command.
        /// </summary>
        Busing
    }
}
