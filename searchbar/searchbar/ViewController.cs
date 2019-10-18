using System;
using CoreGraphics;
using System.Collections.Generic;


using UIKit;


namespace searchbar
{
    public partial class ViewController : UIViewController
    {
        UITableView table;
        TableSource tableSource;
      
       
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            UISearchBar searchBar = new UISearchBar()
            {
                Placeholder = "Enter your search Item",
                Prompt = "Search Entered here",
                ShowsScopeBar = true,
                ScopeButtonTitles = new string[] { "Boston", "London", "SF" },
            };
            NavigationItem.TitleView = searchBar;
            searchBar.SizeToFit();
  
            searchBar.AutocorrectionType = UITextAutocorrectionType.No;
            searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchBar.TextChanged += (sender, e) =>
            {
                //this is the method that is called when the user searches  
                searchTable();
            };
            Title = "SearchBarWithTableView Sample";
            table = new UITableView(new CGRect(0, 20, View.Bounds.Width, View.Bounds.Height - 20));
            List<string> itemData = new List<string>()
    {
        "Android","ios"
       
    };
            TableListView.Source = new TableSource(itemData);
        }


        // Perform any additional setup after loading the view, typically from a nib.
    

        private void searchTable()
        {
            //tableSource.PerformSearch(searchTable);
            //table.ReloadData();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
