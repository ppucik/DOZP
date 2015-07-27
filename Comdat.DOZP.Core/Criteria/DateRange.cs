using System;

namespace Comdat.DOZP.Core
{
    public sealed class DateRange
    {
        #region Private variables
        private DateTime? _from = null;
        private DateTime? _to = null;
        private DateType? _type = null;
        #endregion

        #region Enumerators

        public enum Interval
        {
            None = 0,
            Day = 1,
            Week = 2,
            Month = 3,
            Quarter = 4,
            Year = 5
        }

        public enum DateType
        {
            Undefined = 0,
            Created = 1,
            Modified = 2
        }

        #endregion

        #region Constructors

        public DateRange()
        {
            //_from = DateTime.MinValue;
            //_to = DateTime.MaxValue;
            //_type = DateType.Undefined;
        }

        public DateRange(DateType type)
        {
            _type = type;
        }

        public DateRange(DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("Start date must be before end date");

            _from = from;
            _to = to;
            _type = DateType.Undefined;
        }

        public DateRange(DateTime from, DateTime to, DateType type)
        {
            if (from > to)
                throw new ArgumentException("Start date must be before end date");

            _from = from;
            _to = to;
            _type = type;
        }

        public DateRange(DateTime date, Interval interval, DateType type)
        {
            switch (interval)
            {
                case Interval.None:
                    _from = date;
                    _to = date;
                    break;

                case Interval.Day:
                    _from = date;
                    _to = date;
                    break;

                case Interval.Week:
                    _from = DateTime.Now.Subtract(TimeSpan.FromDays((int)date.DayOfWeek));
                    _to = _from.Value.AddDays(6);
                    break;

                case Interval.Month:
                    _from = new DateTime(date.Year, date.Month, 1);
                    _to = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                    break;

                case Interval.Quarter:
                    if ((date.Month >= 1) && (date.Month <= 3))
                    {
                        _from = new DateTime(date.Year, 1, 1);
                        _to = new DateTime(date.Year, 3, DateTime.DaysInMonth(date.Year, 3));
                    }
                    else if ((date.Month >= 4) && (date.Month <= 6))
                    {
                        _from = new DateTime(date.Year, 4, 1);
                        _to = new DateTime(date.Year, 6, DateTime.DaysInMonth(date.Year, 3));
                    }
                    else if ((date.Month >= 7) && (date.Month <= 9))
                    {
                        _from = new DateTime(date.Year, 7, 1);
                        _to = new DateTime(date.Year, 9, DateTime.DaysInMonth(date.Year, 3));
                    }
                    else
                    {
                        _from = new DateTime(date.Year, 10, 1);
                        _to = new DateTime(date.Year, 12, DateTime.DaysInMonth(date.Year, 12));
                    }
                    break;

                case Interval.Year:
                    _from = new DateTime(date.Year, 1, 1);
                    _to = new DateTime(date.Year, 12, DateTime.DaysInMonth(date.Year, 12));
                    break;

                default:
                    _from = DateTime.MinValue;
                    _to = DateTime.MaxValue;
                    break;
            }

            _type = type;
        }

        public DateRange(int year, int month, int day)
        {
            if (year <= 0)
                throw new ArgumentException("Year must be greater than 0");

            if (month < 0 || month > 12)
                throw new ArgumentException("Month must be between 0 and 12");

            if (month > 0)
            {
                int days = DateTime.DaysInMonth(year, month);
                if (day < 0 || day > days)
                    throw new ArgumentException(String.Format("Day must be between 0 and {0}", days));
            }

            if (month == 0)
            {
                _from = new DateTime(year, 1, 1);
                _to = new DateTime(year, 12, DateTime.DaysInMonth(year, 12));
            }
            else
            {
                if (day == 0)
                {
                    _from = new DateTime(year, month, 1);
                    _to = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                }
                else
                {
                    _from = new DateTime(year, month, day);
                    _to = new DateTime(year, month, day);
                }
            }
        }

        #endregion

        #region Public Properties

        public DateTime? From
        {
            get
            {
                if (_from.HasValue)
                    return new DateTime(_from.Value.Year, _from.Value.Month, _from.Value.Day, 0, 0, 0, 0);
                else
                    return null;
            }
            set
            {
                if ((_to.HasValue) && (value > _to.Value))
                    throw new ArgumentException("Start date must be before end date");

                _from = value;
            }
        }

        public DateTime? To
        {
            get
            {
                if (_to.HasValue)
                    return new DateTime(_to.Value.Year, _to.Value.Month, _to.Value.Day, 23, 59, 59, 999);
                else
                    return null;
            }
            set
            {
                if ((_from.HasValue) && (value < _from.Value))
                    throw new ArgumentException("End date must be after start date");

                _to = value;
            }
        }

        public DateType? Type
        {
            get
            {
                if (_type.HasValue)
                    return _type.Value;
                else if (_from.HasValue || _to.HasValue)
                    return (DateType.Modified);
                else
                    return null;
            }
            set
            {
                _type = value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return ((_from.HasValue == false) && (_to.HasValue == false));
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            string format = "DateRange {0} from {1} to {2}";

            if (From.HasValue && To.HasValue)
                return (String.Format(format, Type.Value.ToString().ToLower(), From.Value.ToShortDateString(), To.Value.ToShortDateString()));
            else if (From.HasValue)
                return (String.Format(format, Type.Value.ToString().ToLower(), From.Value.ToShortDateString(), DateTime.MaxValue.ToShortDateString()));
            else if (To.HasValue)
                return (String.Format(format, Type.Value.ToString().ToLower(), DateTime.MinValue.ToShortDateString(), To.Value.ToShortDateString()));
            else
                return "Empty";
        }

        #endregion
    }
}