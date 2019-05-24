using System.Collections.Concurrent;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;

namespace App1.Droid
{
    internal class CachingNativeBitmapDescriptorFactory : IBitmapDescriptorFactory
    {
        private readonly ConcurrentDictionary<string, AndroidBitmapDescriptor> _cache
            = new ConcurrentDictionary<string, AndroidBitmapDescriptor>();

        public Android.Gms.Maps.Model.BitmapDescriptor ToNative(Xamarin.Forms.GoogleMaps.BitmapDescriptor descriptor)
        {
            var defaultFactory = DefaultBitmapDescriptorFactory.Instance;

            if (!string.IsNullOrEmpty(descriptor.Id))
            {
                var cacheEntry = _cache.GetOrAdd(descriptor.Id, _ => defaultFactory.ToNative(descriptor));
                return cacheEntry;
            }

            return defaultFactory.ToNative(descriptor);
        }
    }
}