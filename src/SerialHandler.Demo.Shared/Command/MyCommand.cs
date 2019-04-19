using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialHandler.Demo.Command
{
    /// <summary>
    /// demo command.
    /// Head + TransType + Key + DataLength + Data + Tail
    /// </summary>
    public class MyCommand
    {
        /// <summary>
        /// Head of command.
        /// </summary>
        public const byte HEAD = 0xaa;
        /// <summary>
        /// Tail of comand.
        /// </summary>
        public const byte TAIL = 0xde;
        /// <summary>
        /// The length of a min command.
        /// </summary>
        public const int MIN_COMMAND_LENGTH = 5;

        public MyCommand(MyCommandTransKey transType, MyCommandKey cmdKey, byte[] data)
        : this((byte)transType, (byte)cmdKey, data)
        {

        }


        private MyCommand(byte transType, byte cmdkey, byte[] data)
        {
            this.TransType = transType;
            this.Key = cmdkey;
            this.Data = data;
        }

        /// <summary>
        /// head
        /// </summary>
        public byte Head { get; private set; } = HEAD;

        /// <summary>
        /// tail 
        /// </summary>
        public byte Tail { get; private set; } = TAIL;

        public byte TransType { get; private set; }

        /// <summary>
        /// key of command.
        /// </summary>
        public byte Key { get; private set; }

        /// <summary>
        /// The length of the data in command.
        /// </summary>
        public byte DataLength { get { return (byte)(Data == null ? 0 : Data.Length); } }

        /// <summary>
        /// The data of command.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Get the full-bytes of command.
        /// </summary>
        /// <returns></returns>
        public byte[] GetCommandBytes()
        {
            return BytesHelper.Concat(new byte[] { Head, TransType, Key, DataLength }, Data, new[] { Tail });
        }
    }
}
