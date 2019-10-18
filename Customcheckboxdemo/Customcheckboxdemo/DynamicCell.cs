using Foundation;
using SaturdayMP.XPlugins.iOS;
using System;
using System.Linq;
using UIKit;

namespace Customcheckboxdemo
{
    public partial class DynamicCell : UITableViewCell
    {
        public BEMCheckBox checkbox;
        public UILabel label;
        public DynamicCell(IntPtr handle) : base(handle)
        {
        }

        public void UpdateCell()
        {

             label = new UILabel();
            label.TextColor = UIColor.Black;
            label.Lines = 0;
            label.Text = "thankfullness is the begining of happiness";
            label.Layer.CornerRadius = 15.0f;

            checkbox =new BEMCheckBox();
            checkbox.HeightAnchor.ConstraintEqualTo(25).Active = true;
            checkbox.WidthAnchor.ConstraintEqualTo(25).Active = true;


            GenerateCheckBox();


            var button = new UIButton();
            button.BackgroundColor = UIColor.Blue;
            button.Layer.CornerRadius = 15.0f;

            CurrentStack.AddArrangedSubview(checkbox);
            CurrentStack.AddArrangedSubview(label);
           

            checkbox.LeadingAnchor.ConstraintEqualTo(label.TrailingAnchor, 10).Active = true;
        
            button.HeightAnchor.ConstraintEqualTo(60).Active = true;
            button.WidthAnchor.ConstraintEqualTo(150).Active = true;
            button.Center = CurrentStack.Center;
            
            CurrentStack.AddArrangedSubview(button);
        }
        void GenerateCheckBox()
        {

            checkbox.DidTapCheckBox +=  (sender, e) =>
            {

                label.Text = "Be the Sunshine!";
            };




        }
    }
}