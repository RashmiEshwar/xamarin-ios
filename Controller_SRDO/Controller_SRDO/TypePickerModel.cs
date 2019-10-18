using System;
using System.Collections.Generic;
using UIKit;

namespace Controller_SRDO
{
    public partial class TypePickerModel : UIPickerViewModel     {         List<string> Type;         public EventHandler ValueChanged;         public string SelectedValue;

     //  public Func<object, object, object> Controllerselected { get; internal set; }

        public TypePickerModel(IntPtr handle) : base(handle)         {         }         public TypePickerModel(List<string> Type)         {             this.Type = Type;         }

        public TypePickerModel()
        {
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)         {             return Type.Count;         }          public override nint GetComponentCount(UIPickerView pickerView)         {             return 1;         }           public override string GetTitle(UIPickerView pickerView, nint row, nint component)         {             return Type[(int)row];         }          public override void Selected(UIPickerView pickerView, nint row, nint component)         {             var TypeSelected = Type[(int)row];             SelectedValue = TypeSelected;             ValueChanged?.Invoke(null, null);          }

             } } 