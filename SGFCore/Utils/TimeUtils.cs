/*
 * Copyright (C) 2018 Slicol Tang. All rights reserved.
 * 
 * TimeUtils
 * 
 * Licensed under the MIT License (the "License"); 
 * you may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, 
 * software distributed under the License is distributed on an "AS IS" BASIS, 
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. 
 * See the License for the specific language governing permissions and limitations under the License.
*/

using System;


namespace SGF.Utils
{

	public class TimeUtils
	{
     

		readonly static DateTime DateTime_1970_01_01_08_00_00 = new DateTime(1970, 1, 1, 8, 0, 0);

		//自 1970 年 1 月 1 日午夜 12:00:00 经过的毫秒数
		public static double GetTotalMillisecondsSince1970()
		{
			DateTime nowtime = DateTime.Now.ToLocalTime();
			return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalMilliseconds;
		}

		public static double GetTotalSecondsSince1970()
		{
			DateTime nowtime = DateTime.Now.ToLocalTime();
			return nowtime.Subtract(DateTime_1970_01_01_08_00_00).TotalSeconds;
		}


		public static TimeSpan GetTimeSpanSince1970()
		{
			return DateTime.Now.Subtract(DateTime_1970_01_01_08_00_00);
		}

		public static string FormatShowTime(ulong timeInSec)
		{
			string _text = "";
			ulong showTime;
			if ((timeInSec / (ulong)86400) > 0)
			{
				showTime = timeInSec / (ulong)86400;
				_text = showTime.ToString() + "天";
			}
			else if ((timeInSec / (ulong)3600) > 0)
			{
				showTime = timeInSec / (ulong)3600;
				_text = showTime.ToString() + "小时";
			}
			else if ((timeInSec / (ulong)60) > 0)
			{
				showTime = timeInSec / (ulong)60;
				_text = showTime.ToString() + "分钟";
			}
			else
			{
				// 对1分钟进行特殊处理, 30秒 和 1分30秒 均为1分钟
				_text = "1分钟";
			}
			return _text;
		}




		//--------------------------------------------------------------------------------
		public const uint OnDaySecond = 24 * 60 * 60;
		public const uint OnHourSecond = 60 * 60;


		public static string GetTimeString(string format, long seconds)
		{
			string label = format;
			int ms = (int)(seconds * 1000);
			int s = (int)seconds;
			int m = s / 60;
			int h = m / 60;
			int d = h / 24;

			string t = "";
			//处理天
			if (label.IndexOf("%dd") >= 0)
			{
				t = d >= 10 ? d.ToString() : ("0" + d.ToString());
				label = label.Replace("%dd", t);
				h = h % 24;
			}
			else if (label.IndexOf("%d") >= 0)
			{
				label = label.Replace("%d", d.ToString());
				h = h % 24;
			}

			//处理小时
			if (label.IndexOf("%hh") >= 0)
			{
				t = h >= 10 ? h.ToString() : ("0" + h.ToString());
				label = label.Replace("%hh", t);
				m = m % 60;
			}
			else if (label.IndexOf("%h") >= 0)
			{
				label = label.Replace("%h", h.ToString());
				m = m % 60;
			}

			//处理分
			if (label.IndexOf("%mm") >= 0)
			{
				t = m >= 10 ? m.ToString() : ("0" + m.ToString());
				label = label.Replace("%mm", t);
				s = s % 60;
			}
			else if (label.IndexOf("%m") >= 0)
			{
				label = label.Replace("%m", m.ToString());
				s = s % 60;
			}

			//处理秒
			if (label.IndexOf("%ss") >= 0)
			{
				t = s >= 10 ? s.ToString() : ("0" + s.ToString());
				label = label.Replace("%ss", t);
				ms = ms % 1000;
			}
			else if (label.IndexOf("%s") >= 0)
			{
				label = label.Replace("%s", s.ToString());
				ms = ms % 1000;
			}

			//处理毫秒
			if (label.IndexOf("ms") >= 0)
			{
				t = ms.ToString();
				label = label.Replace("%ms", t);
			}

			return label;
		}

		/// <summary>
		/// 如果不显示天, 显示hh就会显示所有小时, 但是又怕有地方需要这样, 因此这里新开一下
		/// </summary>
		/// <param name="format"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static string GetTimeStringV2(string format, long seconds)
		{
			string label = format;
			int ms = (int)(seconds * 1000);
			int s = (int)seconds;
			int m = s / 60;
			int h = m / 60;
			int d = h / 24;

			string t = "";
			//处理天
			if (label.IndexOf("%dd") >= 0)
			{
				t = d >= 10 ? d.ToString() : ("0" + d.ToString());
				label = label.Replace("%dd", t);
			}
			else if (label.IndexOf("%d") >= 0)
			{
				label = label.Replace("%d", d.ToString());
			}
			h = h % 24;

			//处理小时
			if (label.IndexOf("%hh") >= 0)
			{
				t = h >= 10 ? h.ToString() : ("0" + h.ToString());
				label = label.Replace("%hh", t);
			}
			else if (label.IndexOf("%h") >= 0)
			{
				label = label.Replace("%h", h.ToString());
			}
			m = m % 60;

			//处理分
			if (label.IndexOf("%mm") >= 0)
			{
				t = m >= 10 ? m.ToString() : ("0" + m.ToString());
				label = label.Replace("%mm", t);
			}
			else if (label.IndexOf("%m") >= 0)
			{
				label = label.Replace("%m", m.ToString());
			}
			s = s % 60;

			//处理秒
			if (label.IndexOf("%ss") >= 0)
			{
				t = s >= 10 ? s.ToString() : ("0" + s.ToString());
				label = label.Replace("%ss", t);
			}
			else if (label.IndexOf("%s") >= 0)
			{
				label = label.Replace("%s", s.ToString());
			}
			ms = ms % 1000;

			//处理毫秒
			if (label.IndexOf("ms") >= 0)
			{
				t = ms.ToString();
				label = label.Replace("%ms", t);
			}

			return label;
		}




		public static DateTime GetLocalTime(uint timeStamp)
		{
			DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
			long lTime = long.Parse(timeStamp + "0000000");
			TimeSpan toNow = new TimeSpan(lTime);
			DateTime dtResult = dtStart.Add(toNow);
			return dtResult;
		}


		private static uint DAY_PER_YEAR = 365;
		private static uint DAY_PER_MONTH = 30;
		private static uint DAY_PER_WEEK = 7;
		public static string DateStringFromNow(DateTime dt)
		{
			TimeSpan span = DateTime.Now - dt;

			double year = (span.TotalDays / DAY_PER_YEAR);
			double month = (span.TotalDays / DAY_PER_MONTH);
			double week = (span.TotalDays / DAY_PER_WEEK);

			if (year > 1)
			{
				return string.Format("{0}年前", (int)System.Math.Floor(year));
			}
			else if (month > 1)
			{
				return string.Format("{0}个月前", (int)System.Math.Floor(month));
			}
			else if (week > 1)
			{
				return string.Format("{0}周前", (int)System.Math.Floor(week));
			}
			else if (span.TotalDays > 1)
			{
				return string.Format("{0}天前", (int)System.Math.Floor(span.TotalDays));
			}
			else if (span.TotalHours > 1)
			{
				return string.Format("{0}小时前", (int)System.Math.Floor(span.TotalHours));
			}
			else if (span.TotalMinutes > 1)
			{
				return string.Format("{0}分钟前", (int)System.Math.Floor(span.TotalMinutes));
			}
			else
			{
				return
					//string.Format("{0}秒前", (int)Math.Floor(span.TotalSeconds));
					"刚才";
			}
		}


	    public static double GetMillisecondsFromTicks(long ticks)
	    {
	        return ticks / 10000.0f;
	    }

	    public static double GetSecondsFromTicks(long ticks)
	    {
	        return (ticks / 10000.0f)/1000.0f;
	    }

    }
}
