using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ImageComponents.WPF.Imaging;
using Comdat.DOZP.Core;

namespace Comdat.DOZP.App
{
    public class EnumComboBox : System.Windows.Controls.ComboBox
    {
        private bool _loadOnInit = true;
        private string _enumTypeName = null;

        static EnumComboBox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumComboBox), new FrameworkPropertyMetadata(typeof(EnumComboBox)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (LoadOnInit) FillItems();
        }

        [Browsable(true), Category("Data"), Description("Typ enumarátora")]
        public bool LoadOnInit
        {
            get
            {
                return _loadOnInit;
            }
            set
            {
                _loadOnInit = value;
            }
        }

        [Browsable(true), Category("Data"), Description("Typ enumarátora")]
        public string EnumTypeName
        {
            get
            {
                return _enumTypeName;
            }
            set
            {
                _enumTypeName = value;
            }
        }

        public EnumItem SelectedEnum
        {
            get
            {
                return (this.SelectedItem as EnumItem);
            }
            set
            {
                this.SelectedValue = value;
            }
        }

        public string SelectedName
        {
            get
            {
                //return (SelectedValue != null ? SelectedValue.ToString() : null);
                return (SelectedEnum != null ? SelectedEnum.Name : null);
            }
        }

        private List<EnumItem> CachedValues
        {
            get
            {
                if (App.Current != null && App.Current.Properties != null)
                {
                    return (List<EnumItem>)App.Current.Properties["EnumComboBox_" + EnumTypeName];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (App.Current != null && App.Current.Properties != null)
                {
                    App.Current.Properties["EnumComboBox_" + EnumTypeName] = value;
                }
            }
        }

        public void Load<TEnum>(List<TEnum> values, string selectedValue = null)
        {
            Type enumType = typeof(TEnum);

            if (!EnumTypeName.Contains(enumType.FullName))
                throw new InvalidOperationException(String.Format("Typ enumerátora {0} neopovídá nastevenemu typu.", enumType.Name));

            Clear();

            this.ItemsSource = Enumeration.GetList(values);
            this.DisplayMemberPath = "Description";
            this.SelectedValuePath = "Name";

            if (!String.IsNullOrEmpty(selectedValue))
                SelectedValue = selectedValue;
            else
                this.SelectedIndex = 0;
        }

        public void Load(List<int> values, int selectedValue = 0)
        {
            Clear();

            this.ItemsSource = values;

            if (selectedValue != 0 && values.Contains(selectedValue))
                SelectedValue = selectedValue;
            else
                this.SelectedIndex = 0;
        }

        private void FillItems()
        {
            if (CachedValues == null)
            {
                CachedValues = Enumeration.GetList(EnumTypeName);
            }

            this.ItemsSource = CachedValues;
            this.DisplayMemberPath = "Description";
            this.SelectedValuePath = "Name";
            this.SelectedIndex = 0;
        }

        public void Clear()
        {
            this.ItemsSource = null;
        }
    }
}
