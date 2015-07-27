using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;

namespace Comdat.DOZP.Core
{
    public class EnumItemsSource : Collection<String>, IValueConverter
    {
        Type type;

        IDictionary<Object, Object> valueToNameMap;
        IDictionary<Object, Object> nameToValueMap;

        public Type Type
        {
            get { return this.type; }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("Type is not an enum.", "value");

                this.type = value;

                Initialize();
            }
        }

        void Initialize()
        {
            this.valueToNameMap = this.type
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .ToDictionary(fi => fi.GetValue(null), GetDescription);

            this.nameToValueMap = this.valueToNameMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            Clear();

            foreach (String name in this.nameToValueMap.Keys)
                Add(name);
        }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return this.valueToNameMap[value];
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return this.nameToValueMap[value];
        }

        static Object GetDescription(FieldInfo fieldInfo)
        {
            var descAtt = (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return descAtt != null ? descAtt.Description : fieldInfo.Name;
        }
    }
}
