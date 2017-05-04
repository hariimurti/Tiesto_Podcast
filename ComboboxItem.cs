using System;

namespace Tiesto.Podcast
{
    class ComboboxItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Episode { get; set; }
        public int Duration { get; set; }
        public string Year { get; set; }
        public DateTime Release { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
