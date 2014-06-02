using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using FlyingHippo.AutoComplete.CustomPicker;
using System.Collections;
using Microsoft.SharePoint;

namespace FlyingHippo.AutoComplete.Fields
{
    public class AutoCompleteFieldControl : BaseFieldControl 
    {
        protected Label EmailPrefix;
        protected Label EmailValueForDisplay;
        private CustomPickerEditor pickerEditor;
        private AutoCompleteFieldType parentField;

        public AutoCompleteFieldControl(AutoCompleteFieldType parent)
        {
            this.parentField = parent;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.ControlMode != SPControlMode.Display)
            {
                pickerEditor = (CustomPickerEditor)this.TemplateContainer.FindControl("CustomPicker");

                if(pickerEditor == null)
                    pickerEditor = new CustomPickerEditor();

                pickerEditor.ID = "CustomPicker";
                pickerEditor.ValidatorEnabled = true;
                pickerEditor.SearchListGuid = parentField.SearchListGuid;
                pickerEditor.SearchColumnsGuid = parentField.SearchColumnGuids;
                pickerEditor.SearchDisplayName = parentField.DisplayColumn;
                pickerEditor.SearchKeyName = "ID";
                pickerEditor.MultiSelect = parentField.AllowMultiple;

                pickerEditor.CustomProperty = new CustomPickerContract
                {
                    AllowMultiple = parentField.AllowMultiple,
                    DisplayColumn = parentField.DisplayColumn,
                    KeyColumn = "ID",
                    LookupGuid = parentField.SearchListGuid,
                    SearchColumns = parentField.SearchColumnGuids
                }.ToString();

                //if (pickerEditor.MultiSelect)
                //    pickerEditor.MaximumEntities = 10;

                if (Value != null && !string.IsNullOrEmpty(Value.ToString()) && SPControlMode.Edit == this.ControlMode)
                {
                    ArrayList entities = new ArrayList();
                    PickerEntity entity = new PickerEntity();
                    entity.Key = (string)this.ItemFieldValue;
                    entities.Add(entity);
                    pickerEditor.UpdateEntities(entities);
                }

                this.Controls.Add(pickerEditor);
            }
            //else
            //{
            //    Literal pickerValue = new Literal();

            //    try
            //    {
            //        string val = this.ItemFieldValue.ToString();

            //        pickerValue.Text = val.Replace(';', ' ');
            //    }
            //    catch (Exception)
            //    {
            //        pickerValue.Text = string.Empty;
            //    }

            //    this.Controls.Add(pickerValue);
            //}
        }

        public override void Validate()
        {
            base.Validate();

            if (ControlMode == SPControlMode.Display || !IsValid)
                return;

            if (Field.Required && pickerEditor.ResolvedEntities.Count > 0)
            {
                this.ErrorMessage = Field.Title + " must have a value.";
                IsValid = false;
                return;
            }

            IsValid = true;
        }

        public override void UpdateFieldValueInItem()
        {
            ItemFieldValue = Value;
        }

        //public override object ItemFieldValue
        //{
        //    get
        //    {
        //        return base.ItemFieldValue;
        //    }
        //    set
        //    {
        //        base.ItemFieldValue = value;
        //        SetFieldControlValue(value);
        //    }
        //}

        //public override void UpdateFieldValueInItem()
        //{
        //    if (pickerEditor != null)
        //    {
        //        if (pickerEditor.Entities.Count >= 1)
        //        {
        //            int searchId = 0;
        //            if (int.TryParse(((PickerEntity)pickerEditor.Entities[0]).Key, out searchId))
        //            {
        //                SPFieldLookupValue searchValue = new SPFieldLookupValue(searchId, ((PickerEntity)pickerEditor.Entities[0]).DisplayText);

        //                ItemFieldValue = searchValue;
        //                return;
        //            }
        //        }
        //    }

        //    base.UpdateFieldValueInItem();
        //}

        //private void SetFieldControlValue(object value)
        //{
        //    if (value == null || value.ToString() == string.Empty)
        //        return;

        //    PickerEntity entity = new PickerEntity();

        //    entity.Key = value.ToString();
        //    entity.IsResolved = true;

        //    ArrayList entities = new ArrayList();
        //    entities.Add(entity);

        //    pickerEditor.UpdateEntities(entities);
        //}

        public override object Value
        {
            get
            {
                this.EnsureChildControls();

                StringBuilder selectedItems = null;
                if (this.pickerEditor != null)
                {
                    if (this.pickerEditor.ResolvedEntities.Count > 0)
                    {
                        selectedItems = new StringBuilder();
                        foreach (PickerEntity entity in pickerEditor.ResolvedEntities)
                        {
                            string val = entity.EntityData[entity.Key] as string;
                            selectedItems.AppendFormat("{0};", val);
                        }
                        if(selectedItems.Length >= 1 && selectedItems[selectedItems.Length-1] == ';')
                            selectedItems.Remove(selectedItems.Length - 1, 1);
                    }
                }

                if (selectedItems == null)
                {
                    pickerEditor.IsValid = false;
                    return null;
                }

                return selectedItems.ToString();
            }
            set
            {
                this.EnsureChildControls();

                string val = value.ToString();

                string[] selectedItems = val.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                ArrayList entities = new ArrayList();

                foreach (var selectedItem in selectedItems)
                {
                    PickerEntity entity = new PickerEntity
                    {
                        Key = selectedItem,
                        IsResolved = true,
                        DisplayText = selectedItem,
                        Description = selectedItem
                    };

                    entity.EntityData.Add(entity.Key, entity.DisplayText);
                    entities.Add(entity);
                }

                pickerEditor.UpdateEntities(entities);
            }
        }

    }
}
