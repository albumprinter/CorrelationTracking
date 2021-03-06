﻿using System.Diagnostics;

namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    internal static class Extensions
    {
        public static string GetTagItem(this Activity activity, string key)
        {
            foreach (var keyValue in activity.Tags)
                if (key == keyValue.Key)
                    return keyValue.Value;
            return null;
        }
    }
}
