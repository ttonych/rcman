﻿using RemoteConnectionManager.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace RemoteConnectionManager.Rdp
{
    public class RdpConnection : IConnection
    {
        public RdpConnection(ConnectionSettings connectionSettings)
        {
            ConnectionSettings = connectionSettings;
        }

        public bool IsConnected { get; private set; }
        public ConnectionSettings ConnectionSettings { get; }
        public FrameworkElement UI { get; private set; }

        // TODO: Implement!
        public event EventHandler Terminated;

        private RdpHost _hostRdp;
        private Grid _hostGrid;

        public void Connect()
        {
            if (!IsConnected)
            {
                _hostRdp = new RdpHost();
                _hostRdp.AxMsRdpClient.Server = ConnectionSettings.Server;

                var credentials = ConnectionSettings.Credentials;
                if (credentials != null)
                {
                    if (!string.IsNullOrEmpty(credentials.Domain))
                    {
                        _hostRdp.AxMsRdpClient.Domain = credentials.Domain;
                    }
                    if (!string.IsNullOrEmpty(credentials.Username))
                    {
                        _hostRdp.AxMsRdpClient.UserName = credentials.Username;
                    }
                    if (!string.IsNullOrEmpty(credentials.Password))
                    {
                        _hostRdp.AxMsRdpClient.AdvancedSettings9.ClearTextPassword = credentials.Password;
                    }
                }

                _hostRdp.AxMsRdpClient.AdvancedSettings9.SmartSizing = true;
                _hostRdp.AxMsRdpClient.Connect();

                _hostGrid = new Grid();
                _hostGrid.Children.Add(new WindowsFormsHost { Child = _hostRdp });
                _hostGrid.SizeChanged += Host_SizeChanged;

                UI = _hostGrid;

                IsConnected = true;
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                if (_hostGrid != null)
                {
                    _hostGrid.SizeChanged -= Host_SizeChanged;
                    _hostGrid.Dispatcher.Invoke(() => _hostGrid.Children.Clear());
                    _hostGrid = null;
                }

                if (_hostRdp != null)
                {
                    if (_hostRdp.AxMsRdpClient.Connected == 1)
                    {
                        _hostRdp.AxMsRdpClient.Disconnect();
                    }
                    _hostRdp.Invoke((MethodInvoker)delegate { _hostRdp.Dispose(); });
                    _hostRdp = null;
                }

                UI = null;
                IsConnected = false;
            }
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _hostRdp.Width = (int)e.NewSize.Width;
            _hostRdp.Height = (int)e.NewSize.Height;
            _hostRdp.AxMsRdpClient.Width = (int)e.NewSize.Width;
            _hostRdp.AxMsRdpClient.Height = (int)e.NewSize.Height;
        }
    }
}
