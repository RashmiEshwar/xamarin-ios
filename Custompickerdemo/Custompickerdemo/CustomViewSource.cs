using System;
using Foundation;
using UIKit;

namespace Custompickerdemo
{
    public class CustomViewSource : UITableViewSource
    {
        UIViewController controller;
        UILabel textlabel;
   
        string strIdentifier = "CustomCellIdentifier";

      



        public CustomViewSource(ViewController viewController)
        {

            controller = viewController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {


            PickerViewCell cell = null;
            try
            {
                cell = (PickerViewCell)tableView.DequeueReusableCell(strIdentifier);
                cell.BackgroundColor = UIColor.FromRGB(244, 244, 244);
                cell.UpdateCell();
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
               // Utility.ErrorLogger.InsertToDeviceDB("JobItemController GetCell", ex.Message, ex);
            }
            return cell;
            //PickerViewCell cell = tableView.DequeueReusableCell(strIdentifier) as PickerViewCell;
            //try
            //{


            //    if (cell == null)
            //    {
            //        cell = (PickerViewCell)new UITableViewCell(UITableViewCellStyle.Default, strIdentifier); }
            //    cell.UpdateCell();
            //    //cell.TextLabel.Text = item;
            //    cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //return cell;
        }


        
       

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return 1;
        }
       
    }

}