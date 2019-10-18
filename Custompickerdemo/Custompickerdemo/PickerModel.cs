using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Custompickerdemo
{
    public partial class PickerModel : UIPickerViewModel
    {
        public EventHandler ValueChanged;
        public string SelectedValue;
        public List<string> itemData;
     
        public PickerModel(List<string> itemData)
        {
            this.itemData = itemData;
        }

        public PickerModel (IntPtr handle) : base (handle)
        {
        }

        public PickerModel()
        {
        }

        public override  nfloat GetRowHeight(UIPickerView pickerView, nint component)
        {

         
            return 80;

        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return itemData.Count;
        }
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return itemData[(int)row];
        }
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            var Data = itemData[(int)row];
            SelectedValue = Data;
            ValueChanged?.Invoke(null, null);
        }

        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            var label = new UILabel(new CGRect(0, 0, 400, 44));
            label.LineBreakMode = UILineBreakMode.WordWrap;
            label.Lines = 0;

            label.Text = itemData[(int)row];

            //var picker = new PickerModel(itemData);
            ////label.Text = picker;

            //pickerView.Model = picker;
            //picker.ValueChanged += (sender, e) =>
            //{
            //    if (picker.SelectedValue != null)
            //    {
            //        PickerViewCell.textlabel.Text = picker.SelectedValue;
            //    }
            //};

            label.SizeToFit();
            return label;
        }
    }
}