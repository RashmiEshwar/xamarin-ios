// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Controller_SRDO
{
    [Register ("ResponseViewController")]
    partial class ResponseViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ResponseText { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ResponseText != null) {
                ResponseText.Dispose ();
                ResponseText = null;
            }
        }
    }
}