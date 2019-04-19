using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialHandler.Demo.Command
{
    /// <summary>
    /// The decoceer of command.
    /// </summary>
    public class MyCommandDecoder
    {
        /// <summary>
        /// Decode util 
        /// </summary>
        /// <param name="buff">Received bytes</param>
        /// <returns></returns>
        private bool Decode(byte[] buff)
        {
            if (buff == null || buff.Length < MyCommand.MIN_COMMAND_LENGTH)
                return false;
            if (buff[0] != MyCommand.HEAD || buff[buff.Length - 1] != MyCommand.TAIL)
                return false;
            if (buff[3] + MyCommand.MIN_COMMAND_LENGTH != buff.Length)
                return false;
            return true;
        }

        /// <summary>
        /// Whether ther recevied bytes is a response.
        /// </summary>
        /// <param name="buff">Received bytes</param>
        /// <returns></returns>
        public bool IsReportData(byte[] buff)
        {
            if (buff != null && buff.Length > MyCommand.MIN_COMMAND_LENGTH && buff[2] == (byte)MyCommandTransKey.Response
                && buff[4] == (byte)MyCommandKey.HartReport)
                return true;
            return false;
        }


        /// <summary>
        /// Decode query state command.
        /// </summary>
        /// <param name="buff">Received bytes</param>
        /// <param name="isError">OUT: state</param>
        /// <returns></returns>
        public bool DecodeQueryState(byte[] buff, out bool isError)
        {
            isError = false;
            if (!Decode(buff))
                return false;
            if (buff[1] != (byte)MyCommandKey.QueryState)
                return false;
            if (buff[4] == 1)
                isError = true;
            return true;
        }

        /// <summary>
        /// Decode heart reporter.
        /// </summary>
        /// <param name="buff">Received bytes</param>
        /// <returns></returns>
        public bool DecodeHartReport(byte[] buff)
        {
            if (!Decode(buff))
                return false;
            if (buff[1] != (byte)MyCommandKey.HartReport)
                return false;
            return true;
        }
    }
}
