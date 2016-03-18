using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;

namespace WinRTBase
{
    public static class ExtFunc
    {
        private static Random _randomizer;
        public static Random Randomizer
        {
            get
            {
                if (_randomizer == null)
                    _randomizer = new Random();

                return _randomizer;
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> e)
        {
            var retVal = new ObservableCollection<T>();
            foreach (var i in e)
                retVal.Add(i);
            return retVal;
        }
        public static Collection<T> ToCollection<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return null;
            else
                return new Collection<T>(list.ToList());
        }

        public static Visibility ToVisibility(this bool visible)
        {
            return (visible ? Visibility.Visible : Visibility.Collapsed);
        }

        public static void DelayedInvoke(Action action, int ms)
        {
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromMilliseconds(ms);
            t.Tick += (object sender, object e) =>
            {
                action.Invoke();
                t.Stop();
            };
            t.Start();
        }
        public static void InvokeDelayed(this Action action, int ms)
        {
            DelayedInvoke(action, ms);
        }
    }
}