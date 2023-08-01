using ChinookDataAccess.ClientModels;
using ChinookDataAccess.Contexts;
using ChinookDataAccess.Models;
using ChinookDataAccess.Pages.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChinookDataAccess.Pages
{
    public class ArtistPageData : IArtistPage
    {

        private readonly IDbContextFactory<ChinookContext> _dbContextFactory;

        public ArtistPageData(IDbContextFactory<ChinookContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        public async Task<List<PlaylistTrack?>> getTrackList(int ArtistId,string CurrentUserId)
        {
            var DbContext = await _dbContextFactory.CreateDbContextAsync();
            var Tracks = DbContext.Tracks.Where(a => a.Album.ArtistId == ArtistId)
            .Include(a => a.Album)
            .Select(t => new PlaylistTrack()
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == "Favorites")).Any()
            })
            .ToList();

            return Tracks;
        }
        public async Task<Artist?>getArtistById(int artistId)
        {
            var DbContext = await _dbContextFactory.CreateDbContextAsync();
            return DbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId);
        }
    }
}
