using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;

namespace Custompickerdemo
{
    public partial class PickerViewCell : UITableViewCell
    {
        public UITextField textfield;
        //public static UILabel textlabel;
        //UIStackView RStaticView;
        public UIImageView imageView;
        private AppDelegate AppD = UIApplication.SharedApplication.Delegate as AppDelegate;
        UIPickerView pickerView;
        UITapGestureRecognizer dropdowngesture;

        public PickerViewCell()
        {
        }

        //public PickerViewCell()
        //{
        //}

        public PickerViewCell (IntPtr handle) : base (handle)
        {
        }

 
        List<string> itemData = new List<string>()
    {
        "The Himalayan region is dotted with hundreds of lakes. Most of the larger lakes are on the northern side of the main range. The most famous is the sacred freshwater Lake Manasarovar, near to Mount Kailas with an area of 420 km2 (160 sq mi) and an altitude of 4,590 m (15,060 ft). It drains into the nearby Lake Rakshastal with an area of 250 km2 (97 sq mi) and slightly lower at 4,575 m (15,010 ft). Pangong Tso, which is spread across the border between India and China, at far western end of Tibet, and Yamdrok Tso, located in south central Tibet, are among the largest with surface areas of 700 km2 (270 sq mi), and 638 km2 (246 sq mi), respectively. Lake Puma Yumco is one of the highest of the larger lakes at an elevation of 5,030 m (16,500 ft).Some of the lakes present a danger of a glacial lake outburst flood. The Tsho Rolpa glacier lake in the Rowaling Valley, in the Dolakha District of Nepal, is rated as the most dangerous. The lake, which is located at an altitude of 4,580 m (15,030 ft) has grown considerably over the last 50 years due to glacial melting.[25][26] The mountain lakes are known to geographers as tarns if they are caused by glacial activity. Tarns are found mostly in the upper reaches of the Himalaya, above 5,500 m (18,000 ft).[27] Hot off the press, another PR just got merged into the Xamarin.Forms repository. This time, adding a new feature on a Label, the ability to specify MaxLines. This will be available in a near-future Xamarin.Forms version on iOS, Mac OS, Android, and UWP.\n\n You can see how the long text is now spread over two lines. Any longer text will still be in the property, but not shown on the screen. When used in conjunction with LineBreakMode you can now create a Label that is neatly cut off at the end with some ellipsis.Wherever possible you can also set it to automatic to let the Label grow as your text needs. For instance, on iOS, Mac OS, and UWP you can do this by setting the MaxLines property to 0. The Label will now just grow as your text grows That is all there is to it! Have a look at some other features I have implemented into Xamarin.Forms framework and of course, consider contributing yourself, it is pretty awesome!",
        "South of the main range, the lakes are smaller. Tilicho Lake in Nepal in the Annapurna massif is one of the highest lakes in the world. Other notable lakes include Rara Lake in western Nepal, She-Phoksundo Lake in the Shey Phoksundo National Park of Nepal, Gurudongmar Lake, in North Sikkim, Gokyo Lakes in Solukhumbu district of Nepal and Lake Tsongmo, near the Indo-China border in Sikkim.",
        "Windows Phone",
        "Xamarin-IOS",
        "Xamarin-Form",
        "Xamarin_Android"
    };

       
        public void UpdateCell()
        {
            PSStackView.Subviews.All(view => { view.RemoveFromSuperview(); return true; });

            //textlabel = new UILabel();
            textfield = new UITextField();
            //textlabel.Lines = 0;

            //textlabel.LineBreakMode = UILineBreakMode.WordWrap;
            textfield.TextColor = UIColor.Black;
            textfield.Font = UIFont.SystemFontOfSize(17);

            PSStackView.AddArrangedSubview(textfield);

            //textlabel.Text = .value;
            textfield.SizeToFit();


            imageView = new UIImageView();
            imageView.Image = new UIImage("DropDown");



            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            PSStackView.AddArrangedSubview(imageView);
            
            imageView.HeightAnchor.ConstraintEqualTo(30).Active = true;
            imageView.WidthAnchor.ConstraintEqualTo(30).Active = true;
            //imageView.CenterYAnchor.ConstraintEqualTo(textlabel.CenterYAnchor, 0).Active = true;

            PSView.Layer.CornerRadius = 10;
            PSView.Layer.BorderColor = new CGColor(0, 0, 0);
            PSView.Layer.BorderWidth = 3;

            dropdowngesture = new UITapGestureRecognizer(Dropdownclicked);
            dropdowngesture.NumberOfTapsRequired = 1;
            dropdowngesture.NumberOfTouchesRequired = 1;
            PSStackView.AddGestureRecognizer(dropdowngesture);
            
           
          
        }
        private void Dropdownclicked()
        {
       //var actionSheetAlert = UIAlertController.Create("Select"," ", UIAlertControllerStyle.Alert);
             pickerView = new UIPickerView();
            textfield.InputView = pickerView;
       
            //pickerView.BackgroundColor = UIColor.Gray;
            //pickerView.ShowSelectionIndicator = true;

            var picker = new PickerModel(itemData);
            //label.Text = picker;
            //var pickerView = new UIPickerView(new CGRect(UIScreen.MainScreen.Bounds.X - UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 730, UIScreen.MainScreen.Bounds.Width, 680));
            // actionSheetAlert.View.AddSubview(pickerView);
            pickerView.Model = picker;
            picker.ValueChanged += (sender, e) =>
            {
                if (picker.SelectedValue != null)
                {
                  textfield.Text = picker.SelectedValue;
                }
            };

             



            //actionSheetAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, alert => Console.WriteLine("Okay was clicked")));
            //actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, alert => Console.WriteLine("Cancel was clicked")));

            //Present Alert
            //AppD.navctrl.PresentViewController(pickerView, true, null);

        
      

    

                //var DropDownViewController = UIStoryboard.FromName("Main", null).InstantiateViewController("TextViewController") as TextViewController;

                //AppD.navctrl.PushViewController(DropDownViewController, true);


            }
        }

    }
