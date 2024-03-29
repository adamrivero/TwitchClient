﻿using System;
using System.Runtime.InteropServices;

using Windows.ApplicationModel.Resources;

namespace TwitchClient.Helpers
{
    internal static class ResourceExtensions
    {
        private static ResourceLoader resLoader = new ResourceLoader();

        public static string GetLocalized(this string resourceKey)
        {
            return resLoader.GetString(resourceKey);
        }
    }
}
