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

namespace Custompickerdemo
{
    [Register ("TextViewController")]
    partial class TextViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ListSource { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ListSource != null) {
                ListSource.Dispose ();
                ListSource = null;
            }
        }
    }
}