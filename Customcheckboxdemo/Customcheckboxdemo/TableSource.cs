using System;
using Foundation;
using UIKit;

namespace Customcheckboxdemo
{
    public class TableSource:UITableViewSource
    {
        string cellIdentifier = "CustomIdentifier";
        public ViewController viewController;

        public TableSource()
        {
        }

        public TableSource(ViewController viewController)
        {
            this.viewController = viewController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {


            DynamicCell cell = null;
            try
            {
                cell = (DynamicCell)tableView.DequeueReusableCell(cellIdentifier);
                cell.UpdateCell();
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return 1;
        }

       
    }
}
