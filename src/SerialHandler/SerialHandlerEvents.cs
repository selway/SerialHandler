using System;
using System.Collections.Generic;
using System.Text;

namespace SerialHandler
{

    /// <summary>
    /// Delegate for serial port actively report data event handler.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    public delegate void DataReportEventHandler(object sender, DataReportEventArgs eventArgs);

    /// <summary>
    /// Delegate for serial port read error event handler.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    public delegate void ReadErrorEventHandler(object sender, ReadErrorEventArgs eventArgs);

    /// <summary>
    /// Delegate for serial port read error event handler.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    public delegate void DataHandleErrorEventHandler(object sender, DataHandleErrorEventArgs eventArgs);


    /// <summary>
    /// Arguments for data report event from serial handler.
    /// </summary>
    public class DataReportEventArgs : EventArgs
    {
        private byte[] data;
        /// <summary>
        /// Initializes a new instance of the <see cref="DataReportEventArgs"/> class.
        /// </summary>
        /// <param name="data">data of report</param>
        public DataReportEventArgs(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// data from serial port.
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }
    }

    /// <summary>
    /// Arguments for read error event from serial handler.
    /// </summary>
    public class ReadErrorEventArgs : EventArgs
    {
        private string description;
        private Exception exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadErrorEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="description">Error description.</param>
        /// 
        public ReadErrorEventArgs(string description, Exception exception)
        {
            this.description = description;
            this.exception = exception;
        }

        /// <summary>
        /// Read error description.
        /// </summary>
        /// 
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        /// Read exception.
        /// </summary>
        public Exception Exception
        {
            get { return exception; }
        }
    }

    /// <summary>
    /// Arguments for data handle error event from serial handler.
    /// </summary>
    public class DataHandleErrorEventArgs : EventArgs
    {
        private string description;
        private Exception exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataHandleErrorEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="description">Error description.</param>
        /// 
        public DataHandleErrorEventArgs(string description, Exception exception)
        {
            this.description = description;
            this.exception = exception;
        }

        /// <summary>
        /// Data handle error description.
        /// </summary>
        /// 
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        /// Data handle exception.
        /// </summary>
        public Exception Exception
        {
            get { return exception; }
        }
    }

}
