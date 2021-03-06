﻿using System;

using UIKit;

namespace Customcheckboxdemo
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {

            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CustomTableView.Source = new TableSource(this);
            CustomTableView.ReloadData();

                // Perform any additional setup after loading the view, typically from a nib.
            }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
