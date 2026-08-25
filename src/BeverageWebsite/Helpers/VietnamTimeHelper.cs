using System;

namespace BeverageWebsite.Helpers
{
    /// <summary>
    /// Provides a single, server-independent conversion from stored UTC timestamps
    /// to Vietnam local time.
    /// </summary>
    public static class VietnamTimeHelper
    {
        private const string WindowsTimeZoneId = "SE Asia Standard Time";
        private const string IanaTimeZoneId = "Asia/Ho_Chi_Minh";
        private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

        /// <summary>
        /// Converts a UTC timestamp to Vietnam local time.
        /// Database values whose <see cref="DateTime.Kind"/> is
        /// <see cref="DateTimeKind.Unspecified"/> are treated as UTC because order
        /// timestamps are persisted in UTC.
        /// </summary>
        /// <param name="utcDateTime">The UTC timestamp to convert.</param>
        /// <returns>The corresponding date and time in Vietnam.</returns>
        public static DateTime FromUtc(DateTime utcDateTime)
        {
            DateTime normalizedUtc;

            if (utcDateTime.Kind == DateTimeKind.Local)
            {
                normalizedUtc = utcDateTime.ToUniversalTime();
            }
            else if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                normalizedUtc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }
            else
            {
                normalizedUtc = utcDateTime;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(normalizedUtc, VietnamTimeZone);
        }

        /// <summary>
        /// Gets the current date and time in Vietnam, derived from UTC.
        /// </summary>
        public static DateTime VietnamNow
        {
            get { return FromUtc(DateTime.UtcNow); }
        }

        /// <summary>
        /// Gets the current Vietnam calendar date at midnight.
        /// </summary>
        public static DateTime VietnamToday
        {
            get { return VietnamNow.Date; }
        }

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            var identifiers = new[] { WindowsTimeZoneId, IanaTimeZoneId };

            foreach (var identifier in identifiers)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(identifier);
                }
                catch (TimeZoneNotFoundException)
                {
                    // Try the next platform-specific identifier.
                }
                catch (InvalidTimeZoneException)
                {
                    // Try the next identifier, then fall back to fixed UTC+07:00.
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone(
                "UTC+07:00",
                TimeSpan.FromHours(7),
                "Vietnam Standard Time",
                "Vietnam Standard Time");
        }
    }
}
