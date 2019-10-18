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
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
//using Google.Protobuf;
namespace ServiceTool.Network
{
    public class AsyncTcpClient : IDisposable
    {
        private TcpClient tcpClient;
        private Stream stream;

        private int minBufferSize = 8192;
        private int maxBufferSize = 15 * 1024 * 1024;
        private int bufferSize = 8192;
        private bool disposed = false;

        private int BufferSize
        {
            get
            {
                return this.bufferSize;
            }
            set
            {
                this.bufferSize = value;
                if (this.tcpClient != null)
                    this.tcpClient.ReceiveBufferSize = value;
            }
        }

        public bool IsReceiving { set; get; }
        public int MinBufferSize
        {
            get
            {
                return this.minBufferSize;
            }
            set
            {
                this.minBufferSize = value;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
            set
            {
                this.maxBufferSize = value;
            }
        }

        public int SendBufferSize
        {
            get
            {
                if (this.tcpClient != null)
                    return this.tcpClient.SendBufferSize;
                else
                    return 0;
            }
            set
            {
                if (this.tcpClient != null)
                    this.tcpClient.SendBufferSize = value;
            }
        }

        public event EventHandler<byte[]> OnDataReceived;
        public event EventHandler OnDisconnected;

        public bool IsConnected
        {
            get
            {
                return this.tcpClient != null && this.tcpClient.Connected;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="data">Data.</param>
        /// <param name="token">Token.</param>
        /// 
        public async Task SendAsync(byte[] data, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await this.stream.WriteAsync(data, 0, data.Length, token);
                await this.stream.FlushAsync(token);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException:" + ex.Message);
                throw new Exception(ex.Message);
            }
            catch (IOException ex)
            {
                var onDisconnected = this.OnDisconnected;
                if (ex.InnerException != null && ex.InnerException is ObjectDisposedException)
                {
                    Console.WriteLine("innocuous ssl stream error");
                    // for SSL streams
                }
                else
                {
                    onDisconnected?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Connects the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="host">Host.</param>
        /// <param name="port">Port.</param>
        /// <param name="ssl">If set to <c>true</c> ssl.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task ConnectAsync(string host, int port, bool ssl = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                //Connect async method
                await this.CloseAsync();
                cancellationToken.ThrowIfCancellationRequested();
                this.tcpClient = new TcpClient();
                cancellationToken.ThrowIfCancellationRequested();
                //   await this.tcpClient.ConnectAsync(host, port);
                await this.tcpClient.ConnectAsync(host, port);
                await this.CloseIfCanceled(cancellationToken);
                // get stream and do SSL handshake if applicable

                this.stream = this.tcpClient.GetStream();
                await this.CloseIfCanceled(cancellationToken);
            }
            catch (Exception)
            {
                this.CloseIfCanceled(cancellationToken).Wait();
                throw;
            }
        }

        /// <summary>
        /// Receive the specified token.
        /// </summary>
        /// <returns>The receive.</returns>
        /// <param name="token">Token.</param>
        public async Task Receive(CancellationToken token = default(CancellationToken))
        {
            try
            {
                if (!this.IsConnected || this.IsReceiving)
                    throw new InvalidOperationException();
                this.IsReceiving = true;
                byte[] buffer = new byte[bufferSize];
                while (this.IsConnected)
                {
                    token.ThrowIfCancellationRequested();
                    int bytesRead = await this.stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead > 0)
                    {
                        if (bytesRead == buffer.Length)
                            this.BufferSize = Math.Min(this.BufferSize * 10, this.maxBufferSize);
                        else
                        {
                            do
                            {
                                int reducedBufferSize = Math.Max(this.BufferSize / 10, this.minBufferSize);
                                if (bytesRead < reducedBufferSize)
                                    this.BufferSize = reducedBufferSize;

                            }
                            while (bytesRead > this.minBufferSize);
                        }
                        var onDataReceived = this.OnDataReceived;
                        if (onDataReceived != null)
                        {
                            byte[] data = new byte[bytesRead];
                            Array.Copy(buffer, data, bytesRead);
                            onDataReceived(this, data);
                        }
                    }
                    buffer = new byte[bufferSize];
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("ODE Exception in receive");
            }
            catch (IOException ex)
            {
                var evt = this.OnDisconnected;
                if (ex.InnerException != null && ex.InnerException is ObjectDisposedException)
                {
                    Console.WriteLine("innocuous ssl stream error");
                    // for SSL streams
                }
                evt?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                this.IsReceiving = false;
            }
        }

        /// <summary>
        /// Closes the async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task CloseAsync()
        {
            await Task.Yield();
            this.Close();
        }

        /// <summary>
        /// Close this instance.
        /// </summary>
        private void Close()
        {
            if (this.tcpClient != null)
            {
                if (this.tcpClient.Client.Connected)
                {
                    this.tcpClient.GetStream().Close();
                }
                this.tcpClient.Close();
                this.tcpClient.Dispose();
                this.tcpClient = null;
            }

            if (this.stream != null)
            {
                this.stream.Close();
                this.stream.Dispose();
                this.stream = null;
            }
        }

        /// <summary>
        /// Closes if canceled.
        /// </summary>
        /// <returns>The if canceled.</returns>
        /// <param name="token">Token.</param>
        /// <param name="onClosed">On closed.</param>
        private async Task CloseIfCanceled(CancellationToken token, Action onClosed = null)
        {
            if (token.IsCancellationRequested)
            {
                await this.CloseAsync();
                onClosed?.Invoke();
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <returns>The dispose.</returns>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    this.Close();
                }
            }

            disposed = true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}