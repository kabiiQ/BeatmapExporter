using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using System;
using System.Collections.Generic;
using static BeatmapExporterGUI.ViewModels.List.BeatmapSorting;

namespace BeatmapExporterGUI.ViewModels.List
{
    public class BeatmapSorting
    {
        public enum SortBy
        {
            ID, // sort by beatmap ID
            Artist, // sort by song artist name
        }

        public static IEnumerable<SortBy> AllSortOptions => ((SortBy[])Enum.GetValues(typeof(SortBy)));

        public enum View { Selected, All }

        public static IEnumerable<View> AllDisplayOptions => (View[])Enum.GetValues(typeof(View));

        internal delegate IComparable? ComparedProperty(BeatmapSet set);

        internal static Comparison<BeatmapSet> SetComparison(ComparedProperty prop) => (x, y) => prop(x)?.CompareTo(prop(y)) ?? 0;
    }

    public static class SortExtension
    {
        public static string FullName(this SortBy sort) => sort switch
        {
            SortBy.ID => "Beatmap ID",
            SortBy.Artist => "Artist Name",
        };

        public static Comparison<BeatmapSet> Comparer(this SortBy sort) => sort switch
        {
            SortBy.ID => SetComparison(b => b.OnlineID),
            SortBy.Artist => SetComparison(b => b.DiffMetadata?.Artist),
        };
    }

    public static class DisplayExtension
    {
        public static string SetName(this View display) => display switch
        {
            View.Selected => "Selected Beatmaps",
            View.All => "All Beatmaps"
        };

        public static string DiffName(this View display) => display switch
        {
            View.Selected => "Selected Difficulties Only",
            View.All => "All Difficulties"
        };
    }
}
