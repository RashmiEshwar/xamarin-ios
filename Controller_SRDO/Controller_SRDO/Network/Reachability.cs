// Copyright 2019 Otis Elevator Co. All Rights Reserved.
//
// 	The Source Code and the information it contains is the property of the
// Otis Elevator Company ("Otis"). The information it contains is highly
// confidential and is a trade secret of Otis. Access to this work is limited
// to those selected employees of Otis having a specific need to use it on
// behalf of Otis. It shall not be used by any other person for any purpose;
// it may not be reproduced, distributed, or disclosed by or to anyone,
// including any employee not having a specific need to use it without the
// express written permission of an officer of Otis.
//
// 	Any unauthorized reproduction, disclosure, or distribution of copies by
// any person of any portion of this work may be a violation of the copyright
// law of the United States of America and other countries, and could result
// in the awarding of statutory damages of up to $150,000 dollars (17 USC 504)
// for infringement, and may result in further civil and criminal penalties.
using System.Net;
using SystemConfiguration;
using CoreFoundation;
using Foundation;

namespace ServiceTool.Network
{
    public enum NetworkStatus
    {
        NotReachable,
        ReachableViaWWAN,
        ReachableViaWiFi
    }

    /// <summary>
    /// Basic demonstration of how to use the SystemConfiguration Reachablity APIs.
    /// </summary>
    public class Reachability : NSObject
    {
        ~Reachability()
        {
            StopNotifier();
            if (NetworkReachability != null)
            {
                NetworkReachability.Dispose();
            }
        }

        public static string ReachabilityChangedNotification { get; } = "kNetworkReachabilityChangedNotification";

        public NetworkReachability NetworkReachability { get; private set; }

        public static Reachability ReachabilityWithHostName(string hostName)
        {
            var reachability = new NetworkReachability(hostName);
            return new Reachability { NetworkReachability = reachability };
        }

        public static Reachability ReachabilityWithAddress(IPAddress hostAddress)
        {
            var reachability = new NetworkReachability(hostAddress);
            return new Reachability { NetworkReachability = reachability };
        }

        public static Reachability ReachabilityForInternetConnection()
        {
            var reachability = new NetworkReachability(new IPAddress(0));
            return new Reachability { NetworkReachability = reachability };
        }

        private void HandleNotification(NetworkReachabilityFlags flags)
        {
            // Post a notification to notify the client that the network reachability changed.
            NSNotificationCenter.DefaultCenter.PostNotificationName(ReachabilityChangedNotification, this);
        }

        #region Start and stop notifier

        public bool StartNotifier()
        {
            var result = false;

            var status = NetworkReachability.SetNotification(HandleNotification);
            if (status == StatusCode.OK)
            {
                result = NetworkReachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }

            return result;
        }

        private void StopNotifier()
        {
            if (NetworkReachability != null)
            {
                NetworkReachability.Unschedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
            }
        }

        #endregion

        #region Network Flag Handling

        public NetworkStatus NetworkStatusForFlags(NetworkReachabilityFlags flags)
        {
            if ((flags & NetworkReachabilityFlags.Reachable) == 0)
            {
                // The target host is not reachable.
                return NetworkStatus.NotReachable;
            }

            NetworkStatus result = NetworkStatus.NotReachable;
            if ((flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 ||
                (flags & NetworkReachabilityFlags.IsDirect) != 0)
            {
                result = NetworkStatus.ReachableViaWiFi;
            }

            return result;
        }

        public bool ConnectionRequired()
        {
            if (NetworkReachability.TryGetFlags(out NetworkReachabilityFlags flags))
            {
                return (flags & NetworkReachabilityFlags.ConnectionRequired) != 0;
            }

            return false;
        }

        public NetworkStatus CurrentReachabilityStatus()
        {
            var returnValue = NetworkStatus.NotReachable;
            if (NetworkReachability.TryGetFlags(out NetworkReachabilityFlags flags))
            {
                returnValue = this.NetworkStatusForFlags(flags);
            }

            return returnValue;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (NetworkReachability != null)
                {
                    NetworkReachability.Dispose();
                }
            }
        }
    }
}