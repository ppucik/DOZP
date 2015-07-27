using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comdat.DOZP.Core
{
	public class FileSumItem
	{
		#region Private variables
		string _caption = "";
		string _key = "";
		string _navigateUrl = "";
		string _comment = "";
		int _year = 0;
		int _month = 0;
		int _day = 0;
		int _frontCoverScanned = 0;
		int _tableOfContentsScanned = 0;
		//int _tableOfContentsUnprocessed = 0;
		int _tableOfContentsInProgress = 0;
		int _tableOfContentsDiscarded = 0;
		int _frontCoverComplete = 0;
		int _tableOfContentsComplete = 0;
		int _frontCoverExported = 0;
		int _tableOfContentsExported = 0;
		int _books = 0;
		int _pages = 0;
		TimeSpan _ocrTime = TimeSpan.Zero;
		#endregion

		#region Constructors

		public FileSumItem()
		{
			_key = "00000000";
			_caption = "Počet záznamů celkem";
			_navigateUrl = "Overview.aspx";
		}

		public FileSumItem(string caption, string key)
		{
			_caption = caption;
			_key = key;
			_navigateUrl = String.Format("UserSum.aspx?caption={0}", caption);
		}

		public FileSumItem(string caption, string key, string comment)
			: this(caption, key)
		{
			_comment = comment;
		}

		public FileSumItem(int year)
		{
			_year = year;
			_key = String.Format("{0:0000}0000", year);
			_caption = String.Format("Rok {0}", year);
			_navigateUrl = String.Format("Overview.aspx?year={0}", year);
		}

		public FileSumItem(int year, int month)
		{
			_year = year;
			_month = month;
			_key = String.Format("{0:0000}{1:00}00", year, month);
			_caption = new DateTime(year, month, 1).ToString("MMMM yyyy");
			_navigateUrl = String.Format("Overview.aspx?year={0}&month={1}", year, month);
		}

		public FileSumItem(int year, int month, int day)
		{
			_year = year;
			_month = month;
			_day = day;
			_key = String.Format("{0:0000}{1:00}{2:00}", year, month, day);
			_caption = new DateTime(year, month, day).ToLongDateString();
			_navigateUrl = String.Format("Overview.aspx?year={0}&month={1}&day={2}", year, month, day);
		}

		#endregion

		#region Public properties

		public string Caption
		{
			get { return _caption; }
			set { _caption = value; }
		}

		public string Key
		{
			get { return _key; }
		}

		public int Year
		{
			get { return _year; }
		}

		public int Month
		{
			get { return _month; }
		}

		public int Day
		{
			get { return _day; }
		}

		public int FrontCoverScanned
		{
			get { return _frontCoverScanned; }
			internal set { _frontCoverScanned = value; }
		}

		public int TableOfContentsScanned
		{
			get { return _tableOfContentsScanned; }
			internal set { _tableOfContentsScanned = value; }
		}

		/// <summary>
		/// Neskenované a OCR nezpracované obsahy, je to podmnozina Scanned
		/// </summary>
		//public int Unprocessed
		//{
		//    get { return _unprocessed; }
		//    set { _unprocessed = value; }
		//}

		public int TableOfContentsInProgress
		{
			get { return _tableOfContentsInProgress; }
			set { _tableOfContentsInProgress = value; }
		}

		public int TableOfContentsDiscarded
		{
			get { return _tableOfContentsDiscarded; }
			set { _tableOfContentsDiscarded = value; }
		}

		public int FrontCoverComplete
		{
			get { return _frontCoverComplete; }
			set { _frontCoverComplete = value; }
		}

		public int TableOfContentsComplete
		{
			get { return _tableOfContentsComplete; }
			set { _tableOfContentsComplete = value; }
		}

		public int FrontCoverExported
		{
			get { return _frontCoverExported; }
			set { _frontCoverExported = value; }
		}

		public int TableOfContentsExported
		{
			get { return _tableOfContentsExported; }
			set { _tableOfContentsExported = value; }
		}

		public int Books
		{
			get { return _books; }
		}

		public int Pages
		{
			get { return _pages; }
		}

		public TimeSpan OcrTime
		{
			get { return _ocrTime; }
		}

		//public int TotalSum
		//{
		//    get { return (Scanned + InProgress + Discarded + Complete + Exported); }
		//}

		public string NavigateUrl
		{
			//return _navigateUrl
			get { return String.Empty; }
		}

		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
		}

		#endregion

		#region Public methods

		public override bool Equals(object obj)
		{
			FileSumItem s = (obj as FileSumItem);

			if (s != null)
				return (this.Key == s.Key);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public bool IsDaySunday
		{
			get
			{
				if (this.Year > 0 && this.Month > 0 && this.Day > 0)
					return (new DateTime(this.Year, this.Month, this.Day).DayOfWeek == (int)DayOfWeek.Sunday);
				else
					return false;
			}
		}

		public void SetVaule(PartOfBook partOfBook, StatusCode status, int count)
		{
			switch (status)
			{
				case StatusCode.Scanned:
					switch (partOfBook)
					{
						case PartOfBook.FrontCover:
							FrontCoverScanned = count;
							break;
						case PartOfBook.TableOfContents:
							TableOfContentsScanned = count;
							break;
						default:
							break;
					}
					break;

				case StatusCode.InProgress:
					switch (partOfBook)
					{
						case PartOfBook.FrontCover:
							break;
						case PartOfBook.TableOfContents:
							TableOfContentsInProgress = count;
							break;
						default:
							break;
					}
					break;

				case StatusCode.Discarded:					
					switch (partOfBook)
					{
						case PartOfBook.FrontCover:
							break;
						case PartOfBook.TableOfContents:
							TableOfContentsDiscarded = count;
							break;
						default:
							break;
					}
					break;

				case StatusCode.Complete:
					switch (partOfBook)
					{
						case PartOfBook.FrontCover:
							FrontCoverComplete = count;
							break;
						case PartOfBook.TableOfContents:
							TableOfContentsComplete = count;
							break;
						default:
							break;
					}
					break;

				case StatusCode.Exported:
					switch (partOfBook)
					{
						case PartOfBook.FrontCover:
							FrontCoverExported = count;
							break;
						case PartOfBook.TableOfContents:
							TableOfContentsExported = count;
							break;
						default:
							break;
					}
					break;
			}
		}

		public void SetVaule(PartOfBook partOfBook, StatusCode status, int count, int pages, int? seconds)
		{
			this.SetVaule(partOfBook, status, count);

			this.AddPages(pages);

			if (seconds.HasValue)
			{
				this.AddOcrTime(seconds.Value);
			}
		}

		public void AddBooks(int books)
		{
			_books += books;
		}

		public void AddPages(int pages)
		{
			_pages += pages;
		}

		public void AddOcrTime(int seconds)
		{
			_ocrTime = _ocrTime.Add(TimeSpan.FromSeconds(seconds));
		}

		#endregion
	}
}
