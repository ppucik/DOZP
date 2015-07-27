using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Comdat.DOZP.Core;
using Comdat.DOZP.Data.Repository;

namespace Comdat.DOZP.Data.Business
{
    public class StatisticsComponent
    {
        public FileSumList GetDataByTime(int catalogueID, int? modifiedYear, int? modifiedMonth, int? modifiedDay)
        {
            StatisticsFilter filter = new StatisticsFilter(StatisticsType.TimePeriod);
            filter.CatalogueID = catalogueID;
            filter.Year = modifiedYear;
            filter.Month = modifiedMonth;
            filter.Day = modifiedDay;

            OperationRepository repository = new OperationRepository();
            return repository.GetTimeStatistics(filter);
        }

        public FileSumList GetDataByUser(int catalogueID, int? modifiedYear, int? modifiedMonth, int? modifiedDay, short? partOfBook, bool? useOCR, string userName, int? status)
        {
            StatisticsFilter filter = new StatisticsFilter(StatisticsType.Users);
            filter.CatalogueID = catalogueID;
            filter.Year = modifiedYear;
            filter.Month = modifiedMonth;
            filter.Day = modifiedDay;
            filter.PartOfBook = (PartOfBook?)partOfBook;
            filter.UseOCR = useOCR;
            filter.UserName = userName;
            filter.Status = (StatusCode?)status;

            OperationRepository repository = new OperationRepository();
            return repository.GetUserStatistics(filter);
        }
    }
}
