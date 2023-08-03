using ChinookDataAccess.Models;
using System;
using System.Collections.Generic;

namespace ChinookDataAccess.ClientModels
{
    public partial class ArtistClient
    {
        public ArtistClient()
        {
            Albums = new HashSet<Album>();
        }

        public long ArtistId { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<Album> Albums { get; set; }
    }
}
