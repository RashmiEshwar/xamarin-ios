using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace searchbar
{
    public class TableSource : UITableViewSource
    {
         List<string> tabledata;
        public TableSource()
        {
        }
        public string cellIdentifier = "TableCell";
       

        public TableSource(List<string> items)
        {
            tabledata = items;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);


            var cellStyle = UITableViewCellStyle.Default;

            // if there are no cells to reuse, create a new one  
            if (cell == null)
            {
                cell = new UITableViewCell(cellStyle, cellIdentifier);
            }



          //  cell.TextLabel.Text = searchItems[indexPath.Row].Title;
          

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return tabledata.Count;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }


        public void PerformSearch(string searchText)
        {
            searchText = searchText.ToLower();
            //this.searchItems = tabledata.Where(x => x.Title.ToLower().Contains(searchText)).ToList();
        }
    }

}