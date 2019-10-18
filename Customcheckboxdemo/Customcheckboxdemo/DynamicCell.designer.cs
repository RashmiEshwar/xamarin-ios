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

namespace Customcheckboxdemo
{
    [Register ("DynamicCell")]
    partial class DynamicCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIStackView CurrentStack { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CurrentStack != null) {
                CurrentStack.Dispose ();
                CurrentStack = null;
            }
        }
    }
}