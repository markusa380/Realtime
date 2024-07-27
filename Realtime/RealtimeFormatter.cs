using System;
using System.Collections.Generic;

namespace Realtime
{
    internal class RealtimeFormatter : IDateTimeFormatter
    {
        private readonly IDateTimeFormatter defaultFormatter =
            new KSPUtil.DefaultDateTimeFormatter();

        private readonly HashSet<string> seenInvocations = new HashSet<string>();

        public int Minute => IsEnabled() ? 60 : defaultFormatter.Minute;

        public int Hour => IsEnabled() ? Minute * 60 : defaultFormatter.Hour;

        public int Day => IsEnabled() ? Hour * 24 : defaultFormatter.Day;

        public int Year => IsEnabled() ? Day * 365 : defaultFormatter.Year;

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            if (GetBaseTime(out DateTimeOffset baseTime))
            {
                var dateTime = baseTime.AddSeconds(time);
                var dateTimeLocalized = DateTimeUtil.Localize(
                    dateTime,
                    RealtimeConfig.Instance.useLocalTime
                );

                if (includeTime)
                {
                    if (includeSeconds)
                    {
                        return dateTimeLocalized.ToString("yyyy-MM-dd HH:mm:sszzz");
                    }
                    else
                    {
                        return dateTimeLocalized.ToString("yyyy-MM-dd HH:mmzzz");
                    }
                }
                else
                {
                    return dateTimeLocalized.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                return LogInvocation(
                    defaultFormatter.PrintDate(time, includeTime, includeSeconds),
                    "PrintDate",
                    includeTime,
                    includeSeconds
                );
            }
        }

        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            return PrintDate(time, includeTime, includeSeconds);
        }

        public string PrintDateDelta(
            double time,
            bool includeTime,
            bool includeSeconds,
            bool useAbs
        )
        {
            return LogInvocation(
                defaultFormatter.PrintDateDelta(time, includeTime, includeSeconds, useAbs),
                "PrintDateDelta",
                includeTime,
                includeSeconds,
                useAbs
            );
        }

        public string PrintDateDeltaCompact(
            double time,
            bool includeTime,
            bool includeSeconds,
            bool useAbs
        )
        {
            return LogInvocation(
                defaultFormatter.PrintDateDeltaCompact(time, includeTime, includeSeconds, useAbs),
                "PrintDateDeltaCompact",
                includeTime,
                includeSeconds,
                useAbs
            );
        }

        public string PrintDateDeltaCompact(
            double time,
            bool includeTime,
            bool includeSeconds,
            bool useAbs,
            int interestedPlaces
        )
        {
            return LogInvocation(
                defaultFormatter.PrintDateDeltaCompact(
                    time,
                    includeTime,
                    includeSeconds,
                    useAbs,
                    interestedPlaces
                ),
                "PrintDateDeltaCompact",
                includeTime,
                useAbs,
                interestedPlaces
            );
        }

        public string PrintDateNew(double time, bool includeTime)
        {
            return LogInvocation(
                defaultFormatter.PrintDateNew(time, includeTime),
                "PrintDateNew",
                includeTime
            );
        }

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
        {
            return LogInvocation(
                defaultFormatter.PrintTime(time, valuesOfInterest, explicitPositive),
                "PrintTime",
                valuesOfInterest,
                explicitPositive
            );
        }

        public string PrintTime(
            double time,
            int valuesOfInterest,
            bool explicitPositive,
            bool logEnglish
        )
        {
            return LogInvocation(
                defaultFormatter.PrintTime(time, valuesOfInterest, explicitPositive, logEnglish),
                "PrintTime",
                valuesOfInterest,
                explicitPositive,
                logEnglish
            );
        }

        public string PrintTimeCompact(double time, bool explicitPositive)
        {
            return LogInvocation(
                defaultFormatter.PrintTimeCompact(time, explicitPositive),
                "PrintTimeCompact",
                explicitPositive
            );
        }

        public string PrintTimeLong(double time)
        {
            return LogInvocation(defaultFormatter.PrintTimeLong(time), "PrintTimeLong", time);
        }

        public string PrintTimeStamp(double time, bool days = false, bool years = false)
        {
            return LogInvocation(
                defaultFormatter.PrintTimeStamp(time, days, years),
                "PrintTimeStamp",
                days,
                years
            );
        }

        public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            return LogInvocation(
                defaultFormatter.PrintTimeStampCompact(time, days, years),
                "PrintTimeStampCompact",
                days,
                years
            );
        }

        private bool GetBaseTime(out DateTimeOffset baseTime)
        {
            if (IsEnabled())
            {
                baseTime = RealtimeConfig.Instance.baseTime.Value;
                return true;
            }
            else
            {
                baseTime = default;
                return false;
            }
        }

        private bool IsEnabled()
        {
            return RealtimeConfig.Instance != null && RealtimeConfig.Instance.baseTime.HasValue;
        }

        private T LogInvocation<T>(T value, params object[] args)
        {
            string argsStr = string.Join(", ", args);
            if (seenInvocations.Add(argsStr))
            {
                Logging.Info(
                    $"RealtimeFormatter invoked: {argsStr}, which returns {value} by default"
                );
            }

            return value;
        }
    }
}
