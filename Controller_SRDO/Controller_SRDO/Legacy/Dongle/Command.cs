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
    public class Command
    {
        static AsyncTcpClient _client;
        static bool FirstChevron;
        public event EventHandler InitPort;
        public event EventHandler DefaultBaud;
        public event EventHandler OnDisconnected;
        readonly CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// Connect this instance.
        /// </summary>
        /// <returns>The connect.</returns>
        internal async Task Connect()
        {
            try
            {
                Console.WriteLine("Command - Connect");
                _client = new AsyncTcpClient();
                _client.OnDataReceived += DataReceived;
                _client.OnDisconnected += Disconnected;
                await _client.ConnectAsync(Constants.Address, Constants.CommandPort);
                Task receiveTask = _client.Receive();
                FirstChevron = false;
            }
            catch (TaskCanceledException ex)
            {
                if (!cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("Command Task Cancel " + ex);
                }

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
            return hex.Replace("-", "");
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
                    byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(str);
                    //Console.WriteLine("Command - Sending Data: " + str + ByteArrayToString(bytesToSend));
                    Console.WriteLine("Command - Sending Data: " + str);
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
                    string bytesToSend = System.Text.Encoding.ASCII.GetString(bytes);
                    Console.WriteLine("Command - Sending Data: " + bytesToSend);
                    await _client.SendAsync(bytes);
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
            string result = System.Text.Encoding.ASCII.GetString(e);
            Console.WriteLine(result);
            if (result.Contains("9600"))
            {
                Console.WriteLine("Switching Port Speed...");
                InitPort?.Invoke(this, null);
            }

            if ((FirstChevron == false) && (result.Contains(">")))
            {
                FirstChevron = true;
                DefaultBaud?.Invoke(this, null);
            }

        }
        /// <summary>
        /// Close this instance.
        /// </summary>
        /// <returns>The close.</returns>
        public async Task Close()
        {
            if (_client != null)
            {
                Console.WriteLine("Closing Command TCP client!");
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
            Console.WriteLine("Command - Disconnected");
            OnDisconnected?.Invoke(this, e);
            //_client = new AsyncTcpClient();
            //_client.OnDataReceived += DataReceived;
            //_client.OnDisconnected += Disconnected;
            //await _client.ConnectAsync(Constants.Address, Constants.CommandPort);
            //Task receiveTask = _client.Receive();
            //FirstChevron = false;
            //Console.WriteLine("Command - Connect");
        }
    }
}