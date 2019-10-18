using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Custompickerdemo
{
    public class TableViewSource :UITableViewSource
    {
        List<string> tabledata;
        string cellIdentifier = "DisplayCellIdentifier";
        //UITableViewCell cell;
        //UILabel textlabel;
        UIViewController ViewController;
        private List<string> itemData;
       public static  string value;
        public AppDelegate AppD = UIApplication.SharedApplication.Delegate as AppDelegate;

        public TableViewSource()
        {

        }


        //public TableViewSource(ViewController viewController)
        //{

        //    ViewController = viewController;
        //}

        public TableViewSource(List<string> items)
        {
            tabledata = items;
         

        }

       

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tabledata.Count;
        }

        //[Export("tableView:viewForFooterInSection:")]
        //public static UIView GetViewForFooter(TableViewSource instance, UITableView tableView, nint section)
        //{
        //    UIView footerView = new UIView(new CGRect(0, 0, 0, 0));
        //    return footerView;
        //}
        //[Export("tableView:heightForFooterInSection:")]
        //public static nfloat GetHeightForFooter(TableViewSource instance, UITableView tableView, nint section)
        //{
        //    return 1;
        //}
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            //WHAT TO DO HERE
        value = tabledata[indexPath.Row];
            //value = tabledata[indexPath.Row];

            var SetUpLabelViewController = UIStoryboard.FromName("Main", null).InstantiateViewController("ViewController") as ViewController;
            AppD.navctrl.PopViewController(true);
        }
      
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {

            TableCell cell = null;
            try
            {
                cell = (TableCell)tableView.DequeueReusableCell(cellIdentifier);
                cell.TextLabel.Text = tabledata[indexPath.Row];
                cell.BackgroundColor = UIColor.FromRGB(244, 244, 244);
                cell.UpdateCell();
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return cell;

        }

     
           

          
          


        }
    }



