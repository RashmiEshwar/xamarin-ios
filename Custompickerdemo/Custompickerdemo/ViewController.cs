using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace Custompickerdemo
{
    public partial class ViewController : UIViewController
    {
       

        UIStackView PSStackView;
    
   
        public AppDelegate AppD = UIApplication.SharedApplication.Delegate as AppDelegate;

        //public List<string> items { get; private set; }

        protected ViewController(IntPtr handle) : base(handle)
        {
            //TableViewSource table = new TableViewSource();

         
            // Note: this .ctor should not contain any initialization logic.
        }
      

       

        public ViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AppD.navctrl = this.NavigationController;


          
           

            tblTableSource.RowHeight = UITableView.AutomaticDimension;
            tblTableSource.EstimatedRowHeight = 200f;

           
        }
       


        public override void ViewWillAppear(bool animated)
        {

            base.ViewWillAppear(animated);


            tblTableSource.Source = new CustomViewSource(this);
           

            tblTableSource.ReloadData();
            //if (TableViewSource.value != null )
            //{
            //    PickerViewCell.textlabel.Text= TableViewSource.value;
            //}

        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }

}












