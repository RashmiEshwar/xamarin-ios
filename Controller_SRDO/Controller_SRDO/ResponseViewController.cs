using Foundation;
using System;
using UIKit;

namespace Controller_SRDO
{
    public partial class ResponseViewController : UIViewController
    {
        public string MyProperty { get; set; }

        public ResponseViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();


            ResponseText.Text = MyProperty;
        }
    }
}