using BeatmapExporter.Exporters.Lazer.LazerDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using static BeatmapExporterGUI.ViewModels.List.BeatmapSorting;

namespace BeatmapExporterGUI.ViewModels.List
{
    public class BeatmapSorting
    {
        public enum SortBy
        {
            ID, // beatmap ID
            Artist, // song artist name
            DateAdded, // beatmap set added to lazer date
            Count, // number of beatmaps in set
            Title, // song title
            Author, // mapper name
            Length, // song length 
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
            SortBy.DateAdded => "Date Added",
            SortBy.Count => "# Beatmaps",
            SortBy.Title => "Song Title",
            SortBy.Author => "Mapper Name",
            SortBy.Length => "Song Length"
        };

        public static Comparison<BeatmapSet> Comparer(this SortBy sort) => sort switch
        {
            SortBy.ID => SetComparison(b => b.OnlineID),
            SortBy.Artist => SetComparison(b => b.DiffMetadata?.Artist),
            SortBy.DateAdded => SetComparison(b => b.DateAdded),
            SortBy.Count => SetComparison(b => b.Beatmaps.Count),
            SortBy.Title => SetComparison(b => b.DiffMetadata?.Title),
            SortBy.Author => SetComparison(b => b.DiffMetadata?.Author?.Username),
            SortBy.Length => SetComparison(b => b.Beatmaps.Select(diff => diff.Length).Max()),
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
