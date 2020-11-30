using System;

namespace ADO.Engine.BusinessEntities
{
	public static class DateTimeExtensions
	{
		public static int GetQuarter(this DateTime date)
		{
			return (date.Month + 2) / 3;
		}

		public static int GetFinancialQuarter(this DateTime date, byte fiscalMonthStart)
		{
			if (fiscalMonthStart < 1 || fiscalMonthStart > 12)
				throw new ArgumentException($"Please ensure fiscalMonthStart {fiscalMonthStart} is between 1 and 12");
			return (date.AddMonths(-(fiscalMonthStart-1)).Month + 2) / 3;
		}

		public static int GetSemester(this DateTime date)
		{
			return (date.Month + 5) / 6;
		}

		public static int GetFinancialSemester(this DateTime date, byte fiscalMonthStart)
		{
			if (fiscalMonthStart < 1 || fiscalMonthStart > 12)
				throw new ArgumentException($"Please ensure fiscalMonthStart {fiscalMonthStart} is between 1 and 12");
			return (date.AddMonths(-(fiscalMonthStart - 1)).Month + 5) / 6;
		}

		public static int GetYear(this DateTime date)
		{
			return date.Year;
		}

		public static int GetFinancialYear(this DateTime date, byte fiscalMonthStart)
		{
			if (fiscalMonthStart < 1 || fiscalMonthStart > 12)
				throw new ArgumentException($"Please ensure fiscalMonthStart {fiscalMonthStart} is between 1 and 12");
			if (fiscalMonthStart == 1)
				return GetYear(date);
			return date.Month < fiscalMonthStart ? date.Year : date.Year + 1;
		}
	}
}
