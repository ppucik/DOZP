using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Comdat.DOZP.Core
{
    public class EnumItem
    {
        #region Private variables
        private string _name;
        private object _value;
        private string _description;
        #endregion

        #region Constructors

        public EnumItem(string name, object value)
        {
            _name = name;
            _value = value;
        }

        /// <summary>
        /// Konstruktor pro nový objekt enumerátora
        /// </summary>
        /// <param name="value">Hodnota enumerátora</param>
        /// <param name="displayText">Popis enumerátora</param>
        public EnumItem(string name, object value, string displayText)
            : this(name, value)
        {
            _description = displayText;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Název enumerátora
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Hodnota enumerátora
        /// </summary>
        public object Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Popis enumerátora
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        #endregion
    }

    /// <summary>
    /// DisplayName attribute
    /// </summary>
    /// <example>
    /// IEnumerable<DisplayName> attributes = from attribute in typeof(CreditCardName).GetCustomAttributes(true)
    ///                                       let displayName = attribute as DisplayName
    ///                                       where displayName != null
    ///                                       select displayName;
    /// </example>
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayName : Attribute
    {
        public string Title { get; private set; }

        public DisplayName(string title)
        {
            this.Title = title;
        }
    }

    public static class Enumeration
    {
        public static string GetDisplayName(object value)
        {
            if (value == null)
                return String.Empty;
            if (value.GetType().IsEnum == false)
                throw new ArgumentException("Argument is not Enumerator");

            return GetItem(value as Enum).Description;
        }

        public static string GetEnumName(Type enumType)
        {
            if (enumType == null) return null;

            string description = enumType.Name;

            object[] attributes = (DescriptionAttribute[])enumType.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((attributes != null) && (attributes.Length > 0))
            {
                description = ((DescriptionAttribute)attributes[0]).Description;
            }

            return description;
        }

        public static TEnum Parse<TEnum>(string value)
        {
            Type enumType = typeof(TEnum);

            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Typ parametru {0} není enumerátor.", enumType));

            string[] names = enumType.GetEnumNames();
            
            foreach (var name in names)
            {
                if (value.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return (TEnum)Enum.Parse(enumType, name);
            }

            return default(TEnum);
            //return (!String.IsNullOrEmpty(value) ? (TEnum)Enum.Parse(enumType, value, true) : default(TEnum));
        }

        /// <summary>
        /// Zjistí popis hodnoty enumerátora
        /// </summary>
        /// <param name="value">Hodnota enumerátora</param>
        /// <returns>Popis enumerátora</returns>
        public static EnumItem GetItem(Enum value)
        {
            if (value == null) return null;

            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            int number =  Convert.ToInt32(value);
            string description = name;

            MemberInfo[] member = type.GetMember(value.ToString());
            if ((member != null) && (member.Length > 0))
            {
                object[] attributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if ((attributes != null) && (attributes.Length > 0))
                {
                    description = ((DescriptionAttribute)attributes[0]).Description;
                }
            }

            return new EnumItem(name, number, description);
        }

        /// <summary>
        /// Načte seznam hodnot enumerátora, které obsahujú atribut s popisem
        /// </summary>
        /// <param name="enumType">Typ enumerátora</param>
        /// <returns>Seznam popisů hodnot enumerátora</returns>
        public static List<EnumItem> GetList(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Parametr {0} není typ enumerátor.", enumType));

            List<EnumItem> result = new List<EnumItem>();
            MemberInfo[] members = enumType.GetMembers();

            foreach (MemberInfo member in members)
            {
                object[] attributes = member.GetCustomAttributes(typeof(DescriptionAttribute), true);

                if ((attributes != null) && (attributes.Length > 0))
                {
                    string name = member.Name;
                    object value = Enum.Parse(enumType, name);
                    int number = Convert.ToInt32(value);
                    string description = ((DescriptionAttribute)attributes[0]).Description;
                    result.Add(new EnumItem(name, number, description));
                }
            }

            return result;
        }

        /// <summary>
        /// Načte seznam všech hodnot enumerátora
        /// </summary>
        /// <param name="enumTypeName">Názav typu enumerátora</param>
        /// <returns>Seznam hodnot enumerátora</returns>
        public static List<EnumItem> GetList(string enumTypeName)
        {
            if (String.IsNullOrEmpty(enumTypeName))
                throw new ArgumentNullException("enumTypeName");
             
            Type enumType = Type.GetType(enumTypeName);

            if (enumType == null)
                throw new ArgumentException(String.Format("Není možné načíst enumerátor {0}", enumTypeName));
            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Typ parametru {0} není enumerátor.", enumType));

            List<EnumItem> result = new List<EnumItem>();

            string[] names = Enum.GetNames(enumType);
            foreach (var name in names)
            { 
                result.Add(GetItem(Enum.Parse(enumType, name) as Enum));
            }

            return result.OrderBy(e => e.Value).ToList();

            //return GetList(enumType, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<EnumItem> GetList<TEnum>(List<TEnum> values) //where TEnum : IComparable, IFormattable, IConvertible
        {
            if (values == null) return null;

            Type enumType = typeof(TEnum);

            if (!enumType.IsEnum)
                throw new ArgumentException(String.Format("Typ parametru {0} není enumerátor.", enumType));

            List<EnumItem> result = new List<EnumItem>();

            foreach (TEnum value in values)
            {
                result.Add(GetItem(value as Enum));
            }

            return result.OrderBy(e => e.Value).ToList();
        }
    }
}
