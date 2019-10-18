// Copyright 2019 Otis Elevator Co. All Rights Reserved.
//
//  The Source Code and the information it contains is the property of the
// Otis Elevator Company ("Otis"). The information it contains is highly
// confidential and is a trade secret of Otis. Access to this work is limited
// to those selected employees of Otis having a specific need to use it on
// behalf of Otis. It shall not be used by any other person for any purpose;
// it may not be reproduced, distributed, or disclosed by or to anyone,
// including any employee not having a specific need to use it without the
// express written permission of an officer of Otis.
//
//  Any unauthorized reproduction, disclosure, or distribution of copies by
// any person of any portion of this work may be a violation of the copyright
// law of the United States of America and other countries, and could result
// in the awarding of statutory damages of up to $150,000 dollars (17 USC 504)
// for infringement, and may result in further civil and criminal penalties.
using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceTool.Network;

namespace ServiceTool.Legacy.Dongle
{
    public class Data
    {
        enum State { RAW, LOGON, PASSTHROUGH, NOSUPPORT }
        static AsyncTcpClient _client;
        public event EventHandler<byte[]> OnEscapeResponse;
        public event EventHandler<byte[]> OnPassThrough;
        public event EventHandler OnDisconnected;
        readonly CancellationTokenSource cts = new CancellationTokenSource();
        public PortStatus status;

        /// <summary>
        /// Connect this instance.
        /// </summary>
        /// <returns>The connect.</returns>
        internal async Task Connect()
        {
            try
            {
                Console.WriteLine("Data - Connect");
                _client = new AsyncTcpClient();
                _client.OnDataReceived += DataReceived;
                _client.OnDisconnected += Disconnected;
                status = new PortStatus();
                status.IsConnected = true;
                await _client.ConnectAsync(Constants.Address, Constants.DataPort);
                Task receiveTask = _client.Receive();
            }
            catch (TaskCanceledException ex)
            {
                if (!cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Data Task Cancel " + ex);
                }

            }
        }
        /// <summary>
        /// Send the specified str.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="str">String.</param>
        internal async Task Send(string str)
        {
            try
            {
                if (_client != null)
                {
                    byte[] bytesToSend = System.Text.Encoding.UTF8.GetBytes(str);
                    //Console.WriteLine("Data - Sending Data: " + str);
                    await _client.SendAsync(bytesToSend);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Send the specified bytes.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="bytes">Bytes.</param>
        internal async Task Send(byte[] bytes)
        {
            try
            {
                if (_client != null)
                {
                    status.OutgoingCount += 1;
                    string bytesToSend = ByteArrayToString(bytes);
                    //Console.WriteLine("Data - Sending Data: " + bytesToSend);
                    await _client.SendAsync(bytes);
                    Console.WriteLine("Out " + status.OutgoingCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Datas the received.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void DataReceived(object sender, byte[] e)
        {
            switch (PortSwitch.PortState)
            {
                case (int) State.RAW:
                    break;
                case (int) State.LOGON:
                    OnEscapeResponse?.Invoke(this, e);
                    break;
                case (int) State.PASSTHROUGH:
                    OnPassThrough?.Invoke(this, e);
                    break;
            }
        }
        /// <summary>
        /// Bytes the array to string.
        /// </summary>
        /// <returns>The array to string.</returns>
        /// <param name="ba">Ba.</param>
        public string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", " ");
        }
        /// <summary>
        /// Close this instance.
        /// </summary>
        /// <returns>The close.</returns>
        public async Task Close()
        {
            if (_client != null)
            {
                Console.WriteLine("Closing Data TCP client!");
                await _client.CloseAsync();
            }
        }
        /// <summary>
        /// Disconnected the specified sender and e.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void Disconnected(object sender, EventArgs e)
        {
            status.IsConnected = false;
            OnDisconnected?.Invoke(this, e);
            //Console.WriteLine("Data - Disconnected");
            //_client = new AsyncTcpClient();
            //_client.OnDataReceived += DataReceived;
            //_client.OnDisconnected += Disconnected;
            //await _client.ConnectAsync(Constants.Address, Constants.DataPort);
            //Task receiveTask = _client.Receive();
            //Console.WriteLine("Data - Connect");
        }
    }

    public class PortStatus
    {
        public bool IsConnected { get; set; }
        public int OutgoingCount { get; set; }
        public int IncomingCount { get; set; }
    }
}