using SerialHandler;
using SerialHandler.Demo.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SerialHandler.Demo
{
    /// <summary>
    /// Usage of <see cref="SerialPortHandler"/>
    /// </summary>
    public class MyCommandService
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="MyCommandService"/> class.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).<see cref="SerialPort"/></param>
        /// <param name="baudRate">The baud rate.<see cref="SerialPort"/></param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait serial port response.</param>
        public MyCommandService(string portName, int baudRate, int millisecondsTimeout = 1500)
        {
            this.Timeout = millisecondsTimeout;
            this.SpHandler = new SerialPortHandler(portName, baudRate, ReportPredicate, DequeueFunc);
            this.SpHandler.DataReport += SpHandler_DataReport;

        }

        #region 属性

        /// <summary>
        /// Instance of <see cref="SerialPortHandler"/>.
        /// </summary>
        protected SerialPortHandler SpHandler { get; set; }

        #region 抽象
        /// <summary>
        /// Command decoder.
        /// </summary>
        protected MyCommandDecoder Decoder { get; set; }

        /// <summary>
        /// Command encoder.
        /// </summary>
        protected MyCommandEncoder Encoder { get; set; }


        /// <summary>
        /// The number of milliseconds to wait serial port response.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// State of the serial port.
        /// </summary>
        public bool IsOpen
        {
            get { return this.SpHandler == null ? false : this.SpHandler.IsOpen; }
        }
        #endregion
        #endregion

        #region 事件
        /// <summary>
        /// Heart report event.
        /// </summary>
        public event Action HeartReport;

        /// <summary>
        /// Recive error event.
        /// </summary>
        public event Action<Exception> OnRecivedError;
        #endregion

        #region 方法
        private void SpHandler_DataReport(object sender, DataReportEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Data == null || eventArgs.Data.Length == 0)
                    return;
                if (this.Decoder.DecodeHartReport(eventArgs.Data))
                {
                    this.HeartReport?.BeginInvoke(null, null);
                }
            }
            catch (Exception ex)
            {
                OnRecivedError?.BeginInvoke(ex, null, null);
            }
        }

        /// <summary>
        /// Whether ther recevied bytes is a response.
        /// </summary>
        /// <param name="data">Received bytes</param>
        /// <returns></returns>
        protected bool ReportPredicate(byte[] data)
        {
            return this.Decoder.IsReportData(data);
        }

        /// <summary>
        /// Your methos of dequeuing a command from the received data queue method.
        /// </summary>
        /// <param name="quReceive"></param>
        /// <returns></returns>
        protected byte[] DequeueFunc(Queue<byte> quReceive)
        {
            if (quReceive == null || !quReceive.Contains((byte)MyCommand.HEAD) || !quReceive.Contains((byte)MyCommand.TAIL))
            {
                SpinWait.SpinUntil(() => quReceive.Contains((byte)MyCommand.HEAD) && quReceive.Contains((byte)MyCommand.HEAD), 100);
                return null;
            }
            byte head = quReceive.Dequeue();
            if (head != (byte)MyCommand.HEAD)
                return null;

            List<byte> data = new List<byte>();
            data.Add(head);
            while (quReceive.Count > 0)
            {
                byte bt = quReceive.Peek();
                if (bt == (byte)MyCommand.HEAD)
                    break;
                else
                {
                    data.Add(quReceive.Dequeue());
                    if (bt == (byte)MyCommand.HEAD)
                        break;
                }
            }
            return data.ToArray();
        }

        /// <summary>
        /// Open
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            return this.SpHandler.Start();
        }

        /// <summary>
        /// Query terminal state.
        /// </summary>
        /// <param name="isError">OUT: state</param>
        /// <returns></returns>
        public SendResult QueryState(out bool isError)
        {
            var result = SendResult.NoResponse;
            isError = false;
            if (!this.SpHandler.IsOpen)
                return result;
            var command = this.Encoder.EncodeQueryState();
            if (command == null)
                return result;
            byte[] ReciveData = new byte[0];
            result = this.SpHandler.SendCommand(command.GetCommandBytes(), ref ReciveData, Timeout);
            if (result != SendResult.Success)
                return result;

            if (!this.Decoder.DecodeQueryState(ReciveData, out isError))
                return SendResult.NoResponse;
            return SendResult.Success;
        }

        /// <summary>
        /// Close
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            return this.SpHandler.Stop();
        }

        #endregion
    }
}
